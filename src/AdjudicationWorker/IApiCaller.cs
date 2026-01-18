namespace AdjudicationWorker;

public interface IApiCaller
{
    Task<TResponse> PostAsync<TRequest, TResponse>(
        HttpClient client,
        string path,
        TRequest payload,
        CancellationToken token);
}

