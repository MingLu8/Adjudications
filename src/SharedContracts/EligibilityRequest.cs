using SharedContracts;

namespace AdjudicationWorker.ApiClients
{
    public class EligibilityRequest : RequestBase
    {
        public EligibilityRequest()
        {
            
        }
        public EligibilityRequest(string transactionId)
        {
            TransactionId = transactionId;
        }

        public string Bin { get; set; } = string.Empty;
        public string Pcn { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;

        public DateTime ClaimDate { get; set; }
    }
}
