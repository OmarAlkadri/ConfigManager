using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using ConfigManager.API.DependencyInjection;
using ConfigManager.Application.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using AspNetCoreRateLimit;
using Serilog;
using RabbitMQ.Client;
using MongoDB.Driver;
using HealthChecks.MongoDb;
using ConfigManager.Infrastructure.DependencyInjection;
using ConfigManager.Infrastructure.Persistence;
using Newtonsoft.Json;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"];
    return new MongoClient(mongoConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDB:DatabaseName"];
    return client.GetDatabase(databaseName);
});

builder.Services.AddHealthChecks()
    .AddMongoDb(sp => new MongoClient(builder.Configuration["MongoDB:ConnectionString"]!), name: "MongoDB")
    .AddRabbitMQ(sp =>
    {
        var factory = new ConnectionFactory()
        {
            Uri = new Uri(sp.GetRequiredService<IConfiguration>()?["RabbitMQ:ConnectionString"]!)
        };
        return factory.CreateConnectionAsync();
    }, name: "RabbitMQ");

string? mongoConnection = builder.Configuration["MongoDB:ConnectionString"];
string? databaseName = builder.Configuration["MongoDB:DatabaseName"];
string? collectionName = builder.Configuration["MongoDB:CollectionName"];
string? rabbitMqConnection = builder.Configuration["RabbitMQ:ConnectionString"];
string? applicationName = builder.Configuration["Application:Name"];

if (string.IsNullOrEmpty(mongoConnection) || string.IsNullOrEmpty(rabbitMqConnection) || string.IsNullOrEmpty(applicationName))
{
    throw new InvalidOperationException("Missing required configuration values.");
}

builder.Services.AddInfrastructureServices(collectionName ?? "ConfigurationSettings");
builder.Services.AddApplicationServices(collectionName ?? "ConfigurationSettings");
builder.Services.AddApiServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
var corsPolicy = "AllowAllOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins?? new string[] { "http://localhost:5000" ,"http://localhost:5001"})
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConfigManager API v1"));
}

app.UseHttpsRedirection();
app.UseCors(corsPolicy);

app.UseAuthorization();
app.UseIpRateLimiting();
app.MapControllers();
app.UseSerilogRequestLogging();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = JsonConvert.SerializeObject(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
        }, Formatting.Indented);

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
});

var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
