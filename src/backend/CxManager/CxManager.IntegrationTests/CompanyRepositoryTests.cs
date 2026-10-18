using System.Data.Common;
using CxManager.Domain.Entities;
using CxManager.Infrastructure.Persistence;
using CxManager.Infrastructure.Persistence.Repositories;
using CxManager.Infrastructure.Persistence.Schema;

namespace CxManager.IntegrationTests;

public sealed class CompanyRepositoryTests : IAsyncDisposable
{
    private readonly DatabaseInitializer _initializer;
    private readonly CompanyRepository _repository;
    private readonly DbConnection _keepAliveConnection;

    public CompanyRepositoryTests()
    {
        string connectionString = $"Data Source=file:company_repo_test_{Guid.NewGuid():N}?mode=memory&cache=shared";
        var connectionFactory = new SqliteConnectionFactory(connectionString: connectionString);
        _initializer = new DatabaseInitializer(connectionFactory: connectionFactory);
        _repository = new CompanyRepository(connectionFactory: connectionFactory);

        _keepAliveConnection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetAllActiveAsync_ShouldReturnActiveCompanies()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        var company1 = Company.Create(name: "Academia Alpha", description: "Unidade 1");
        var company2 = Company.Create(name: "Academia Beta", description: "Unidade 2");

        long id1 = await _repository.CreateAsync(company: company1, cancellationToken: TestContext.Current.CancellationToken);
        long id2 = await _repository.CreateAsync(company: company2, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(condition: id1 > 0);
        Assert.True(condition: id2 > 0);

        IReadOnlyList<Company> companies = await _repository.GetAllActiveAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: 2, actual: companies.Count);
        Assert.Equal(expected: "Academia Alpha", actual: companies[0].Name);
        Assert.Equal(expected: "Academia Beta", actual: companies[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        Company? company = await _repository.GetByIdAsync(id: 999, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(@object: company);
    }

    public async ValueTask DisposeAsync() => await _keepAliveConnection.DisposeAsync();
}
