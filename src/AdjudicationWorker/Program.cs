using AdjudicationWorker;
using AdjudicationWorker.Extensions;
using FormularyApi;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Core worker dependencies (Kafka, Redis, typed clients, orchestrator, API caller, ActivitySource)
builder.Services.AddAdjudicationWorkerCore(builder.Configuration);
builder.Services.AddGrpcClient<Greeter.GreeterClient>(o =>
{
    o.Address = new Uri("https://localhost:7027/api/v1");
});
// Hosted service stays here
builder.Services.AddHostedService<ClaimWorker>();

// Health checks stay here
builder.Services.AddHealthChecks()
    .AddRedis(
        sp => sp.GetRequiredService<RedisSettings>().ConnectionString,
        name: "redis")
    .AddCheck("kafka", () =>
    {
        try
        {
            return HealthCheckResult.Healthy("Kafka client initialized");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Kafka not healthy", ex);
        }
    });

var app = builder.Build();
app.UseGlobalExceptionHandler();
app.MapHealthChecks("/health");

app.Run();