using AacApi.Infrastructures;
using SharedContracts;

namespace AacApi.Abstractions
{
    public interface IAacRepository
    {
        Task<Aac?> GetByIdAsync(Guid id);
        Task<Aac?> GetByStateAndNdcAsync(AacState state, string ndc);
        Task<int> SaveAsync(IEnumerable<Aac> records);
    }
}