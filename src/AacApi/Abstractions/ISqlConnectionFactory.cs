using System.Data;

namespace AacApi.Abstractions;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
