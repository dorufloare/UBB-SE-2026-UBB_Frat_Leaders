using Microsoft.Data.SqlClient;

namespace matchmaking.Repositories;

public abstract class SqlRepositoryBase
{
    protected readonly string ConnectionString;

    protected SqlRepositoryBase(string connectionString)
    {
        ConnectionString = connectionString;
    }

    protected SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }
}
