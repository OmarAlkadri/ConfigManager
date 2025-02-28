using ConfigManager.Domain.Repositories;
using ConfigManager.Domain.Interfaces;
using ConfigManager.Infrastructure.Messaging;
using ConfigManager.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ConfigManager.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string applicationName)
        {
            services.AddSingleton<IConfigurationRepository>(provider =>
            {
                var repo = new RedisConfigurationRepository(applicationName);
                Task.Run(() => repo.SeedDataAsync()); // تشغيل SeedData عند بدء التشغيل
                return repo;
            });

            services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();
            services.AddTransient<ConfigurationService>();

            return services;
        }
    }
}
