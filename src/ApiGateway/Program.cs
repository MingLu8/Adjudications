using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using StackExchange.Redis;
using SharedContracts;
using ApiGateway;
using Microsoft.AspNetCore.OpenApi; // Add this using directive at the top of the file

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 1. SETUP SINGLETONS ---
// The "Parking Lot" for waiting HTTP requests
var responseMap = new ConcurrentDictionary<string, TaskCompletionSource<ClaimResponse>>();
builder.Services.AddSingleton(responseMap);

// Redis Connection (The Bridge)
var redisMux = ConnectionMultiplexer.Connect("localhost:6379,password=redis123");
builder.Services.AddSingleton<IConnectionMultiplexer>(redisMux);

// Kafka Producer (The Request Sender)
var producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };
builder.Services.AddSingleton<IProducer<Null, string>>(new ProducerBuilder<Null, string>(producerConfig).Build());

// --- 2. THE EGRESS BRIDGE (Background Listener) ---
// This runs constantly, listening for ANY worker shouting a response
builder.Services.AddHostedService<EgressBridgeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// --- 3. THE MINIMAL API (Ingress) ---
app.MapPost("/adjudicate", async (HttpContext context,
    IProducer<Null, string> producer,
    ConcurrentDictionary<string, TaskCompletionSource<ClaimResponse>> map) =>
{
    // A. Read the raw NCPDP string
    using var reader = new StreamReader(context.Request.Body);
    var ncpdpString = await reader.ReadToEndAsync();

    // B. Create the Request Object
    var claim = new ClaimRequest { NcpdpPayload = ncpdpString };

    // C. Create the "Promise" (TCS) and store it
    var tcs = new TaskCompletionSource<ClaimResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
    map.TryAdd(claim.TransactionId, tcs);

    // D. Send to Kafka (Fire and Forget)
    var json = JsonSerializer.Serialize(claim);
    await producer.ProduceAsync("pharmacy-claims", new Message<Null, string> { Value = json });

    // E. PAUSE HERE (The "Async Wait")
    // The thread is released. We wait for the Egress Bridge to call SetResult on our TCS.
    // Safety: Add a timeout so we don't hang forever.
    var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(15)));

    if (completedTask == tcs.Task)
    {
        var response = await tcs.Task;
        return Results.Text(response.NcpdpResponsePayload);
    }
    else
    {
        map.TryRemove(claim.TransactionId, out _); // Cleanup
        return Results.StatusCode(504); // Gateway Timeout
    }
})
.WithDescription("Accepts raw NCPDP D.0 string, processes it via Kafka, and returns the response.")
.WithSummary("Adjudicate a Pharmacy Claim");

app.Run();