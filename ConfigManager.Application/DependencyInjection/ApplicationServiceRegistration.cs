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
            services.AddSingleton<IMongoDatabase>(provider =>
            {
                var mongoClient = new MongoClient(configuration["MongoDB:ConnectionString"]);
                return mongoClient.GetDatabase(configuration["MongoDB:DatabaseName"]);
            });

            services.AddScoped<IConfigurationRepository>(provider =>
            {
                var rabbitMqConfig = configuration.GetSection("RabbitMQ");
                var host = rabbitMqConfig["Host"] ?? "rabbitmq";
                var port = rabbitMqConfig["Port"] ?? "5672";
                var username = rabbitMqConfig["Username"] ?? "guest";
                var password = rabbitMqConfig["Password"] ?? "guest";

                var rabbitMqUrl = $"amqp://{username}:{password}@{host}:{port}/";
                Console.WriteLine($"[InfrastructureServiceRegistration] RabbitMQ URL: {rabbitMqUrl}");

                var mongoDatabase = provider.GetRequiredService<IMongoDatabase>();
                
                var messageBroker = new RabbitMqMessageBroker(rabbitMqUrl);
                
                return new MongoDbConfigurationRepository(mongoDatabase, collectionName, messageBroker);
            });

            services.AddTransient<IConfigurationService, ConfigurationService>();

            return services;
        }
    }
}
