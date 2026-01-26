namespace AacApi.Abstractions;

public interface ICsvService
{
    Task WriteRecordsToStreamAsync<T>(IEnumerable<T> records, Stream stream);
}
