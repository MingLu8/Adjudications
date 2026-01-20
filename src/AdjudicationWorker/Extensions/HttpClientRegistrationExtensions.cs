using Microsoft.Extensions.Options;

namespace AdjudicationWorker;

public static class HttpClientRegistrationExtensions
{
    public static IServiceCollection AddTypedApiClient<TClient, TInterface, TSettings>(
        this IServiceCollection services,
        IConfiguration config,
        string sectionName)
        where TClient : class, TInterface
        where TInterface : class
        where TSettings : class
    {
        // Bind settings
        services.Configure<TSettings>(config.GetSection(sectionName));

        // Expose settings as singleton
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<TSettings>>().Value);

        // Register typed HttpClient
        services.AddHttpClient<TInterface, TClient>()
            .ConfigureHttpClient((sp, http) =>
            {
                var settings = sp.GetRequiredService<TSettings>();
                var baseUrlProp = typeof(TSettings).GetProperty("BaseUrl");

                if (baseUrlProp == null)
                    throw new InvalidOperationException($"{typeof(TSettings).Name} must contain a BaseUrl property.");

                var baseUrl = baseUrlProp.GetValue(settings)?.ToString()
                    ?? throw new InvalidOperationException($"{typeof(TSettings).Name}.BaseUrl is null.");

                http.BaseAddress = new Uri(baseUrl);
            });

        return services;
    }


}

