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
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

DotNetEnv.Env.Load();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var port = builder.Configuration["CONFIG_MANAGER_PORT"] ?? "8081";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

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

// MongoDB Configuration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"];
    return new MongoClient(mongoConnectionString ?? throw new ArgumentNullException("MongoDB ConnectionString is missing."));
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDB:DatabaseName"] ?? throw new ArgumentNullException("MongoDB DatabaseName is missing.");
    return client.GetDatabase(databaseName ?? throw new ArgumentNullException("MongoDB DatabaseName is missing."));
});

// RabbitMQ Configuration
builder.Services.AddSingleton<IConnection>(sp =>
{
    var rabbitMqConfig = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMQ");
    var factory = new ConnectionFactory()
    {
        HostName = rabbitMqConfig["Host"],
        Port = int.Parse(rabbitMqConfig["Port"] ?? "5672"),
        UserName = rabbitMqConfig["Username"],
        Password = rabbitMqConfig["Password"],
        VirtualHost = rabbitMqConfig["VirtualHost"],
        Ssl = new SslOption
        {
            Enabled = bool.Parse(rabbitMqConfig["UseSsl"] ?? "true"),
            ServerName = rabbitMqConfig["Host"],
            AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.None
        }
    };

    return Task.Run(async () => await factory.CreateConnectionAsync()).GetAwaiter().GetResult();
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddMongoDb(sp => new MongoClient(builder.Configuration["MongoDB:ConnectionString"]!), name: "MongoDB")
    .AddRabbitMQ(sp => sp.GetRequiredService<IConnection>(), name: "RabbitMQ");

var mongoConnection = builder.Configuration["MongoDB:ConnectionString"] ?? throw new ArgumentNullException("MongoDB ConnectionString is missing.");
var databaseName = builder.Configuration["MongoDB:DatabaseName"];
var collectionName = builder.Configuration["MongoDB:CollectionName"] ?? "ConfigurationSettings";
var applicationName = builder.Configuration["Application:Name"];

if (string.IsNullOrEmpty(mongoConnection) || string.IsNullOrEmpty(applicationName))
{
    throw new InvalidOperationException("Missing required configuration values.");
}

// Inject Dependencies
builder.Services.AddInfrastructureServices(builder.Configuration, collectionName);
builder.Services.AddApplicationServices(builder.Configuration, collectionName);
builder.Services.AddApiServices();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? 
    new string[] { "http://localhost:5000", "http://localhost:5001", "https://configmanager.onrender.com" };
var corsPolicy = "AllowAllOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();





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

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConfigManager API v1"));
https://configmanager.onrender.com/

app.UseHttpsRedirection();
app.UseCors(corsPolicy);
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.UseAuthorization();
app.UseIpRateLimiting();
app.MapControllers();
app.UseSerilogRequestLogging();

// Health Check Endpoint
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

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}
