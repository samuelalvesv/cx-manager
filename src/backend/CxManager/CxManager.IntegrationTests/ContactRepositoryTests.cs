using System.Data.Common;
using CxManager.Domain.Entities;
using CxManager.Infrastructure.Persistence;
using CxManager.Infrastructure.Persistence.Repositories;
using CxManager.Infrastructure.Persistence.Schema;

namespace CxManager.IntegrationTests;

public sealed class ContactRepositoryTests : IAsyncDisposable
{
    private readonly DatabaseInitializer _initializer;
    private readonly CompanyRepository _companyRepository;
    private readonly ContactRepository _contactRepository;
    private readonly DbConnection _keepAliveConnection;

    public ContactRepositoryTests()
    {
        string connectionString = $"Data Source=file:contact_repo_test_{Guid.NewGuid():N}?mode=memory&cache=shared";
        var connectionFactory = new SqliteConnectionFactory(connectionString: connectionString);
        _initializer = new DatabaseInitializer(connectionFactory: connectionFactory);
        _companyRepository = new CompanyRepository(connectionFactory: connectionFactory);
        _contactRepository = new ContactRepository(connectionFactory: connectionFactory);

        _keepAliveConnection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetPagedByCompanyAsync_WithoutSearch_ShouldReturnPaginatedActiveContacts()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long company1Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 1", description: "Desc 1"),
            cancellationToken: TestContext.Current.CancellationToken);

