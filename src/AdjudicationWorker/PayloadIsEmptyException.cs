
namespace AdjudicationWorker
{
    [Serializable]
    internal class PayloadIsEmptyException : ConsumerMessageException
    {
        public PayloadIsEmptyException()
        {
        }

        public PayloadIsEmptyException(string? message) : base(message)
        {
        }

        public PayloadIsEmptyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}