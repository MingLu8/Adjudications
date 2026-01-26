using AacApi.Infrastructures;

namespace AacApi.Abstractions
{
    public interface IExcelParserService
    {
        List<AacPrice> ParsePricingStream(MemoryStream stream);
    }
}