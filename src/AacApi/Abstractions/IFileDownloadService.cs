namespace AacApi.Abstractions
{
    public interface IFileDownloadService
    {
        Task<MemoryStream> DownloadToMemoryAsync(string url, CancellationToken ct = default);
    }
}