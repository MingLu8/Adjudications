using ApiGateway.Abstractions;
using ApiGateway.ConfigurationSettings;
using ApiGateway.Infrastructures;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ApiGateway.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddGatewayInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddAppSettings<KafkaSettings>(config, "Kafka");
        services.AddAppSettings<RedisSettings>(config, "Redis");

        services.AddSingleton<IConnectionMultiplexer>(sp => 
        { 
            var cfg = sp.GetRequiredService<RedisSettings>();
            return ConnectionMultiplexer.Connect(cfg.ConnectionString); 
        }); 
        services.AddSingleton(sp => 
        {
            var cfg = sp.GetRequiredService<KafkaSettings>(); 
            return new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = cfg.BootstrapServers }).Build();
        }); 
        services.AddSingleton<IResponseMap, ResponseMap>(); 
       // services.AddSingleton<IClaimProducer, KafkaClaimProducer>(); 
        services.AddSingleton<IClaimProducer, RedisClaimProducer>(); 
        services.AddSingleton<ClaimGatewayService>(); 
        services.AddHostedService<EgressBridgeService>();

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