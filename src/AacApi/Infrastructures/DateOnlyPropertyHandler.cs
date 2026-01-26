using RepoDb.Interfaces;
using RepoDb.Options;

namespace AacApi.Infrastructures;

public class DateOnlyPropertyHandler : IPropertyHandler<DateTime?, DateOnly?>
{
    // When reading from SQL (DateTime) to C# (DateOnly)
    public DateOnly? Get(DateTime? input, PropertyHandlerGetOptions options)
    {
        return input.HasValue ? DateOnly.FromDateTime(input.Value) : null;
    }

    // When writing from C# (DateOnly) to SQL (DateTime)
    public DateTime? Set(DateOnly? input, PropertyHandlerSetOptions options)
    {
        return input.HasValue ? input.Value.ToDateTime(TimeOnly.MinValue) : null;
    }
}
