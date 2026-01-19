namespace AdjudicationWorker;

public class ApiCaller : IApiCaller
{
    private readonly ILogger<ApiCaller> _logger;

    public ApiCaller(ILogger<ApiCaller> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        HttpClient client,
        string path,
        TRequest payload,
        CancellationToken token)
    {
        var transactionId = ExtractTransactionId(payload);

        if (!string.IsNullOrWhiteSpace(transactionId))
            client.DefaultRequestHeaders.Remove("X-Transaction-Id");

        if (!string.IsNullOrWhiteSpace(transactionId))
            client.DefaultRequestHeaders.Add("X-Transaction-Id", transactionId);

        _logger.LogInformation("POST request started Path={Path} TransactionId={TransactionId} PayloadType={PayloadType} ResponseType={ResponseType}", path, transactionId, typeof(TRequest).Name, typeof(TResponse).Name);

        try
        {
            var response = await client.PostAsJsonAsync(path, payload, token);

            _logger.LogInformation("POST response received Path={Path} TransactionId={TransactionId} StatusCode={StatusCode}", path, transactionId, response.StatusCode);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: token);

            if (result == null)
            {
                _logger.LogError("POST empty response body Path={Path} TransactionId={TransactionId} ExpectedType={ResponseType}", path, transactionId, typeof(TResponse).Name);
                throw new InvalidOperationException("Empty response body");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("POST request canceled Path={Path} TransactionId={TransactionId} PayloadType={PayloadType}", path, transactionId, typeof(TRequest).Name);
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "POST HTTP failure Path={Path} TransactionId={TransactionId} PayloadType={PayloadType}", path, transactionId, typeof(TRequest).Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST unexpected error Path={Path} TransactionId={TransactionId} PayloadType={PayloadType}", path, transactionId, typeof(TRequest).Name);
            throw;
        }
    }

    private static string ExtractTransactionId<TRequest>(TRequest payload)
    {
        var prop = typeof(TRequest).GetProperty("TransactionId");
        return prop?.GetValue(payload)?.ToString() ?? string.Empty;
    }
}

