using System.Data.Common;
using CxManager.Application.Models;
using CxManager.Domain.Entities;
using CxManager.Domain.Enums;
using CxManager.Infrastructure.Persistence;
using CxManager.Infrastructure.Persistence.Queries;
using CxManager.Infrastructure.Persistence.Repositories;
using CxManager.Infrastructure.Persistence.Schema;

namespace CxManager.IntegrationTests;

public sealed class FeedbackRepositoryTests : IAsyncDisposable
{
    private readonly DatabaseInitializer _initializer;
    private readonly CompanyRepository _companyRepository;
    private readonly SurveyRepository _surveyRepository;
    private readonly ContactRepository _contactRepository;
    private readonly FeedbackRepository _feedbackRepository;
    private readonly FeedbackQueryService _feedbackQueryService;
    private readonly DbConnection _keepAliveConnection;

    public FeedbackRepositoryTests()
    {
        string connectionString = $"Data Source=file:feedback_repo_test_{Guid.NewGuid():N}?mode=memory&cache=shared";
        var connectionFactory = new SqliteConnectionFactory(connectionString: connectionString);
        _initializer = new DatabaseInitializer(connectionFactory: connectionFactory);
        _companyRepository = new CompanyRepository(connectionFactory: connectionFactory);
        _surveyRepository = new SurveyRepository(connectionFactory: connectionFactory);
        _contactRepository = new ContactRepository(connectionFactory: connectionFactory);
        _feedbackRepository = new FeedbackRepository(connectionFactory: connectionFactory);
        _feedbackQueryService = new FeedbackQueryService(connectionFactory: connectionFactory);

        _keepAliveConnection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CreateAsync_AndGetByIdAsync_ShouldPersistAndRetrieveFeedback()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Academia Vita", description: "Unidade Central"),
            cancellationToken: TestContext.Current.CancellationToken);

        long surveyId = await _surveyRepository.CreateAsync(
            survey: Survey.Create(companyId: companyId, name: "NPS Pós-Treino", type: SurveyType.Nps),
            cancellationToken: TestContext.Current.CancellationToken);

        long contactId = await _contactRepository.CreateAsync(
            contact: Contact.Create(companyId: companyId, name: "Lucas Lima", email: "lucas@example.com", segment: "Plano Black"),
            cancellationToken: TestContext.Current.CancellationToken);

        var feedback = Feedback.Create(
            surveyId: surveyId,
            contactId: contactId,
            score: 10,
            channel: "whatsapp",
            comment: "Treino impecável!");

        long feedbackId = await _feedbackRepository.CreateAsync(
            feedback: feedback,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(condition: feedbackId > 0);

        Feedback? retrieved = await _feedbackRepository.GetByIdAsync(
            id: feedbackId,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: retrieved);
        Assert.Equal(expected: feedbackId, actual: retrieved.Id);
        Assert.Equal(expected: surveyId, actual: retrieved.SurveyId);
        Assert.Equal(expected: contactId, actual: retrieved.ContactId);
        Assert.Equal(expected: 10, actual: retrieved.Score);
        Assert.Equal(expected: "whatsapp", actual: retrieved.Channel);
        Assert.Equal(expected: "Treino impecável!", actual: retrieved.Comment);
    }

