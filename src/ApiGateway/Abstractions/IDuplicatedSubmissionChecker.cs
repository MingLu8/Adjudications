namespace ApiGateway.Abstractions
{
    public interface IDuplicatedSubmissionChecker
    {
         Task<bool> IsUniqueAsync(string ncpdp, CancellationToken token);
    }

}
