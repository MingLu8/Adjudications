using SharedContracts;

namespace AdjudicationWorker;

public class NcpdpClaimParser : INcpdpClaimParser
{
    public NCPDPClaim Parse(string ncpdpPayload)
    {
        return new NCPDPClaim();
    }
}
