using AacApi.Infrastructures;

namespace AacApi.Abstractions
{
    public interface IAacRepository
    {
        Task<Aac?> GetByIdAsync(Guid id);
        Task<Aac?> GetByStateAndNdcAsync(string state, string ndc);
        Task<int> SaveAsync(IEnumerable<Aac> records);
    }
}