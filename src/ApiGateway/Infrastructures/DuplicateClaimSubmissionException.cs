
namespace ApiGateway
{
    [Serializable]
    internal class DuplicateClaimSubmissionException : Exception
    {
        public DuplicateClaimSubmissionException()
        {
        }

        public DuplicateClaimSubmissionException(string? ncpdpPayload) : base($"duplicated claim: {ncpdpPayload}.")
        {
        }

        public DuplicateClaimSubmissionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}