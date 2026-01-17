using AdjudicationWorker;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

// --- 1. BIND CONFIGURATION ---
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));

// Register as flat objects for clean injection
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RedisSettings>>().Value);
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<KafkaSettings>>().Value);


// --- 2. INFRASTRUCTURE SETUP ---

// Redis Setup
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisSettings = sp.GetRequiredService<RedisSettings>();
    return ConnectionMultiplexer.Connect(redisSettings.ConnectionString);
});

// Kafka Consumer Setup
builder.Services.AddSingleton<IConsumer<Ignore, string>>(sp =>
{
    var kafkaSettings = sp.GetRequiredService<KafkaSettings>();
    var config = new ConsumerConfig
    {
        BootstrapServers = kafkaSettings.BootstrapServers,
        GroupId = "pharmacy-claims-worker-group",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false, // Manual commits for reliability
        SessionTimeoutMs = 45000
    };
    return new ConsumerBuilder<Ignore, string>(config).Build();
});


// --- 3. HOSTED SERVICES ---
builder.Services.AddHostedService<ClaimWorker>();

var host = builder.Build();
host.Run();