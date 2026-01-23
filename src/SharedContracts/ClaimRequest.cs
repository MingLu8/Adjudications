namespace SharedContracts;

public class ClaimRequest : RequestBase
{
    public ClaimRequest() : base(string.Empty)
    {
        
    }
    public ClaimRequest(string transactionId, string ncpdpPayload, long receivedAt) : base(transactionId)
    {
        NcpdpPayload = ncpdpPayload;
        ReceivedAt = receivedAt;
    }

    public string NcpdpPayload { get; set; } = string.Empty;
    public long ReceivedAt { get; }
}
