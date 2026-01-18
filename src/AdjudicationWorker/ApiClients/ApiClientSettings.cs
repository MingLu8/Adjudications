public class ApiClientSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string RouteTemplate { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;

    public int RetryCount { get; set; }
    public int TimeoutSeconds { get; set; }

    public int HedgedAttempts { get; set; }
    public int HedgingDelayMs { get; set; }

    public int BulkheadLimit { get; set; }
    public int BulkheadQueueLimit { get; set; }

    public bool EnableRequestLogging { get; set; }
}
