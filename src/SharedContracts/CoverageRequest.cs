using SharedContracts;

namespace AdjudicationWorker.ApiClients
{
    public class CoverageRequest : RequestBase
    {
        public CoverageRequest()
        {
            
        }
        public CoverageRequest(string transactionId)
        {
            TransactionId = transactionId;
        }
    }
}
