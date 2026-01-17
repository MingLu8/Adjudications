namespace AdjudicationWorker;

public class RedisSettings
{
    public string ConnectionString { get; set; } = default!;
    public string ResponseChannel { get; set; } = "pharmacy-responses";
}
