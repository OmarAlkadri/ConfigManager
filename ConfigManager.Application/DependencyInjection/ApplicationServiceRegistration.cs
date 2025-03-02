using Microsoft.Extensions.DependencyInjection;
using ConfigManager.Domain.Interfaces;
using ConfigManager.Application.Services;
using ConfigManager.Infrastructure.Persistence;
using MongoDB.Driver;
using ConfigManager.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;

namespace ConfigManager.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration, string collectionName)
        {
            // تأكد من أن MongoDB Client يتم تحميله لمرة واحدة فقط عبر الـ DI
            services.AddSingleton<IMongoClient>(provider =>
            {
                var connectionString = configuration["MongoDB:ConnectionString"] ?? throw new ArgumentNullException("MongoDB ConnectionString is missing.");
                return new MongoClient(connectionString);
            });

            services.AddScoped<IMongoDatabase>(provider =>
            {
                var client = provider.GetRequiredService<IMongoClient>();
                var databaseName = configuration["MongoDB:DatabaseName"] ?? throw new ArgumentNullException("MongoDB DatabaseName is missing.");
                return client.GetDatabase(databaseName);
            });

            services.AddScoped<IConfigurationRepository>(provider =>
            {
                var rabbitMqConfig = configuration.GetSection("RabbitMQ");
                var host = rabbitMqConfig["Host"] ?? throw new ArgumentNullException("RabbitMQ Host is missing.");
                var port = int.Parse(rabbitMqConfig["Port"] ?? "5672");
                var username = rabbitMqConfig["Username"] ?? "guest";
                var password = rabbitMqConfig["Password"] ?? "guest";
                var virtualHost = rabbitMqConfig["VirtualHost"] ?? "/";
                var useSsl = bool.Parse(rabbitMqConfig["UseSsl"] ?? "false");

                var rabbitMqUrl = $"amqp://{username}:{password}@{host}:{port}{virtualHost}";
                Console.WriteLine($"[ApplicationServiceRegistration] RabbitMQ URL: {rabbitMqUrl}");

                var mongoDatabase = provider.GetRequiredService<IMongoDatabase>();
                var messageBroker = new RabbitMqMessageBroker(rabbitMqUrl);

                return new MongoDbConfigurationRepository(mongoDatabase, collectionName, messageBroker);
            });

            services.AddTransient<IConfigurationService, ConfigurationService>();

            return services;
        }
    }
}
