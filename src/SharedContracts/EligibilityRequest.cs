using SharedContracts;
using System.Security.Claims;

namespace AdjudicationWorker.ApiClients
{
    public class EligibilityRequest : RequestBase
    {
        public EligibilityRequest() : base(string.Empty)
        {
            
        }
        public EligibilityRequest(string transactionId, NCPDPClaim claim) : base(transactionId)
        {
            
        }       
        public string Bin { get; set; } = string.Empty;
        public string Pcn { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;

        public DateTime ClaimDate { get; set; }
    }
}
