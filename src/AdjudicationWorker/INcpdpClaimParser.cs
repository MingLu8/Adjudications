using SharedContracts;

namespace AdjudicationWorker;

public interface INcpdpClaimParser
{
    NCPDPClaim Parse(string ncpdpPayload);
}
