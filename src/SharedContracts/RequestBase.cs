namespace SharedContracts;

public class RequestBase
{
    public RequestBase(string transactionId)
    {
        TransactionId = transactionId;
    }
    public string TransactionId { get; }
}
