using Microsoft.Extensions.DependencyInjection;

namespace ConfigManager.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<ConfigurationService>();
            return services;
        }
    }
}
