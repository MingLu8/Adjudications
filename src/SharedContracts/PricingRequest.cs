using SharedContracts;

namespace AdjudicationWorker.ApiClients
{
    public class PricingRequest : RequestBase
    {
        public PricingRequest() : base(string.Empty)
        {
            
        }
        public PricingRequest(string transactionId, NCPDPClaim claim) : base(transactionId)
        {
            
        }    
    }
}