    [Fact]
    public async Task GetDetailByIdAsync_ShouldReturnSurveyNameAndSurveyType()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Academia Vita", description: "Unidade Central"),
            cancellationToken: TestContext.Current.CancellationToken);

        long surveyId = await _surveyRepository.CreateAsync(
            survey: Survey.Create(companyId: companyId, name: "NPS Anual", type: SurveyType.Nps),
            cancellationToken: TestContext.Current.CancellationToken);

        long contactId = await _contactRepository.CreateAsync(
            contact: Contact.Create(companyId: companyId, name: "Mariana Souza", email: "mariana@example.com", segment: "Plano Fit"),
            cancellationToken: TestContext.Current.CancellationToken);

        var feedback = Feedback.Create(
            surveyId: surveyId,
            contactId: contactId,
            score: 9,
            channel: "email",
            comment: "Ótima infraestrutura");

        long feedbackId = await _feedbackRepository.CreateAsync(
            feedback: feedback,
            cancellationToken: TestContext.Current.CancellationToken);

        FeedbackReadModel? detail = await _feedbackQueryService.GetDetailByIdAsync(
            id: feedbackId,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: detail);
        Assert.Equal(expected: feedbackId, actual: detail.Id);
        Assert.Equal(expected: "NPS Anual", actual: detail.SurveyName);
        Assert.Equal(expected: "NPS", actual: detail.SurveyType);
        Assert.Equal(expected: 9, actual: detail.Score);
        Assert.Equal(expected: "email", actual: detail.Channel);
    }

    [Fact]
    public async Task GetActiveByContactIdAsync_ShouldReturnOrderedFeedbacksByRespondedAtDesc()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Academia Vita", description: "Unidade Central"),
            cancellationToken: TestContext.Current.CancellationToken);

        long surveyNpsId = await _surveyRepository.CreateAsync(
            survey: Survey.Create(companyId: companyId, name: "NPS Semanal", type: SurveyType.Nps),
            cancellationToken: TestContext.Current.CancellationToken);

        long surveyCsatId = await _surveyRepository.CreateAsync(
            survey: Survey.Create(companyId: companyId, name: "CSAT Recepção", type: SurveyType.Csat),
            cancellationToken: TestContext.Current.CancellationToken);

        long contactId = await _contactRepository.CreateAsync(
            contact: Contact.Create(companyId: companyId, name: "Felipe Melo", email: "felipe@example.com", segment: "Plano Black"),
            cancellationToken: TestContext.Current.CancellationToken);

        DateTimeOffset pastDate = DateTimeOffset.UtcNow.AddDays(days: -5);
        DateTimeOffset recentDate = DateTimeOffset.UtcNow.AddDays(days: -1);

        var feedback1 = Feedback.Create(
            surveyId: surveyNpsId,
            contactId: contactId,
            score: 7,
            channel: "link",
            comment: "Mais antiga",
            respondedAt: pastDate);

        var feedback2 = Feedback.Create(
            surveyId: surveyCsatId,
            contactId: contactId,
            score: 5,
            channel: "whatsapp",
            comment: "Mais recente",
            respondedAt: recentDate);

        await _feedbackRepository.CreateAsync(feedback: feedback1, cancellationToken: TestContext.Current.CancellationToken);
        await _feedbackRepository.CreateAsync(feedback: feedback2, cancellationToken: TestContext.Current.CancellationToken);

        IReadOnlyList<FeedbackReadModel> items = await _feedbackQueryService.GetActiveByContactIdAsync(
            contactId: contactId,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: 2, actual: items.Count);
        Assert.Equal(expected: "Mais recente", actual: items[0].Comment);
        Assert.Equal(expected: "CSAT Recepção", actual: items[0].SurveyName);
        Assert.Equal(expected: "CSAT", actual: items[0].SurveyType);
        Assert.Equal(expected: "Mais antiga", actual: items[1].Comment);
        Assert.Equal(expected: "NPS Semanal", actual: items[1].SurveyName);
        Assert.Equal(expected: "NPS", actual: items[1].SurveyType);
        Assert.True(condition: items[0].RespondedAt >= items[1].RespondedAt);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateScoreAndComment()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Academia Vita", description: "Unidade Central"),
            cancellationToken: TestContext.Current.CancellationToken);

        long surveyId = await _surveyRepository.CreateAsync(
            survey: Survey.Create(companyId: companyId, name: "Pesquisa NPS", type: SurveyType.Nps),
            cancellationToken: TestContext.Current.CancellationToken);

        long contactId = await _contactRepository.CreateAsync(
            contact: Contact.Create(companyId: companyId, name: "Renata Dias", email: "renata@example.com", segment: "Plano Black"),
            cancellationToken: TestContext.Current.CancellationToken);

        var feedback = Feedback.Create(
            surveyId: surveyId,
            contactId: contactId,
            score: 6,
            channel: "whatsapp",
            comment: "Nota inicial");

        long feedbackId = await _feedbackRepository.CreateAsync(
            feedback: feedback,
            cancellationToken: TestContext.Current.CancellationToken);

        Feedback? existing = await _feedbackRepository.GetByIdAsync(
            id: feedbackId,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: existing);
        existing.Update(score: 10, comment: "Nota corrigida", surveyType: SurveyType.Nps);

        await _feedbackRepository.UpdateAsync(
            feedback: existing,
            cancellationToken: TestContext.Current.CancellationToken);

        Feedback? updated = await _feedbackRepository.GetByIdAsync(
            id: feedbackId,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: updated);
        Assert.Equal(expected: 10, actual: updated.Score);
        Assert.Equal(expected: "Nota corrigida", actual: updated.Comment);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteFeedback()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long companyId = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Academia Vita", description: "Unidade Central"),
            cancellationToken: TestContext.Current.CancellationToken);

        long surveyId = await _surveyRepository.CreateAsync(
            survey: Survey.Create(companyId: companyId, name: "Pesquisa NPS", type: SurveyType.Nps),
            cancellationToken: TestContext.Current.CancellationToken);

        long contactId = await _contactRepository.CreateAsync(
            contact: Contact.Create(companyId: companyId, name: "Julio Cesar", email: "julio@example.com", segment: "Plano Fit"),
            cancellationToken: TestContext.Current.CancellationToken);

        var feedback = Feedback.Create(
            surveyId: surveyId,
            contactId: contactId,
            score: 8,
            channel: "email");

        long feedbackId = await _feedbackRepository.CreateAsync(
            feedback: feedback,
            cancellationToken: TestContext.Current.CancellationToken);

        await _feedbackRepository.DeleteAsync(
            id: feedbackId,
            companyId: companyId,
            deletedAt: DateTimeOffset.UtcNow,
            cancellationToken: TestContext.Current.CancellationToken);

        Feedback? deleted = await _feedbackRepository.GetByIdAsync(
            id: feedbackId,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(@object: deleted);

        bool exists = await _feedbackRepository.ExistsActiveAsync(
            id: feedbackId,
            companyId: companyId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(condition: exists);
    }

    [Fact]
    public async Task MultiTenant_DifferentCompany_ShouldNotAccessFeedback()
    {
        await _initializer.InitializeAsync(cancellationToken: TestContext.Current.CancellationToken);

        long company1Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 1", description: "Desc 1"),
            cancellationToken: TestContext.Current.CancellationToken);

        long company2Id = await _companyRepository.CreateAsync(
            company: Company.Create(name: "Empresa 2", description: "Desc 2"),
            cancellationToken: TestContext.Current.CancellationToken);

        long surveyId = await _surveyRepository.CreateAsync(
            survey: Survey.Create(companyId: company1Id, name: "Pesquisa C1", type: SurveyType.Nps),
            cancellationToken: TestContext.Current.CancellationToken);

        long contactId = await _contactRepository.CreateAsync(
            contact: Contact.Create(companyId: company1Id, name: "Aluno C1", email: "aluno@c1.com", segment: "Fit"),
            cancellationToken: TestContext.Current.CancellationToken);

        var feedback = Feedback.Create(
            surveyId: surveyId,
            contactId: contactId,
            score: 10,
            channel: "whatsapp");

        long feedbackId = await _feedbackRepository.CreateAsync(
            feedback: feedback,
            cancellationToken: TestContext.Current.CancellationToken);

        Feedback? fromCompany2 = await _feedbackRepository.GetByIdAsync(
            id: feedbackId,
            companyId: company2Id,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(@object: fromCompany2);

        bool existsInCompany2 = await _feedbackRepository.ExistsActiveAsync(
            id: feedbackId,
            companyId: company2Id,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(condition: existsInCompany2);
    }

    public async ValueTask DisposeAsync()
        => await _keepAliveConnection.DisposeAsync();
}
