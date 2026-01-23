
using AdjudicationWorker.ApiClients;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
namespace AdjudicationWorker;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddAdjudicationWorkerCore(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Strongly typed settings
        services.AddAppSettings<KafkaSettings>(config, "Kafka");
        services.AddAppSettings<RedisSettings>(config, "Redis");
        services.AddAppSettings<WorkerSettings>(config, "Worker");

        // Core orchestrator + API caller
        services.AddSingleton<ITaskOrchestrator, TaskOrchestrator>();
        services.AddSingleton<IApiCaller, ApiCaller>();
        services.AddSingleton<IClaimConsumer, RedisClaimConsumer>();
        services.AddSingleton<IClaimResponsePublisher, RedisClaimResponsePublisher>();
        services.AddSingleton<INcpdpClaimParser, NcpdpClaimParser>();

        // Typed API clients
        services.AddTypedApiClient<EligibilityApiClient, IEligibilityApiClient, EligibilityApiSettings>(config, "EligibilityApi");

        services.AddTypedApiClient<CoverageApiClient, ICoverageApiClient, CoverageApiSettings>(config, "CoverageApi");

        services.AddTypedApiClient<PricingApiClient, IPricingApiClient, PricingApiSettings>(config, "PricingApi");

        services.AddSingleton<IFormularyApiClient, FormularyApiClient>();

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var cfg = sp.GetRequiredService<RedisSettings>();
            return ConnectionMultiplexer.Connect(cfg.ConnectionString);
        });

        // Kafka consumer
        //services.AddSingleton<IConsumer<Ignore, string>>(sp =>
        //{
        //    var settings = sp.GetRequiredService<KafkaSettings>();

        //    var consumerConfig = new ConsumerConfig
        //    {
        //        BootstrapServers = settings.BootstrapServers,
        //        GroupId = "claim-worker",
        //        AutoOffsetReset = AutoOffsetReset.Earliest,
        //        EnableAutoCommit = false
        //    };

        //    return new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        //});

        //// Kafka DLQ producer
        //services.AddSingleton<IProducer<Null, string>>(sp =>
        //{
        //    var settings = sp.GetRequiredService<KafkaSettings>();

        //    var producerConfig = new ProducerConfig
        //    {
        //        BootstrapServers = settings.BootstrapServers
        //    };

        //    return new ProducerBuilder<Null, string>(producerConfig).Build();
        //});

        //// OpenTelemetry ActivitySource
        //services.AddSingleton(new System.Diagnostics.ActivitySource("AdjudicationWorker"));

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

