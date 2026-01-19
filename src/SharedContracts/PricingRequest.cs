using SharedContracts;

namespace AdjudicationWorker.ApiClients
{
    public class PricingRequest : RequestBase
    {
        public PricingRequest()
        {
            
        }
        public PricingRequest(string transactionId)
        {
            TransactionId = transactionId;
        }
    }
}
