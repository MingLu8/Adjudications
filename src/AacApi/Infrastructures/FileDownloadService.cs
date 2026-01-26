using AacApi.Abstractions;
using Microsoft.IO;
namespace AacApi.Infrastructures;

public class FileDownloadService : IFileDownloadService
{
    private readonly HttpClient _httpClient;

    // MemoryManager avoids LOH fragmentation by pooling memory segments
    private static readonly RecyclableMemoryStreamManager _memoryManager = new();

    public FileDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Downloads a file into memory using streaming to keep memory overhead low.
    /// </summary>
    public async Task<MemoryStream> DownloadToMemoryAsync(string url, CancellationToken ct = default)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);

        // 3. Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // 4. Create a pooled memory stream
        var destination = _memoryManager.GetStream("FileDownload");

        // 5. Stream content directly from the network to our pooled memory
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        await stream.CopyToAsync(destination, ct);

        // Reset position for the consumer
        destination.Position = 0;
        return (MemoryStream)destination;
    }
}
