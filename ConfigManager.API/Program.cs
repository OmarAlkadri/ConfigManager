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
using ConfigManager.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using AspNetCoreRateLimit;
using Serilog;
using RabbitMQ.Client;

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

builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration["Redis:ConnectionString"]!, name: "Redis")
    .AddRabbitMQ(sp => {
        var factory = new ConnectionFactory()
        {
            Uri = new Uri(sp.GetRequiredService<IConfiguration>()?["RabbitMQ:ConnectionString"]!)
        };
        return factory.CreateConnectionAsync();
    }, name: "RabbitMQ");

string? redisConnection = builder.Configuration["Redis:ConnectionString"];
string? rabbitMqConnection = builder.Configuration["RabbitMQ:ConnectionString"];
string? applicationName = builder.Configuration["Application:Name"];

if (string.IsNullOrEmpty(redisConnection) || string.IsNullOrEmpty(rabbitMqConnection) || string.IsNullOrEmpty(applicationName))
{
    throw new InvalidOperationException("Missing required configuration values.");
}

builder.Services.AddInfrastructureServices(applicationName);
builder.Services.AddApplicationServices();
builder.Services.AddApiServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors(policy =>
    policy.WithOrigins("http://localhost:5000")
          .AllowAnyMethod()
          .AllowAnyHeader());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConfigManager API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseIpRateLimiting();
app.MapControllers();
app.UseSerilogRequestLogging();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString() })
            });
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(result);
    }
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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
