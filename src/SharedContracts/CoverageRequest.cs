using SharedContracts;

namespace AdjudicationWorker.ApiClients
{
    public class CoverageRequest : RequestBase
    {
        public CoverageRequest() : base(string.Empty)
        {
            
        }
        public CoverageRequest(string transactionId, NCPDPClaim claim) : base(transactionId)
        {
            
        }
    }
}
