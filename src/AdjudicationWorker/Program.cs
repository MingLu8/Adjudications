using AdjudicationWorker;
using StackExchange.Redis;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // UPDATE THIS LINE
        var redisMux = ConnectionMultiplexer.Connect("localhost:6379,password=redis123");
        services.AddSingleton<IConnectionMultiplexer>(redisMux);

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();