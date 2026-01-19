namespace SharedContracts;
public class ClaimRequest : RequestBase
{
    public string NcpdpPayload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
