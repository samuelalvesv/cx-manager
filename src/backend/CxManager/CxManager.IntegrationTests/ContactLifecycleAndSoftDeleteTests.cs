using System.Net;
using CxManager.Api.Extensions;
using CxManager.Api.Serialization;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Analytics;
using CxManager.Contract.Responses.Contacts;

namespace CxManager.IntegrationTests;

public sealed class ContactLifecycleAndSoftDeleteTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public ContactLifecycleAndSoftDeleteTests()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        string dbName = $"contact_lifecycle_test_{Guid.NewGuid():N}";
        Dictionary<string, string?> inMemorySettings = new()
        {
            ["ConnectionStrings:DefaultConnection"] = $"Data Source=file:{dbName}?mode=memory&cache=shared"
        };
        builder.Configuration.AddInMemoryCollection(initialData: inMemorySettings);

        builder.AddDatabasePersistence();
        builder.AddApiServices();

        _app = builder.Build();
        _app.UseApiPipeline();
        _app.InitializeDatabaseAsync().GetAwaiter().GetResult();
        _app.MapApiEndpoints();

        _app.StartAsync().GetAwaiter().GetResult();

        string baseAddress = _app.Urls.First();
        _client = new HttpClient { BaseAddress = new Uri(uriString: baseAddress) };
    }

    [Fact]
    public async Task CompleteContactLifecycle_ShouldValidateCreation_Conflict409_SoftDelete204_Search404_EmailLiberation_AndAnalyticsPurge()
    {
        const string testEmail = "aluno.lifecycle@exemplo.com.br";

        // 1. Verificar estado inicial do resumo analítico (1246 respostas ativas do seed)
        using var initialSummaryReq = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary", uriKind: UriKind.Relative));
        initialSummaryReq.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage initialSummaryResp = await _client.SendAsync(
            request: initialSummaryReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.OK, actual: initialSummaryResp.StatusCode);

        AnalyticsSummaryResponse? initialSummary = await initialSummaryResp.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: initialSummary);
        Assert.Equal(expected: 1246, actual: initialSummary.ResponsesCount);

        // 2. Cadastrar novo contato com sucesso (201 Created)
        CreateContactRequest createContactPayload = new()
        {
            Name = "Aluno Ciclo de Vida",
            Email = testEmail,
            Segment = "Plano Fit"
        };

        using var createContactReq = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: new Uri(uriString: "/api/contacts", uriKind: UriKind.Relative));
        createContactReq.Headers.Add(name: "X-Company-Id", value: "1");
        createContactReq.Content = JsonContent.Create(
            inputValue: createContactPayload,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);

        HttpResponseMessage createContactResp = await _client.SendAsync(
            request: createContactReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.Created, actual: createContactResp.StatusCode);

        ContactResponse? createdContact = await createContactResp.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: createdContact);
        Assert.True(condition: createdContact.Id > 0);
        Assert.Equal(expected: "Aluno Ciclo de Vida", actual: createdContact.Name);
        Assert.Equal(expected: testEmail, actual: createdContact.Email);
        Assert.Equal(expected: $"/api/contacts/{createdContact.Id}", actual: createContactResp.Headers.Location?.OriginalString);

        long contactId = createdContact.Id;

        // 3. Tentar cadastrar duplicado com o mesmo e-mail (409 Conflict com mensagem em pt-BR)
        CreateContactRequest duplicatePayload = new()
        {
            Name = "Clone Não Permitido",
            Email = testEmail.ToUpperInvariant(), // Testar insensibilidade a caixa
            Segment = "Plano Black"
        };

        using var duplicateReq = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: new Uri(uriString: "/api/contacts", uriKind: UriKind.Relative));
        duplicateReq.Headers.Add(name: "X-Company-Id", value: "1");
        duplicateReq.Content = JsonContent.Create(
            inputValue: duplicatePayload,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);

        HttpResponseMessage duplicateResp = await _client.SendAsync(
            request: duplicateReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.Conflict, actual: duplicateResp.StatusCode);

        ErrorResponse? conflictError = await duplicateResp.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: conflictError);
        Assert.Equal(
            expected: "Já existe um contato ativo cadastrado com o e-mail informado para esta empresa.",
            actual: conflictError.Error);

        // 4. Registrar feedback para esse contato e verificar incremento no resumo (1247)
        CreateFeedbackRequest feedbackPayload = new()
        {
            SurveyId = 1,
            ContactId = contactId,
            Score = 10,
            Channel = "whatsapp",
            Comment = "Excelente experiência!"
        };

        using var feedbackReq = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: new Uri(uriString: "/api/responses", uriKind: UriKind.Relative));
        feedbackReq.Headers.Add(name: "X-Company-Id", value: "1");
        feedbackReq.Content = JsonContent.Create(
            inputValue: feedbackPayload,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);

        HttpResponseMessage feedbackResp = await _client.SendAsync(
            request: feedbackReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.Created, actual: feedbackResp.StatusCode);

        // Verificar que o resumo analítico foi invalidado e refletiu a nova resposta (1247)
        using var summaryAfterFeedbackReq = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary", uriKind: UriKind.Relative));
        summaryAfterFeedbackReq.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage summaryAfterFeedbackResp = await _client.SendAsync(
            request: summaryAfterFeedbackReq,
            cancellationToken: TestContext.Current.CancellationToken);
        AnalyticsSummaryResponse? summaryAfterFeedback = await summaryAfterFeedbackResp.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: summaryAfterFeedback);
        Assert.Equal(expected: 1247, actual: summaryAfterFeedback.ResponsesCount);

        // 5. Excluir o contato logicamente (204 NoContent)
        using var deleteReq = new HttpRequestMessage(
            method: HttpMethod.Delete,
            requestUri: new Uri(uriString: $"/api/contacts/{contactId}", uriKind: UriKind.Relative));
        deleteReq.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage deleteResp = await _client.SendAsync(
            request: deleteReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.NoContent, actual: deleteResp.StatusCode);

        // 6. Consultar o contato excluído por ID deve retornar 404 NotFound
        using var getDeletedReq = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: $"/api/contacts/{contactId}", uriKind: UriKind.Relative));
        getDeletedReq.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage getDeletedResp = await _client.SendAsync(
            request: getDeletedReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.NotFound, actual: getDeletedResp.StatusCode);

        // 7. Buscar o contato na busca FTS não deve retornar o contato excluído
        using var searchReq = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/contacts?page=1&pageSize=10&search=Lifecycle", uriKind: UriKind.Relative));
        searchReq.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage searchResp = await _client.SendAsync(
            request: searchReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.OK, actual: searchResp.StatusCode);

        PagedResponse<ContactResponse>? searchPaged = await searchResp.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.PagedResponseContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: searchPaged);
        Assert.Equal(expected: 0, actual: searchPaged.Total);
        Assert.Empty(collection: searchPaged.Items);

        // 8. Liberação de e-mail: cadastrar novo contato com o mesmo e-mail deve ter sucesso (201 Created)
        CreateContactRequest recreatedPayload = new()
        {
            Name = "Aluno Reativado",
            Email = testEmail,
            Segment = "Plano Black"
        };

        using var recreatedReq = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: new Uri(uriString: "/api/contacts", uriKind: UriKind.Relative));
        recreatedReq.Headers.Add(name: "X-Company-Id", value: "1");
        recreatedReq.Content = JsonContent.Create(
            inputValue: recreatedPayload,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);

        HttpResponseMessage recreatedResp = await _client.SendAsync(
            request: recreatedReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.Created, actual: recreatedResp.StatusCode);

        ContactResponse? recreatedContact = await recreatedResp.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: recreatedContact);
        Assert.NotEqual(expected: contactId, actual: recreatedContact.Id);
        Assert.Equal(expected: "Aluno Reativado", actual: recreatedContact.Name);

        // 9. Expurgo imediato do analytics via cache invalidation:
        // O contato original foi soft-deleted; suas respostas não são contadas no resumo analítico
        using var finalSummaryReq = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary", uriKind: UriKind.Relative));
        finalSummaryReq.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage finalSummaryResp = await _client.SendAsync(
            request: finalSummaryReq,
            cancellationToken: TestContext.Current.CancellationToken);
        AnalyticsSummaryResponse? finalSummary = await finalSummaryResp.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: finalSummary);
        Assert.Equal(expected: 1246, actual: finalSummary.ResponsesCount);
    }

    [Fact]
    public async Task DeleteFeedback_ShouldDirectlyInvalidateCacheAndPurgeFromSummary()
    {
        // 1. Resumo inicial: 1246 respostas
        using var req1 = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary", uriKind: UriKind.Relative));
        req1.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage resp1 = await _client.SendAsync(
            request: req1,
            cancellationToken: TestContext.Current.CancellationToken);
        AnalyticsSummaryResponse? summary1 = await resp1.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: summary1);
        Assert.Equal(expected: 1246, actual: summary1.ResponsesCount);

        // 2. Soft-delete da resposta 1
        using var deleteReq = new HttpRequestMessage(
            method: HttpMethod.Delete,
            requestUri: new Uri(uriString: "/api/responses/1", uriKind: UriKind.Relative));
        deleteReq.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage deleteResp = await _client.SendAsync(
            request: deleteReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.NoContent, actual: deleteResp.StatusCode);

        // 3. Resumo após exclusão: 1245 respostas imediatas
        using var req2 = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary", uriKind: UriKind.Relative));
        req2.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage resp2 = await _client.SendAsync(
            request: req2,
            cancellationToken: TestContext.Current.CancellationToken);
        AnalyticsSummaryResponse? summary2 = await resp2.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: summary2);
        Assert.Equal(expected: 1245, actual: summary2.ResponsesCount);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
