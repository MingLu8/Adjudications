
namespace AdjudicationWorker
{
    public class ConsumerMessageException : Exception
    {
        public ConsumerMessageException()
        {
        }

        public ConsumerMessageException(string? message) : base(message)
        {
        }

        public ConsumerMessageException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }
}