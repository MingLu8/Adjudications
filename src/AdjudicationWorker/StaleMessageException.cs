
namespace AdjudicationWorker
{
    [Serializable]
    internal class StaleMessageException : Exception
    {

        public StaleMessageException()
        {
        }

        public StaleMessageException(string? message) : base(message)
        {
        }

        public StaleMessageException(string transactionId, double ageSeconds)
        {
            TransactionId = transactionId;
            AgeSeconds = ageSeconds;
        }

        public StaleMessageException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public string TransactionId { get; }
        public double AgeSeconds { get; }
    }
}