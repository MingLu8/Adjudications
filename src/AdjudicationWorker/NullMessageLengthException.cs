
namespace AdjudicationWorker
{
    [Serializable]
    internal class NullMessageLengthException : ConsumerMessageException
    {
        public NullMessageLengthException()
        {
        }

        public NullMessageLengthException(string? message) : base(message)
        {
        }

        public NullMessageLengthException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}