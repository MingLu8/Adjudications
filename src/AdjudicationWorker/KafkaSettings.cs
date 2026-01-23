namespace AdjudicationWorker;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = default!;
    public string ClaimsTopic { get; set; } = "pharmacy-claims";
    public string DlqTopic { get; set; } = "";
}
