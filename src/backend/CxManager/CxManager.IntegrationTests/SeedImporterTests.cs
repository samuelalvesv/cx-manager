using System.Data.Common;
using CxManager.Infrastructure.Persistence;
using CxManager.Infrastructure.Persistence.Schema;
using CxManager.Infrastructure.Persistence.Seed;
using Dapper;

namespace CxManager.IntegrationTests;

public sealed class SeedImporterTests : IAsyncDisposable
{
    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly DatabaseInitializer _initializer;
    private readonly SeedImporter _importer;
    private readonly DbConnection _keepAliveConnection;

    public SeedImporterTests()
    {
        string connectionString = $"Data Source=file:seed_test_{Guid.NewGuid():N}?mode=memory&cache=shared";
        _connectionFactory = new SqliteConnectionFactory(connectionString: connectionString);
        _initializer = new DatabaseInitializer(connectionFactory: _connectionFactory);
        _importer = new SeedImporter(connectionFactory: _connectionFactory);

        _keepAliveConnection = _connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task ImportAsync_ShouldPopulateDatabaseAndBeIdempotent()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        // First import: should load all data
        await _importer.ImportAsync(cancellationToken: TestContext.Current.CancellationToken);

        using DbConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: TestContext.Current.CancellationToken);

        int companyCount = await connection.ExecuteScalarAsync<int>(sql: "SELECT COUNT(1) FROM companies;");
        Assert.Equal(expected: 1, actual: companyCount);

        string? companyName = await connection.ExecuteScalarAsync<string>(sql: "SELECT name FROM companies WHERE id = 1;");
        Assert.Equal(expected: "Vita Bem-Estar", actual: companyName);

        int surveysCount = await connection.ExecuteScalarAsync<int>(sql: "SELECT COUNT(1) FROM surveys WHERE company_id = 1;");
        Assert.Equal(expected: 3, actual: surveysCount);

        int contactsCount = await connection.ExecuteScalarAsync<int>(sql: "SELECT COUNT(1) FROM contacts WHERE company_id = 1;");
        Assert.Equal(expected: 320, actual: contactsCount);

        int responsesCount = await connection.ExecuteScalarAsync<int>(sql: "SELECT COUNT(1) FROM responses;");
        Assert.Equal(expected: 1284, actual: responsesCount);

        int activeResponses = await connection.ExecuteScalarAsync<int>(sql: "SELECT COUNT(1) FROM responses WHERE deleted_at IS NULL;");
        Assert.Equal(expected: 1246, actual: activeResponses);

        int deletedResponses = await connection.ExecuteScalarAsync<int>(sql: "SELECT COUNT(1) FROM responses WHERE deleted_at IS NOT NULL;");
        Assert.Equal(expected: 38, actual: deletedResponses);

        // Verify FTS5 table is populated via triggers
        int ftsCount = await connection.ExecuteScalarAsync<int>(sql: "SELECT COUNT(1) FROM contacts_fts;");
        Assert.Equal(expected: 320, actual: ftsCount);

        // Second import: idempotent, should not duplicate or throw
        await _importer.ImportAsync(cancellationToken: TestContext.Current.CancellationToken);

        int companyCountAfter = await connection.ExecuteScalarAsync<int>(sql: "SELECT COUNT(1) FROM companies;");
        Assert.Equal(expected: 1, actual: companyCountAfter);
    }

    public async ValueTask DisposeAsync() => await _keepAliveConnection.DisposeAsync();
}
