
using AdjudicationWorker.ApiClients;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using SharedKernel.Extensions;

namespace AdjudicationWorker;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddAdjudicationWorkerCore(
        this IServiceCollection services,
        IConfiguration config)
    {
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

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var cfg = sp.GetRequiredService<RedisSettings>();
            var options = ConfigurationOptions.Parse(cfg.ConnectionString);

            // We set these to ensure that even when it eventually connects, it doesn't panic
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 10;
            options.ReconnectRetryPolicy = new ExponentialRetry(5000); // Retry every 5s

            // By using a Lazy wrapper, the 'Connect' method isn't called 
            // until the first time you call GetDatabase()
            var lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                Console.WriteLine($"[Redis] Executing delayed connection to {cfg.ConnectionString}...");
                return ConnectionMultiplexer.Connect(options);
            });

            return lazyConnection.Value;
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
}

