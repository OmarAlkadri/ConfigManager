using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;


namespace ConfigManager.API.DependencyInjection
{
    public static class ApiServiceRegistration
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ConfigManager API", Version = "v1" });
            });

            return services;
        }
    }
}
