using AacApi.Abstractions;
using Microsoft.Data.SqlClient;
using RepoDb;
using SharedContracts;
namespace AacApi.Infrastructures;

public class AacRepository(ISqlConnectionFactory connectionFactory) : IAacRepository
{
    public async Task<Aac?> GetByIdAsync(Guid id)
    {
        using var connection = connectionFactory.CreateConnection();
        return (await connection.QueryAsync<Aac>(id)).FirstOrDefault();
    }

    public async Task<Aac?> GetByStateAndNdcAsync(AacState state, string ndc)
    {
        using var connection = connectionFactory.CreateConnection();
        return (await connection.QueryAsync<Aac>(a=> a.State == state && a.Ndc == ndc)).FirstOrDefault();
    }


    public async Task<int> SaveAsync(IEnumerable<Aac> records)
    {
        using var connection = connectionFactory.CreateConnection() as SqlConnection;

        var affectedRows = await connection.BulkMergeAsync(
            entities: records,
            qualifiers: e => new { e.State, e.Ndc }, // Use State and NDC to decide if it's an Update or Insert
            options: SqlBulkCopyOptions.Default,
            batchSize: 10000 // Process in large chunks for speed
        );

        return affectedRows;
    }
}
