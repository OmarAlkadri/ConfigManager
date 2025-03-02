using ConfigManager.Domain.Interfaces;
using ConfigManager.Infrastructure.Messaging;
using ConfigManager.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace ConfigManager.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string collectionName)
        {
            services.AddSingleton<IMessageBroker>(provider =>
            {
                var messageBroker = new RabbitMqMessageBroker();
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
