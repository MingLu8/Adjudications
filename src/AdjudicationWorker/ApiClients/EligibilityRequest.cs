namespace AdjudicationWorker.ApiClients
{
    public class EligibilityRequest
    {
        public string Bin { get; set; } = string.Empty;
        public string Pcn { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;

        public DateTime ClaimDate { get; set; }
    }
}
