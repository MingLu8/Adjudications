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
        services.Configure<KafkaSettings>(config.GetSection("Kafka")); 
        services.Configure<RedisSettings>(config.GetSection("Redis")); 
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<KafkaSettings>>().Value); 
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RedisSettings>>().Value); 
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
        services.AddSingleton<IKafkaProducerService, KafkaProducerService>(); 
        services.AddSingleton<ClaimGatewayService>(); 
        services.AddHostedService<EgressBridgeService>();

        return services;
    }
}