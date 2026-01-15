namespace SharedContracts;

public class ClaimResponse
{
    public string TransactionId { get; set; } = string.Empty;
    public string NcpdpResponsePayload { get; set; } = string.Empty;
    public bool Success { get; set; }
}
