using System.Data.Common;
using CxManager.Infrastructure.Persistence;
using CxManager.Infrastructure.Persistence.Schema;
using Dapper;

namespace CxManager.IntegrationTests;

public sealed class DatabaseSchemaTests : IAsyncDisposable
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly DatabaseInitializer _initializer;
    private readonly DbConnection _keepAliveConnection;

    public DatabaseSchemaTests()
    {
        string connectionString = $"Data Source=file:schema_test_{Guid.NewGuid():N}?mode=memory&cache=shared";
        _connectionFactory = new SqliteConnectionFactory(connectionString: connectionString);
        _initializer = new DatabaseInitializer(connectionFactory: _connectionFactory);

        // Keep-alive connection to prevent in-memory database from being disposed between calls
        _keepAliveConnection = _connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task InitializeAsync_ShouldCreateAllTablesAndFtsTriggers()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        using DbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: TestContext.Current.CancellationToken);

        // Verify tables exist
        IEnumerable<string> tables = await connection.QueryAsync<string>(
            sql: "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name;");
        var tableList = tables.ToList();

        Assert.Contains(expected: "companies", collection: tableList);
        Assert.Contains(expected: "surveys", collection: tableList);
        Assert.Contains(expected: "contacts", collection: tableList);
        Assert.Contains(expected: "responses", collection: tableList);
        Assert.Contains(expected: "contacts_fts", collection: tableList);

        // Test insertion of company and contact
        await connection.ExecuteAsync(
            sql: "INSERT INTO companies (id, name, description, created_at) VALUES (1, 'Empresa Teste', 'Descrição Teste', '2026-01-01T00:00:00Z');");

        await connection.ExecuteAsync(
            sql: "INSERT INTO contacts (id, company_id, name, email, segment, created_at) VALUES (10, 1, 'Fábio Costa', 'fabio.costa@exemplo.com.br', 'Plano Fit', '2026-01-01T00:00:00Z');");

        // Verify FTS trigger synced the contact
        int ftsCount = await connection.ExecuteScalarAsync<int>(
            sql: "SELECT COUNT(1) FROM contacts_fts WHERE contacts_fts MATCH 'Fábio';");
        Assert.Equal(expected: 1, actual: ftsCount);

        // Verify partial match without accent (trigram matching)
        int ftsNoAccent = await connection.ExecuteScalarAsync<int>(
            sql: "SELECT COUNT(1) FROM contacts_fts WHERE contacts_fts MATCH 'costa';");
        Assert.Equal(expected: 1, actual: ftsNoAccent);
    }

    public async ValueTask DisposeAsync() => await _keepAliveConnection.DisposeAsync();
}
