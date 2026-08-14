using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedKernel.Extensions;

public static class AppSettingsExtensions
{
    /// <summary>
    /// Binds a configuration section to a strongly typed settings class and registers it in the DI container.
    /// </summary>
    /// <typeparam name="T">The settings class type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The application configuration.</param>
    /// <param name="sectionName">The name of the configuration section.</param>
    /// <returns>The bound settings instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services, config, or sectionName is null/empty.</exception>
    public static T AddAppSettings<T>(
        this IServiceCollection services,
        IConfiguration config,
        string sectionName) where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(sectionName, nameof(sectionName));

        var settings = new T();
        config.GetSection(sectionName).Bind(settings);
        services.AddSingleton(settings);
        return settings;
    }
}
