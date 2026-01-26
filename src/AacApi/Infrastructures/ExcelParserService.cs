using AacApi.Abstractions;
using ClosedXML.Excel;
namespace AacApi.Infrastructures;

public class ExcelParserService : IExcelParserService
{
    public List<AacPrice> ParsePricingStream(MemoryStream stream)
    {
        var dataList = new List<AacPrice>();
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // 1. Identify Header Positions Dynamically
        // We look at the first row to find the column numbers
        // row(1) is the title row
        var headerRow = worksheet.Row(2);

        // Find column indices by header text (case-insensitive for safety)
        int ndcColumn = headerRow.CellsUsed()
            .FirstOrDefault(c => c.GetString().Contains("NDC", StringComparison.OrdinalIgnoreCase))
            ?.Address.ColumnNumber ?? -1;

        int aacColumn = headerRow.CellsUsed()
            .FirstOrDefault(c => c.GetString().Contains("AAC", StringComparison.OrdinalIgnoreCase))
            ?.Address.ColumnNumber ?? -1;

        int effectiveDateColumn = headerRow.CellsUsed()
           .FirstOrDefault(c => c.GetString().Contains("Effective Date", StringComparison.OrdinalIgnoreCase))
           ?.Address.ColumnNumber ?? -1;

        // 2. Validation: Ensure both headers were found
        if (ndcColumn == -1 || aacColumn == -1 || effectiveDateColumn == -1)
        {
            throw new Exception($"Required headers not found. Found columns: {string.Join(", ", headerRow.CellsUsed().Select(c => c.GetString()))}");
        }

        // 3. Process Rows using the discovered indices
        // RangeUsed() ensures we don't process empty cells outside the data area
        var rows = worksheet.RangeUsed().RowsUsed().Skip(2); // Skip title and header rows

        foreach (var row in rows)
        {
            var ndc = row.Cell(ndcColumn).GetString();

            if(string.IsNullOrEmpty(ndc))
                throw new Exception($"ndc can not be empty at row {row.RowNumber()}.");

            if (!row.Cell(aacColumn).TryGetValue(out decimal aac))
                throw new Exception($"Invalid AAC value at row {row.RowNumber()}, value: {row.Cell(aacColumn).GetText()}.");

            if (!row.Cell(effectiveDateColumn).TryGetValue(out DateTime effectiveDate))
                throw new Exception($"Invalid effectiveDate value at row {row.RowNumber()}, value: {row.Cell(effectiveDateColumn).GetText()}.");
                
            dataList.Add(new AacPrice(ndc, aac, DateOnly.FromDateTime(effectiveDate)));
        }

        return dataList;
    }
}
