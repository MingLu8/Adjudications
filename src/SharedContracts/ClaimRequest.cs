namespace SharedContracts;
public class ClaimRequest : RequestBase
{
    public ClaimRequest()
    {
        
    }
    public ClaimRequest(string transactionId)
    {
        TransactionId = transactionId;
    }

    public string NcpdpPayload { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string PricingResponse { get; }
}
