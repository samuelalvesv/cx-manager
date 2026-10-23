using System.Net;
using CxManager.Api.Extensions;
using CxManager.Api.Serialization;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Feedbacks;

namespace CxManager.IntegrationTests;

public sealed class ResponseEndpointsTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public ResponseEndpointsTests()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        string dbName = $"response_endpoint_test_{Guid.NewGuid():N}";
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
    public async Task GetContactResponses_WithoutCompanyHeader_ShouldFallbackToDefaultCompanyAndReturn200()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/contacts/1/responses", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        IReadOnlyList<FeedbackResponse>? list = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.IReadOnlyListFeedbackResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: list);
    }

    [Fact]
    public async Task GetContactResponses_WithInvalidCompanyHeader_ShouldReturn400BadRequest()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/contacts/1/responses");
        request.Headers.Add("X-Company-Id", "invalido");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);
    }

    [Fact]
    public async Task GetContactResponses_WithValidCompanyHeader_ShouldReturn200AndOrderedResponsesWithSurveyInfo()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/contacts/1/responses");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        IReadOnlyList<FeedbackResponse>? items = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.IReadOnlyListFeedbackResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: items);
        Assert.NotEmpty(collection: items);

        for (int i = 0; i < items.Count - 1; i++)
            Assert.True(condition: items[i].RespondedAt >= items[i + 1].RespondedAt);

        FeedbackResponse first = items[0];
        Assert.False(condition: string.IsNullOrWhiteSpace(value: first.SurveyName));
        Assert.True(condition: first.SurveyType is "NPS" or "CSAT");
        Assert.Equal(expected: 1, actual: first.ContactId);
    }

    [Fact]
    public async Task GetContactResponses_NonExistentContact_ShouldReturn404NotFound()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/contacts/99999/responses");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }

    [Fact]
    public async Task GetResponseById_ExistingResponse_ShouldReturn200AndDetail()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/responses/1");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        FeedbackResponse? item = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.FeedbackResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: item);
        Assert.Equal(expected: 1, actual: item.Id);
        Assert.False(condition: string.IsNullOrWhiteSpace(value: item.SurveyName));
        Assert.False(condition: string.IsNullOrWhiteSpace(value: item.SurveyType));
    }

    [Fact]
    public async Task GetResponseById_NonExistent_ShouldReturn404NotFound()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/responses/99999");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }

    [Fact]
    public async Task CreateResponse_NpsSurvey_ValidScore_ShouldReturn201CreatedAndLocationHeader()
    {
        var createRequest = new CreateFeedbackRequest
        {
            SurveyId = 1, // NPS Pós-Treino
            ContactId = 1,
            Score = 10,
            Channel = "whatsapp",
            Comment = "Instrutores fantásticos!"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/responses");
        request.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Created, actual: response.StatusCode);
        Assert.NotNull(@object: response.Headers.Location);

        FeedbackResponse? created = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.FeedbackResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);
        Assert.True(condition: created.Id > 0);
        Assert.Equal(expected: 10, actual: created.Score);
        Assert.Equal(expected: "whatsapp", actual: created.Channel);
        Assert.Equal(expected: "Instrutores fantásticos!", actual: created.Comment);
        Assert.Equal(expected: "NPS Pós-Treino", actual: created.SurveyName);
        Assert.Equal(expected: "NPS", actual: created.SurveyType);
    }

    [Fact]
    public async Task CreateResponse_NpsSurvey_ScoreOutOfRange_ShouldReturn400BadRequest()
    {
        var createRequest = new CreateFeedbackRequest
        {
            SurveyId = 1, // NPS
            ContactId = 1,
            Score = 11,
            Channel = "whatsapp",
            Comment = null
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/responses");
        request.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);
    }

    [Fact]
    public async Task CreateResponse_CsatSurvey_ValidScore_ShouldReturn201Created()
    {
        var createRequest = new CreateFeedbackRequest
        {
            SurveyId = 3, // CSAT Onboarding de Novos Alunos
            ContactId = 1,
            Score = 4,
            Channel = "email",
            Comment = "Onboarding tranquilo"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/responses");
        request.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Created, actual: response.StatusCode);

        FeedbackResponse? created = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.FeedbackResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);
        Assert.Equal(expected: 4, actual: created.Score);
        Assert.Equal(expected: "CSAT", actual: created.SurveyType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task CreateResponse_CsatSurvey_ScoreOutOfRange_ShouldReturn400BadRequest(int invalidCsatScore)
    {
        var createRequest = new CreateFeedbackRequest
        {
            SurveyId = 3, // CSAT
            ContactId = 1,
            Score = invalidCsatScore,
            Channel = "email",
            Comment = null
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/responses");
        request.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.Equal(expected: "A nota para pesquisas CSAT deve estar entre 1 e 5.", actual: error.Error);
    }

    [Fact]
    public async Task CreateResponse_InvalidChannel_ShouldReturn400BadRequest()
    {
        var createRequest = new CreateFeedbackRequest
        {
            SurveyId = 1,
            ContactId = 1,
            Score = 10,
            Channel = "sms",
            Comment = null
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/responses");
        request.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.Equal(expected: "O canal deve ser 'whatsapp', 'email' ou 'link'.", actual: error.Error);
    }

    [Fact]
    public async Task CreateResponse_NonExistentSurvey_ShouldReturn404NotFound()
    {
        var createRequest = new CreateFeedbackRequest
        {
            SurveyId = 99999,
            ContactId = 1,
            Score = 10,
            Channel = "link",
            Comment = null
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/responses");
        request.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }

    [Fact]
    public async Task CreateResponse_NonExistentContact_ShouldReturn404NotFound()
    {
        var createRequest = new CreateFeedbackRequest
        {
            SurveyId = 1,
            ContactId = 99999,
            Score = 10,
            Channel = "link",
            Comment = null
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/responses");
        request.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }

    [Fact]
    public async Task UpdateResponse_ValidScoreAndComment_ShouldReturn200OkAndUpdatedValues()
    {
        var updateRequest = new UpdateFeedbackRequest
        {
            Score = 9,
            Comment = "Comentário editado com sucesso"
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/responses/1");
        request.Content = JsonContent.Create(
            inputValue: updateRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.UpdateFeedbackRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        FeedbackResponse? updated = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.FeedbackResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: updated);
        Assert.Equal(expected: 9, actual: updated.Score);
        Assert.Equal(expected: "Comentário editado com sucesso", actual: updated.Comment);
    }

    [Fact]
    public async Task UpdateResponse_ScoreOutOfRangeForCsat_ShouldReturn400BadRequest()
    {
        // First find a CSAT feedback ID or create one
        var createRequest = new CreateFeedbackRequest
        {
            SurveyId = 3, // CSAT
            ContactId = 1,
            Score = 5,
            Channel = "email",
            Comment = "Inicial CSAT"
        };

        using var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/responses");
        createMsg.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);
        createMsg.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage createRes = await _client.SendAsync(
            request: createMsg,
            cancellationToken: TestContext.Current.CancellationToken);

        FeedbackResponse? created = await createRes.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.FeedbackResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);

        // Try updating with score 8 (valid for NPS, but invalid for CSAT!)
        var updateRequest = new UpdateFeedbackRequest
        {
            Score = 8,
            Comment = "Inválido para CSAT"
        };

        using var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/responses/{created.Id}");
        updateMsg.Content = JsonContent.Create(
            inputValue: updateRequest,
            jsonTypeInfo: AppJsonSerializerContext.Default.UpdateFeedbackRequest);
        updateMsg.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage updateRes = await _client.SendAsync(
            request: updateMsg,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: updateRes.StatusCode);

        ErrorResponse? error = await updateRes.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.Equal(expected: "A nota para pesquisas CSAT deve estar entre 1 e 5.", actual: error.Error);
    }

    [Fact]
    public async Task DeleteResponse_ExistingResponse_ShouldReturn204NoContent_AndSubsequentGetShouldReturn404()
    {
        using var deleteMsg = new HttpRequestMessage(HttpMethod.Delete, "/api/responses/2");
        deleteMsg.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage deleteRes = await _client.SendAsync(
            request: deleteMsg,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NoContent, actual: deleteRes.StatusCode);

        using var getMsg = new HttpRequestMessage(HttpMethod.Get, "/api/responses/2");
        getMsg.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage getRes = await _client.SendAsync(
            request: getMsg,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: getRes.StatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }
}
