namespace AdjudicationApi.Infrastructures;

using AdjudicationApi.Abstractions;
using SharedKernel.Extensions;
using StackExchange.Redis;

public class DuplicatedSubmissionChecker : IDuplicatedSubmissionChecker
{
    private readonly IDatabase _db;

    public DuplicatedSubmissionChecker(
    IConnectionMultiplexer redis,
    ILogger<DuplicatedSubmissionChecker> logger
    )
    {
        _db = redis.GetDatabase();
    }
    public async Task<bool> IsUniqueAsync(string ncpdp, CancellationToken token)
    {
        var key = ncpdp.GetHash();
        return await _db.StringSetAsync(
            key,
            "locked",
            TimeSpan.FromSeconds(10),
            When.NotExists
        );      
    }
}
