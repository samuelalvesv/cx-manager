using System.Data.Common;
using CxManager.Domain.Entities;
using CxManager.Domain.Enums;
using CxManager.Infrastructure.Persistence;
using CxManager.Infrastructure.Persistence.Repositories;
using CxManager.Infrastructure.Persistence.Schema;

namespace CxManager.IntegrationTests;

public sealed class SurveyRepositoryTests : IAsyncDisposable
{
    private readonly DatabaseInitializer _initializer;
    private readonly CompanyRepository _companyRepository;
    private readonly SurveyRepository _surveyRepository;
    private readonly DbConnection _keepAliveConnection;

    public SurveyRepositoryTests()
    {
        string connectionString = $"Data Source=file:survey_repo_test_{Guid.NewGuid():N}?mode=memory&cache=shared";
        var connectionFactory = new SqliteConnectionFactory(connectionString: connectionString);
        _initializer = new DatabaseInitializer(connectionFactory: connectionFactory);
        _companyRepository = new CompanyRepository(connectionFactory: connectionFactory);
        _surveyRepository = new SurveyRepository(connectionFactory: connectionFactory);

        _keepAliveConnection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task GetAllActiveByCompanyAsync_ShouldReturnActiveSurveysForSpecificCompany()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long company1Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 1", description: "Desc 1"),
            cancellationToken: TestContext.Current.CancellationToken);

        long company2Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 2", description: "Desc 2"),
            cancellationToken: TestContext.Current.CancellationToken);

        var survey1 = Survey.Create(companyId: company1Id, name: "Pesquisa NPS 1", type: SurveyType.Nps);
        var survey2 = Survey.Create(companyId: company1Id, name: "Pesquisa CSAT 1", type: SurveyType.Csat);
        var surveyDeleted = Survey.Create(companyId: company1Id, name: "Pesquisa Excluída", type: SurveyType.Nps);
        var surveyOtherCompany = Survey.Create(companyId: company2Id, name: "Pesquisa Outra Empresa", type: SurveyType.Nps);

        long s1Id = await _surveyRepository.CreateAsync(survey: survey1, cancellationToken: TestContext.Current.CancellationToken);
        long s2Id = await _surveyRepository.CreateAsync(survey: survey2, cancellationToken: TestContext.Current.CancellationToken);
        long sDeletedId = await _surveyRepository.CreateAsync(survey: surveyDeleted, cancellationToken: TestContext.Current.CancellationToken);
        await _surveyRepository.CreateAsync(survey: surveyOtherCompany, cancellationToken: TestContext.Current.CancellationToken);

        await _surveyRepository.DeleteAsync(
            id: sDeletedId,
            companyId: company1Id,
            deletedAt: DateTimeOffset.UtcNow,
            cancellationToken: TestContext.Current.CancellationToken);

        IReadOnlyList<Survey> surveys = await _surveyRepository.GetAllActiveByCompanyAsync(
            companyId: company1Id,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: 2, actual: surveys.Count);
        Assert.Contains(collection: surveys, filter: s => s.Id == s1Id && s.Type == SurveyType.Nps);
        Assert.Contains(collection: surveys, filter: s => s.Id == s2Id && s.Type == SurveyType.Csat);
        Assert.DoesNotContain(collection: surveys, filter: s => s.Id == sDeletedId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExistsAndActive_ShouldReturnSurvey()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa Teste", description: "Desc"),
            cancellationToken: TestContext.Current.CancellationToken);

        var survey = Survey.Create(companyId: companyId, name: "NPS Semanal", type: SurveyType.Nps);
        long id = await _surveyRepository.CreateAsync(survey: survey, cancellationToken: TestContext.Current.CancellationToken);

        Survey? retrieved = await _surveyRepository.GetByIdAsync(
            id: id,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: retrieved);
        Assert.Equal(expected: id, actual: retrieved.Id);
        Assert.Equal(expected: companyId, actual: retrieved.CompanyId);
        Assert.Equal(expected: "NPS Semanal", actual: retrieved.Name);
        Assert.Equal(expected: SurveyType.Nps, actual: retrieved.Type);
    }

    [Fact]
    public async Task GetByIdAsync_WithDifferentCompanyId_ShouldReturnNull()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long company1Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 1", description: "Desc 1"),
            cancellationToken: TestContext.Current.CancellationToken);

        long company2Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 2", description: "Desc 2"),
            cancellationToken: TestContext.Current.CancellationToken);

        var survey = Survey.Create(companyId: company1Id, name: "Pesquisa Isolada", type: SurveyType.Csat);
        long id = await _surveyRepository.CreateAsync(survey: survey, cancellationToken: TestContext.Current.CancellationToken);

        Survey? retrieved = await _surveyRepository.GetByIdAsync(
            id: id,
            companyId: company2Id,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(@object: retrieved);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSurveyName()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa Teste", description: "Desc"),
            cancellationToken: TestContext.Current.CancellationToken);

        var survey = Survey.Create(companyId: companyId, name: "Nome Antigo", type: SurveyType.Nps);
        long id = await _surveyRepository.CreateAsync(survey: survey, cancellationToken: TestContext.Current.CancellationToken);

        var toUpdate = Survey.Reconstitute(
            id: id,
            companyId: companyId,
            name: "Nome Atualizado",
            type: SurveyType.Nps,
            createdAt: survey.CreatedAt,
            deletedAt: null);

        await _surveyRepository.UpdateAsync(survey: toUpdate, cancellationToken: TestContext.Current.CancellationToken);

        Survey? retrieved = await _surveyRepository.GetByIdAsync(
            id: id,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: retrieved);
        Assert.Equal(expected: "Nome Atualizado", actual: retrieved.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteSurvey_AndExcludeFromActiveQueries()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa Teste", description: "Desc"),
            cancellationToken: TestContext.Current.CancellationToken);

        var survey = Survey.Create(companyId: companyId, name: "Pesquisa Para Deletar", type: SurveyType.Nps);
        long id = await _surveyRepository.CreateAsync(survey: survey, cancellationToken: TestContext.Current.CancellationToken);

        await _surveyRepository.DeleteAsync(
            id: id,
            companyId: companyId,
            deletedAt: DateTimeOffset.UtcNow,
            cancellationToken: TestContext.Current.CancellationToken);

        Survey? retrieved = await _surveyRepository.GetByIdAsync(
            id: id,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(@object: retrieved);

        bool exists = await _surveyRepository.ExistsActiveAsync(
            id: id,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(condition: exists);
    }

    public async ValueTask DisposeAsync() => await _keepAliveConnection.DisposeAsync();
}
