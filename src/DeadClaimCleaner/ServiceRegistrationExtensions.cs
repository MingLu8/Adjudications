using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
namespace DeadClaimCleaner;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection ConfigureAppDependencies(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Strongly typed settings
        services.AddAppSettings<RedisSettings>(config, "Redis");

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var cfg = sp.GetRequiredService<RedisSettings>();
            return ConnectionMultiplexer.Connect(cfg.ConnectionString);
        });
        return services;
    }

    public static IServiceCollection AddAppSettings<T>(
     this IServiceCollection services,
     IConfiguration config, string sectionName) where T : class
    {
        services.Configure<T>(config.GetSection(sectionName));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<T>>().Value);
        return services;
    }
}

