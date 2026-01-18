using System.Net.Http.Json;

namespace AdjudicationWorker;

public class ApiCaller : IApiCaller
{
    public async Task<TResponse> PostAsync<TRequest, TResponse>(
        HttpClient client,
        string path,
        TRequest payload,
        CancellationToken token)
    {
        var response = await client.PostAsJsonAsync(path, payload, token);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: token)
               ?? throw new InvalidOperationException("Empty response body");
    }
}

