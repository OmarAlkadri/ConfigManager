using Microsoft.Extensions.DependencyInjection;
using ConfigManager.Domain.Interfaces;
using ConfigManager.Application.Services;
using ConfigManager.Infrastructure.Persistence;
using MongoDB.Driver;
using ConfigManager.Infrastructure.Messaging;

namespace ConfigManager.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string collectionName)
        {
            services.AddScoped<IConfigurationRepository>(provider =>
            {
                var mongoDatabase = provider.GetRequiredService<IMongoDatabase>();
                var messageBroker = new RabbitMqMessageBroker();
                return new MongoDbConfigurationRepository(mongoDatabase, collectionName, messageBroker);
            });

            services.AddTransient<IConfigurationService, ConfigurationService>();
            return services;
        }
    }
}
