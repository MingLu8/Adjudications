namespace DeadClaimCleaner;

public class RedisSettings
{
    public string ConnectionString { get; set; } = default!;
    public string StreamName { get; set; } = "pharmacy-claims";
    public string ConsumerGroup { get; set; } = "adjudication-group";
    public string ConsumerNamePrefix { get; set; } = "DeadClaimCleaner";
    public int DeadClaimAgeInSeconds { get; set; } = 300;

    public string ConsumerName
    {
        get => field ??= $"{ConsumerNamePrefix}-{Environment.MachineName}-{Guid.NewGuid().ToString("N")[..6]}";
        set => field = value;
    }
}
