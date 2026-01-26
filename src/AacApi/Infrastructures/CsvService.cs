using AacApi.Abstractions;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace AacApi.Infrastructures;

public class CsvService : ICsvService
{
    public async Task WriteRecordsToStreamAsync<T>(IEnumerable<T> records, Stream stream)
    {
        // 1. Use 'await using' to ensure disposal is asynchronous
        // 2. LeaveOpen: true is critical so the Response.Body isn't closed by the service
        await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // 3. Write records asynchronously
        await csv.WriteRecordsAsync(records);

        // 4. Manually flush asynchronously before the method ends
        await writer.FlushAsync();
    }
}
