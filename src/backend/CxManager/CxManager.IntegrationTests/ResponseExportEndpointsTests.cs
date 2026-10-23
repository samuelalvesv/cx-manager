using System.Net;
using CxManager.Api.Extensions;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;

namespace CxManager.IntegrationTests;

public sealed class ResponseExportEndpointsTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public ResponseExportEndpointsTests()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        string dbName = $"response_export_test_{Guid.NewGuid():N}";
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

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Export_WithoutFilters_ShouldReturn200AndValidCsvHeaders()
    {
        using var request = new HttpRequestMessage(method: HttpMethod.Get, requestUri: "/api/responses/export");
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);
        Assert.Equal(expected: "text/csv", actual: response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expected: "utf-8", actual: response.Content.Headers.ContentType?.CharSet);
        Assert.NotNull(@object: response.Content.Headers.ContentDisposition);
        Assert.Equal(expected: "attachment", actual: response.Content.Headers.ContentDisposition.DispositionType);
        Assert.Equal(expected: "respostas.csv", actual: response.Content.Headers.ContentDisposition.FileName);

        string csvContent = await response.Content.ReadAsStringAsync(cancellationToken: TestContext.Current.CancellationToken);
        string[] lines = csvContent.Split(["\r\n", "\n"], options: StringSplitOptions.RemoveEmptyEntries);

        Assert.True(condition: lines.Length > 1, userMessage: "O CSV deve conter cabeçalho e linhas de dados.");
        Assert.Equal(expected: "id,survey,contact,score,comment,channel,respondedAt", actual: lines[0]);
    }

    [Fact]
    public async Task Export_WithSurveyIdFilter_ShouldReturnOnlyResponsesForSurvey()
    {
        using var request = new HttpRequestMessage(method: HttpMethod.Get, requestUri: "/api/responses/export?surveyId=1");
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        string csvContent = await response.Content.ReadAsStringAsync(cancellationToken: TestContext.Current.CancellationToken);
        string[] lines = csvContent.Split(["\r\n", "\n"], options: StringSplitOptions.RemoveEmptyEntries);

        Assert.True(condition: lines.Length > 1);
        Assert.Equal(expected: "id,survey,contact,score,comment,channel,respondedAt", actual: lines[0]);

        // Todas as linhas de dados devem ser da pesquisa 1 ("NPS Pós-Treino")
        for (int i = 1; i < lines.Length; i++)
            Assert.Contains(expectedSubstring: "NPS Pós-Treino", actualString: lines[i], comparisonType: StringComparison.Ordinal);
    }

    [Fact]
    public async Task Export_WithInvalidDateFilter_ShouldReturn400BadRequest()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: "/api/responses/export?from=2026-05-01&to=2026-01-01");
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);
    }

    [Fact]
    public async Task Export_WithSpecialCharacters_ShouldEscapeRFC4180Correctly()
    {
        // 1. Cria uma resposta com aspas, vírgulas e quebra de linha
        CreateFeedbackRequest createRequest = new()
        {
            SurveyId = 1,
            ContactId = 1,
            Score = 10,
            Channel = "WhatsApp",
            Comment = "Ambiente \"limpo\", aparelhos novos\ne atendimento nota 10!"
        };

        using var postRequest = new HttpRequestMessage(method: HttpMethod.Post, requestUri: "/api/responses");
        postRequest.Headers.Add(name: "X-Company-Id", value: "1");
        postRequest.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: Api.Serialization.AppJsonSerializerContext.Default.CreateFeedbackRequest);

        HttpResponseMessage postResponse = await _client.SendAsync(
            request: postRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Created, actual: postResponse.StatusCode);

        // 2. Exporta e verifica que o comentário foi escapado conforme RFC 4180
        using var exportRequest = new HttpRequestMessage(method: HttpMethod.Get, requestUri: "/api/responses/export?surveyId=1");
        exportRequest.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage exportResponse = await _client.SendAsync(
            request: exportRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: exportResponse.StatusCode);

        string csvContent = await exportResponse.Content.ReadAsStringAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Contains(
            expectedSubstring: "\"Ambiente \"\"limpo\"\", aparelhos novos\ne atendimento nota 10!\"",
            actualString: csvContent,
            comparisonType: StringComparison.Ordinal);
    }

    [Fact]
    public async Task Export_WhenResponseIsSoftDeleted_ShouldExcludeFromExport()
    {
        // 1. Cria uma resposta
        CreateFeedbackRequest createRequest = new()
        {
            SurveyId = 1,
            ContactId = 1,
            Score = 9,
            Channel = "WhatsApp",
            Comment = "ComentarioParaDeletar12345"
        };

        using var postRequest = new HttpRequestMessage(method: HttpMethod.Post, requestUri: "/api/responses");
        postRequest.Headers.Add(name: "X-Company-Id", value: "1");
        postRequest.Content = JsonContent.Create(
            inputValue: createRequest,
            jsonTypeInfo: Api.Serialization.AppJsonSerializerContext.Default.CreateFeedbackRequest);

        HttpResponseMessage postResponse = await _client.SendAsync(
            request: postRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        FeedbackResponse? created = await postResponse.Content.ReadFromJsonAsync(
            jsonTypeInfo: Api.Serialization.AppJsonSerializerContext.Default.FeedbackResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: created);

        // 2. Exclui a resposta logicamente (soft delete)
        using var deleteRequest = new HttpRequestMessage(method: HttpMethod.Delete, requestUri: $"/api/responses/{created.Id}");
        deleteRequest.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage deleteResponse = await _client.SendAsync(
            request: deleteRequest,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.NoContent, actual: deleteResponse.StatusCode);

        // 3. Exporta e verifica que a resposta excluída NÃO consta no CSV
        using var exportRequest = new HttpRequestMessage(method: HttpMethod.Get, requestUri: "/api/responses/export");
        exportRequest.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage exportResponse = await _client.SendAsync(
            request: exportRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: exportResponse.StatusCode);
        string csvContent = await exportResponse.Content.ReadAsStringAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.DoesNotContain(expectedSubstring: "ComentarioParaDeletar12345", actualString: csvContent, comparisonType: StringComparison.Ordinal);
    }
}
