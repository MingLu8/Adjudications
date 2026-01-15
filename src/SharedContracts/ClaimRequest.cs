namespace SharedContracts;

public class ClaimRequest
{
    public string TransactionId { get; set; } = Guid.NewGuid().ToString();
    public string NcpdpPayload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
