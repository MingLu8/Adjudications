
using SharedContracts;

namespace AdjudicationWorker
{
    [Serializable]
    internal class MessageFieldNotFoundException : ConsumerMessageException
    {
        public MessageFieldNotFoundException()
        {
        }

        public MessageFieldNotFoundException(string field) : base($"{field} field not found.")
        {
        }

        public MessageFieldNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}