        long company2Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 2", description: "Desc 2"),
            cancellationToken: TestContext.Current.CancellationToken);

        var c1 = Contact.Create(companyId: company1Id, name: "Bruno Alves", email: "bruno@exemplo.com", segment: "Fit");
        var c2 = Contact.Create(companyId: company1Id, name: "Ana Clara", email: "ana@exemplo.com", segment: "Black");
        var c3 = Contact.Create(companyId: company1Id, name: "Carlos Daniel", email: "carlos@exemplo.com", segment: "Fit");
        var cDeleted = Contact.Create(companyId: company1Id, name: "Aline Excluída", email: "aline@exemplo.com", segment: "Fit");
        var cOtherCompany = Contact.Create(companyId: company2Id, name: "Amélia Outra", email: "amelia@exemplo.com", segment: "Fit");

        await _contactRepository.CreateAsync(contact: c1, cancellationToken: TestContext.Current.CancellationToken);
        await _contactRepository.CreateAsync(contact: c2, cancellationToken: TestContext.Current.CancellationToken);
        await _contactRepository.CreateAsync(contact: c3, cancellationToken: TestContext.Current.CancellationToken);
        long deletedId = await _contactRepository.CreateAsync(contact: cDeleted, cancellationToken: TestContext.Current.CancellationToken);
        await _contactRepository.CreateAsync(contact: cOtherCompany, cancellationToken: TestContext.Current.CancellationToken);

        await _contactRepository.DeleteAsync(
            id: deletedId,
            companyId: company1Id,
            deletedAt: DateTimeOffset.UtcNow,
            cancellationToken: TestContext.Current.CancellationToken);

        (IReadOnlyList<Contact> page1, int total) = await _contactRepository.GetPagedByCompanyAsync(
            companyId: company1Id,
            page: 1,
            pageSize: 2,
            search: null,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: 3, actual: total);
        Assert.Equal(expected: 2, actual: page1.Count);
        // Ordered by name ASC: Ana Clara, Bruno Alves
        Assert.Equal(expected: "Ana Clara", actual: page1[0].Name);
        Assert.Equal(expected: "Bruno Alves", actual: page1[1].Name);

        (IReadOnlyList<Contact> page2, int total2) = await _contactRepository.GetPagedByCompanyAsync(
            companyId: company1Id,
            page: 2,
            pageSize: 2,
            search: null,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: 3, actual: total2);
        Assert.Single(collection: page2);
        Assert.Equal(expected: "Carlos Daniel", actual: page2[0].Name);
    }

    [Fact]
    public async Task GetPagedByCompanyAsync_WithFtsSearch_ShouldMatchCaseAndAccentInsensitive()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa FTS", description: "Desc"),
            cancellationToken: TestContext.Current.CancellationToken);

        var c1 = Contact.Create(companyId: companyId, name: "Fábio Costa", email: "fabio.costa@exemplo.com.br", segment: "Fit");
        var c2 = Contact.Create(companyId: companyId, name: "Gabriel Souza", email: "gabriel@costa-advogados.com.br", segment: "Black");
        var c3 = Contact.Create(companyId: companyId, name: "Juliana Silva", email: "juliana@gmail.com", segment: "Fit");

        await _contactRepository.CreateAsync(contact: c1, cancellationToken: TestContext.Current.CancellationToken);
        await _contactRepository.CreateAsync(contact: c2, cancellationToken: TestContext.Current.CancellationToken);
        await _contactRepository.CreateAsync(contact: c3, cancellationToken: TestContext.Current.CancellationToken);

        // Search "fabio" (without accent) matches "Fábio Costa"
        (IReadOnlyList<Contact> results1, int total1) = await _contactRepository.GetPagedByCompanyAsync(
            companyId: companyId,
            page: 1,
            pageSize: 10,
            search: "fabio",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: 1, actual: total1);
        Assert.Single(collection: results1);
        Assert.Equal(expected: "Fábio Costa", actual: results1[0].Name);

        // Search "costa" matches both c1 ("Costa" in name) and c2 ("costa" in email)
        (IReadOnlyList<Contact> results2, int total2) = await _contactRepository.GetPagedByCompanyAsync(
            companyId: companyId,
            page: 1,
            pageSize: 10,
            search: "costa",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: 2, actual: total2);
        Assert.Equal(expected: 2, actual: results2.Count);

        // Search "gmail" matches c3
        (IReadOnlyList<Contact> results3, int total3) = await _contactRepository.GetPagedByCompanyAsync(
            companyId: companyId,
            page: 1,
            pageSize: 10,
            search: "gmail",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: 1, actual: total3);
        Assert.Equal(expected: "Juliana Silva", actual: results3[0].Name);
    }

    [Fact]
    public async Task ExistsActiveEmailAsync_ShouldEnforceUniqueEmailOnlyForActiveContactsInSameCompany()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long company1Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 1", description: "Desc 1"),
            cancellationToken: TestContext.Current.CancellationToken);

        long company2Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 2", description: "Desc 2"),
            cancellationToken: TestContext.Current.CancellationToken);

        var c1 = Contact.Create(companyId: company1Id, name: "Contato 1", email: "teste@exemplo.com", segment: "Fit");
        long c1Id = await _contactRepository.CreateAsync(contact: c1, cancellationToken: TestContext.Current.CancellationToken);

        // Should exist for company 1
        bool existsComp1 = await _contactRepository.ExistsActiveEmailAsync(
            companyId: company1Id,
            email: "TESTE@EXEMPLO.COM",
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(condition: existsComp1);

        // Same email in different company should not conflict
        bool existsComp2 = await _contactRepository.ExistsActiveEmailAsync(
            companyId: company2Id,
            email: "teste@exemplo.com",
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(condition: existsComp2);

        // Exclude self during update should return false
        bool existsSelf = await _contactRepository.ExistsActiveEmailAsync(
            companyId: company1Id,
            email: "teste@exemplo.com",
            excludeContactId: c1Id,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(condition: existsSelf);

        // Soft delete contact 1
        await _contactRepository.DeleteAsync(
            id: c1Id,
            companyId: company1Id,
            deletedAt: DateTimeOffset.UtcNow,
            cancellationToken: TestContext.Current.CancellationToken);

        // After soft delete, email is freed!
        bool existsAfterDelete = await _contactRepository.ExistsActiveEmailAsync(
            companyId: company1Id,
            email: "teste@exemplo.com",
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(condition: existsAfterDelete);
    }

    [Fact]
    public async Task GetByIdAsync_WhenActive_ShouldReturnContact()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa Teste", description: "Desc"),
            cancellationToken: TestContext.Current.CancellationToken);

        var contact = Contact.Create(companyId: companyId, name: "Contato Ativo", email: "ativo@exemplo.com", segment: "Fit");
        long id = await _contactRepository.CreateAsync(contact: contact, cancellationToken: TestContext.Current.CancellationToken);

        Contact? retrieved = await _contactRepository.GetByIdAsync(id: id, companyId: companyId, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: retrieved);
        Assert.Equal(expected: id, actual: retrieved.Id);
        Assert.Equal(expected: "Contato Ativo", actual: retrieved.Name);
        Assert.Equal(expected: "ativo@exemplo.com", actual: retrieved.Email);
        Assert.Equal(expected: "Fit", actual: retrieved.Segment);
    }

    [Fact]
    public async Task GetByIdAsync_WhenContactIsSoftDeleted_ShouldReturnNull()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa Teste", description: "Desc"),
            cancellationToken: TestContext.Current.CancellationToken);

        var contact = Contact.Create(companyId: companyId, name: "Contato Soft Delete", email: "soft@exemplo.com", segment: "Fit");
        long id = await _contactRepository.CreateAsync(contact: contact, cancellationToken: TestContext.Current.CancellationToken);

        await _contactRepository.DeleteAsync(
            id: id,
            companyId: companyId,
            deletedAt: DateTimeOffset.UtcNow,
            cancellationToken: TestContext.Current.CancellationToken);

        Contact? retrieved = await _contactRepository.GetByIdAsync(id: id, companyId: companyId, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(@object: retrieved);
    }

    [Fact]
    public async Task GetByIdAsync_WhenDifferentCompany_ShouldReturnNull()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long company1Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 1", description: "Desc 1"),
            cancellationToken: TestContext.Current.CancellationToken);

        long company2Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 2", description: "Desc 2"),
            cancellationToken: TestContext.Current.CancellationToken);

        var contact = Contact.Create(companyId: company1Id, name: "Contato C1", email: "c1@exemplo.com", segment: "Fit");
        long id = await _contactRepository.CreateAsync(contact: contact, cancellationToken: TestContext.Current.CancellationToken);

        Contact? retrieved = await _contactRepository.GetByIdAsync(id: id, companyId: company2Id, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(@object: retrieved);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFieldsCorrectly()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa Teste", description: "Desc"),
            cancellationToken: TestContext.Current.CancellationToken);

        var contact = Contact.Create(companyId: companyId, name: "Nome Antigo", email: "antigo@exemplo.com", segment: "Fit");
        long id = await _contactRepository.CreateAsync(contact: contact, cancellationToken: TestContext.Current.CancellationToken);

        var toUpdate = Contact.Reconstitute(
            id: id,
            companyId: companyId,
            name: "Nome Novo",
            email: "novo@exemplo.com",
            segment: "Black",
            createdAt: contact.CreatedAt,
            deletedAt: null);

        await _contactRepository.UpdateAsync(contact: toUpdate, cancellationToken: TestContext.Current.CancellationToken);

        Contact? retrieved = await _contactRepository.GetByIdAsync(id: id, companyId: companyId, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: retrieved);
        Assert.Equal(expected: "Nome Novo", actual: retrieved.Name);
        Assert.Equal(expected: "novo@exemplo.com", actual: retrieved.Email);
        Assert.Equal(expected: "Black", actual: retrieved.Segment);
    }

    public async ValueTask DisposeAsync() => await _keepAliveConnection.DisposeAsync();
}
