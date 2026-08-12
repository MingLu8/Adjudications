using ApiGateway.Abstractions;
using ApiGateway.ConfigurationSettings;
using ApiGateway.Infrastructures;
using Confluent.Kafka;
using StackExchange.Redis;
using SharedKernel.Extensions;

namespace ApiGateway.Extensions;

public static class DependencyResolutions
{
    public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddAppSettings<ClaimProcessorSettings>(config, "ClaimProcessor");
        services.AddAppSettings<KafkaSettings>(config, "Kafka");
        services.AddAppSettings<RedisSettings>(config, "Redis");

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
        services.AddSingleton(sp => 
        {
            var cfg = sp.GetRequiredService<KafkaSettings>(); 
            return new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = cfg.BootstrapServers }).Build();
        }); 
        services.AddSingleton<IResponseMap, ResponseMap>(); 
       // services.AddSingleton<IClaimProducer, KafkaClaimProducer>(); 
        services.AddSingleton<IDuplicatedSubmissionChecker, DuplicatedSubmissionChecker>(); 
        services.AddSingleton<IClaimQueue, RedisClaimProducer>(); 
        services.AddSingleton<IClaimIngestionService, ClaimIngestionService>();

        //services.AddOptions<ClaimProcessorSettings>()
        //    .Bind(config.GetSection("ClaimProcessor"))
        //    .ValidateOnStart();
        //services.AddSingleton<IValidateOptions<ClaimProcessorSettings>, ClaimProcessorSettingsValidator>();

        services.AddHostedService<ResponseMapSweeper>();
        services.AddHostedService<EgressBridgeService>();

        return services;
    }
}