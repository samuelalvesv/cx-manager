using System.Net;
using CxManager.Api.Extensions;
using CxManager.Api.Serialization;
using CxManager.Contract.Requests.Surveys;
using CxManager.Contract.Responses.Surveys;

namespace CxManager.IntegrationTests;

public sealed class SurveyEndpointsTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public SurveyEndpointsTests()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        string dbName = $"survey_endpoint_test_{Guid.NewGuid():N}";
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
    public async Task GetSurveys_WithoutCompanyHeader_ShouldFallbackToDefaultCompanyAndReturn200()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/surveys", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        IReadOnlyList<SurveyResponse>? surveys = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.IReadOnlyListSurveyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: surveys);
        Assert.Equal(expected: 3, actual: surveys.Count);
    }

    [Fact]
    public async Task GetSurveys_WithInvalidCompanyHeader_ShouldReturn400BadRequest()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/surveys");
        request.Headers.Add("X-Company-Id", "invalido");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);
    }

    [Fact]
    public async Task GetSurveys_WithValidCompanyHeader_ShouldReturn200AndActiveSurveys()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/surveys");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        IReadOnlyList<SurveyResponse>? surveys = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.IReadOnlyListSurveyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: surveys);
        Assert.True(condition: surveys.Count >= 3);
        Assert.Contains(collection: surveys, filter: s => s.Name == "NPS Pós-Treino" && s.Type == "NPS");
        Assert.Contains(collection: surveys, filter: s => s.Name == "CSAT Onboarding de Novos Alunos" && s.Type == "CSAT");
    }

    [Fact]
    public async Task GetSurveyById_WhenExists_ShouldReturn200AndSurvey()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/surveys/1");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        SurveyResponse? survey = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.SurveyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: survey);
        Assert.Equal(expected: 1, actual: survey.Id);
        Assert.Equal(expected: "NPS Pós-Treino", actual: survey.Name);
        Assert.Equal(expected: "NPS", actual: survey.Type);
    }

    [Fact]
    public async Task GetSurveyById_ForDifferentCompany_ShouldReturn404NotFound()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/surveys/1");
        request.Headers.Add("X-Company-Id", "999"); // Other company

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }

    [Fact]
    public async Task CreateSurvey_WithValidNps_ShouldReturn201CreatedAndLocationHeader()
    {
        CreateSurveyRequest payload = new()
        {
            Name = "Nova Pesquisa Trimestral",
            Type = "NPS"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/surveys")
        {
            Content = JsonContent.Create(
                inputValue: payload,
                jsonTypeInfo: AppJsonSerializerContext.Default.CreateSurveyRequest)
        };
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Created, actual: response.StatusCode);
        Assert.NotNull(@object: response.Headers.Location);

        SurveyResponse? created = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.SurveyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);
        Assert.True(condition: created.Id > 0);
        Assert.Equal(expected: "Nova Pesquisa Trimestral", actual: created.Name);
        Assert.Equal(expected: "NPS", actual: created.Type);
        Assert.Equal(expected: $"/api/surveys/{created.Id}", actual: response.Headers.Location.OriginalString);
    }

    [Fact]
    public async Task CreateSurvey_WithValidCsat_ShouldReturn201Created()
    {
        CreateSurveyRequest payload = new()
        {
            Name = "Avaliação de Suporte",
            Type = "CSAT"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/surveys")
        {
            Content = JsonContent.Create(
                inputValue: payload,
                jsonTypeInfo: AppJsonSerializerContext.Default.CreateSurveyRequest)
        };
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Created, actual: response.StatusCode);

        SurveyResponse? created = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.SurveyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);
        Assert.Equal(expected: "CSAT", actual: created.Type);
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("CES")]
    [InlineData("")]
    public async Task CreateSurvey_WithInvalidType_ShouldReturn400BadRequest(string invalidType)
    {
        CreateSurveyRequest payload = new()
        {
            Name = "Pesquisa Inválida",
            Type = invalidType
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/surveys")
        {
            Content = JsonContent.Create(
                inputValue: payload,
                jsonTypeInfo: AppJsonSerializerContext.Default.CreateSurveyRequest)
        };
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);
    }

    [Fact]
    public async Task CreateSurvey_WithInvalidName_ShouldReturn400BadRequest()
    {
        CreateSurveyRequest payload = new()
        {
            Name = "A",
            Type = "NPS"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/surveys")
        {
            Content = JsonContent.Create(
                inputValue: payload,
                jsonTypeInfo: AppJsonSerializerContext.Default.CreateSurveyRequest)
        };
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);
    }

    [Fact]
    public async Task UpdateSurvey_WithValidData_ShouldReturn200AndUpdatedSurvey()
    {
        UpdateSurveyRequest payload = new()
        {
            Name = "NPS Pós-Treino Atualizado"
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/surveys/1")
        {
            Content = JsonContent.Create(
                inputValue: payload,
                jsonTypeInfo: AppJsonSerializerContext.Default.UpdateSurveyRequest)
        };
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        SurveyResponse? updated = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.SurveyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: updated);
        Assert.Equal(expected: 1, actual: updated.Id);
        Assert.Equal(expected: "NPS Pós-Treino Atualizado", actual: updated.Name);
    }

    [Fact]
    public async Task UpdateSurvey_WhenNotFound_ShouldReturn404()
    {
        UpdateSurveyRequest payload = new()
        {
            Name = "Nome Atualizado"
        };

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/surveys/999")
        {
            Content = JsonContent.Create(
                inputValue: payload,
                jsonTypeInfo: AppJsonSerializerContext.Default.UpdateSurveyRequest)
        };
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }

    [Fact]
    public async Task DeleteSurvey_WhenExists_ShouldReturn204NoContentAndSoftDelete()
    {
        // First create a survey to delete
        CreateSurveyRequest payload = new()
        {
            Name = "Pesquisa Descartável",
            Type = "NPS"
        };

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/surveys")
        {
            Content = JsonContent.Create(
                inputValue: payload,
                jsonTypeInfo: AppJsonSerializerContext.Default.CreateSurveyRequest)
        };
        createRequest.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage createResponse = await _client.SendAsync(
            request: createRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        SurveyResponse? created = await createResponse.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.SurveyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);
        long surveyId = created.Id;

        // Delete survey
        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/surveys/{surveyId}");
        deleteRequest.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage deleteResponse = await _client.SendAsync(
            request: deleteRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NoContent, actual: deleteResponse.StatusCode);

        // Subsequent GET should return 404
        using var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/surveys/{surveyId}");
        getRequest.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage getResponse = await _client.SendAsync(
            request: getRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteSurvey_WhenNotFound_ShouldReturn404()
    {
        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, "/api/surveys/999");
        deleteRequest.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: deleteRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }
}
