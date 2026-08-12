namespace ApiGateway.Infrastructures
{
    [Serializable]
    public sealed class DuplicateTransactionException(string transactionId)
    : Exception($"A pending response entry already exists for TransactionId: {transactionId}")
    {
        public string TransactionId { get; } = transactionId;
    }


    public sealed class ClaimProducerException(string message, Exception innerException)
        : Exception(message, innerException);

}