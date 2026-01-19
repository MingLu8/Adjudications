namespace SharedContracts;

public class RequestBase
{
    public string TransactionId { get; set; } = Guid.NewGuid().ToString();
}
