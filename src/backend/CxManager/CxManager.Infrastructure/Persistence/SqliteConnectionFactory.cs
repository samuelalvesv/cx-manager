using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace CxManager.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        SqliteConnection connection = new(connectionString: connectionString);
        await connection.OpenAsync(cancellationToken: cancellationToken);

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        await command.ExecuteNonQueryAsync(cancellationToken: cancellationToken);

        return connection;
    }
}
