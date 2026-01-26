using AacApi.Abstractions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AacApi.Infrastructures;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        // Your specific logic here:
        _connectionString = configuration.GetConnectionString("Aac")
            ?? throw new InvalidOperationException("Connection string 'Aac' not found.");
    }

    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);
}