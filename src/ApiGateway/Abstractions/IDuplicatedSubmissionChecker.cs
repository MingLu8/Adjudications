namespace ApiGateway.Abstractions
{
    public interface IDuplicatedSubmissionChecker
    {
         Task<bool> IsDuplicateAsync(string ncpdp, CancellationToken token);
    }

}
