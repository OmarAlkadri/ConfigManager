using ConfigManager.Domain.Interfaces;
using ConfigManager.Infrastructure.Messaging;
using ConfigManager.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace ConfigManager.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, string collectionName)
    {
        services.AddSingleton<IMessageBroker>(provider =>
        {
            var rabbitMqConfig = configuration.GetSection("RabbitMQ");
            var host = rabbitMqConfig["Host"] ?? "rabbitmq";
            var port = rabbitMqConfig["Port"] ?? "5672";
            var username = rabbitMqConfig["Username"] ?? "guest";
            var password = rabbitMqConfig["Password"] ?? "guest";

            var rabbitMqUrl = $"amqp://{username}:{password}@{host}:{port}/";

            Console.WriteLine($"[InfrastructureServiceRegistration] RabbitMQ URL: {rabbitMqUrl}");

            var messageBroker = new RabbitMqMessageBroker(rabbitMqUrl);
            Task.Run(async () => await messageBroker.InitializeAsync()).Wait();
            return messageBroker;
        });

        services.AddSingleton<IConfigurationRepository>(provider =>
        {
            var mongoDatabase = provider.GetRequiredService<IMongoDatabase>();
            var messageBroker = provider.GetRequiredService<IMessageBroker>();
            return new MongoDbConfigurationRepository(mongoDatabase, collectionName, messageBroker);
        });

        return services;
    }
    }
}
