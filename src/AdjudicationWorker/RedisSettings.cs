namespace AdjudicationWorker;

public class RedisSettings
{
    public string ConnectionString { get; set; } = default!;
    public string ResponseChannel { get; set; } = "claim-responses";
    public int MaxProcessingAgeSeconds { get; set; } = 30;
    public string StreamName { get; set; } = "pharmacy-claims";
    public string ConsumerGroup { get; set; } = "adjudication-group";
    public string ConsumerName { get; set; } = $"worker-{Guid.NewGuid().ToString()[..4]}";
    public int ClaimTimeoutSeconds { get; set; } = 12;
}
