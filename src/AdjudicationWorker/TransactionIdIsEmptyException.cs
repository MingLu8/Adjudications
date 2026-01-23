
namespace AdjudicationWorker
{
    [Serializable]
    internal class TransactionIdIsEmptyException : ConsumerMessageException
    {
        public TransactionIdIsEmptyException()
        {
        }

        public TransactionIdIsEmptyException(string? message) : base(message)
        {
        }

        public TransactionIdIsEmptyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}