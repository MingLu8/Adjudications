
namespace AdjudicationWorker
{
    [Serializable]
    internal class NullMessageLengthException : Exception
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