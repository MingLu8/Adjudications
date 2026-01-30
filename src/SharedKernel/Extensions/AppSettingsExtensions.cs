using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SharedKernel.Extensions
{
    public static class AppSettingsExtensions
    {
        public static IServiceCollection AddAppSettings<T>(this IServiceCollection services, IConfiguration config, string sectionName) where T : class
        {
            services.Configure<T>(config.GetSection(sectionName));
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<T>>().Value);
            return services;
        }
    }
}
