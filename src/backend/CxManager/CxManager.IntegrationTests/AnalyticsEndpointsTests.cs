using System.Net;
using CxManager.Api.Extensions;
using CxManager.Api.Serialization;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Analytics;

namespace CxManager.IntegrationTests;

public sealed class AnalyticsEndpointsTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public AnalyticsEndpointsTests()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        string dbName = $"analytics_endpoint_test_{Guid.NewGuid():N}";
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
    public async Task GetSummary_WithoutFilters_ShouldMatchExactSeedConferenceValues()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary", uriKind: UriKind.Relative));
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        AnalyticsSummaryResponse? summary = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: summary);
        Assert.Equal(expected: 1246, actual: summary.ResponsesCount);
        Assert.Equal(expected: 978, actual: summary.NpsResponses);
        Assert.Equal(expected: 477, actual: summary.PromotersCount);
        Assert.Equal(expected: 256, actual: summary.NeutralsCount);
        Assert.Equal(expected: 245, actual: summary.DetractorsCount);
        Assert.Equal(expected: 48.8, actual: summary.PromotersPercent);
        Assert.Equal(expected: 26.2, actual: summary.NeutralsPercent);
        Assert.Equal(expected: 25.1, actual: summary.DetractorsPercent);
        Assert.Equal(expected: 24, actual: summary.NpsScore);
        Assert.Equal(expected: 3.91, actual: summary.CsatAvg);
    }

    [Fact]
    public async Task GetSummary_FilteredBySurvey1_ShouldReturnNpsScore21()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary?surveyId=1", uriKind: UriKind.Relative));
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        AnalyticsSummaryResponse? summary = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: summary);
        Assert.Equal(expected: 21, actual: summary.NpsScore);
    }

    [Fact]
    public async Task GetSummary_FilteredBySurvey2_ShouldReturnNpsScore29()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary?surveyId=2", uriKind: UriKind.Relative));
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        AnalyticsSummaryResponse? summary = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: summary);
        Assert.Equal(expected: 29, actual: summary.NpsScore);
    }

    [Fact]
    public async Task GetSummary_WithInvalidDateRange_ShouldReturn400BadRequest()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary?from=2026-06-01&to=2026-01-01", uriKind: UriKind.Relative));
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.Contains(expectedSubstring: "A data inicial não pode ser posterior à data final.", actualString: error.Error, comparisonType: StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetNpsByMonth_ShouldMatchSeedMonthlySeries()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/nps-by-month", uriKind: UriKind.Relative));
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        IReadOnlyList<MonthlyNpsResponse>? series = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.IReadOnlyListMonthlyNpsResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: series);
        Assert.Equal(expected: 6, actual: series.Count);

        // Jan: 104 respostas, NPS 17
        Assert.Equal(expected: "2026-01", actual: series[0].Month);
        Assert.Equal(expected: 104, actual: series[0].ResponsesCount);
        Assert.Equal(expected: 17, actual: series[0].NpsScore);

        // Fev: 112 respostas, NPS 26
        Assert.Equal(expected: "2026-02", actual: series[1].Month);
        Assert.Equal(expected: 112, actual: series[1].ResponsesCount);
        Assert.Equal(expected: 26, actual: series[1].NpsScore);

        // Mar: 270 respostas, NPS 11
        Assert.Equal(expected: "2026-03", actual: series[2].Month);
        Assert.Equal(expected: 270, actual: series[2].ResponsesCount);
        Assert.Equal(expected: 11, actual: series[2].NpsScore);

        // Abr: 96 respostas, NPS 14
        Assert.Equal(expected: "2026-04", actual: series[3].Month);
        Assert.Equal(expected: 96, actual: series[3].ResponsesCount);
        Assert.Equal(expected: 14, actual: series[3].NpsScore);

        // Mai: 121 respostas, NPS 22
        Assert.Equal(expected: "2026-05", actual: series[4].Month);
        Assert.Equal(expected: 121, actual: series[4].ResponsesCount);
        Assert.Equal(expected: 22, actual: series[4].NpsScore);

        // Jun: 275 respostas, NPS 42
        Assert.Equal(expected: "2026-06", actual: series[5].Month);
        Assert.Equal(expected: 275, actual: series[5].ResponsesCount);
        Assert.Equal(expected: 42, actual: series[5].NpsScore);
    }

    [Fact]
    public async Task GetNpsBySegment_ShouldMatchSeedSegmentsAndBeOrderedByNpsScoreDescending()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/nps-by-segment", uriKind: UriKind.Relative));
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        IReadOnlyList<SegmentNpsResponse>? segments = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.IReadOnlyListSegmentNpsResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: segments);
        Assert.Equal(expected: 3, actual: segments.Count);

        // Plano Black: 415 respostas, NPS 31
        Assert.Equal(expected: "Plano Black", actual: segments[0].Segment);
        Assert.Equal(expected: 415, actual: segments[0].ResponsesCount);
        Assert.Equal(expected: 31, actual: segments[0].NpsScore);

        // Corporativo: 292 respostas, NPS 19
        Assert.Equal(expected: "Corporativo", actual: segments[1].Segment);
        Assert.Equal(expected: 292, actual: segments[1].ResponsesCount);
        Assert.Equal(expected: 19, actual: segments[1].NpsScore);

        // Plano Fit: 271 respostas, NPS 18
        Assert.Equal(expected: "Plano Fit", actual: segments[2].Segment);
        Assert.Equal(expected: 271, actual: segments[2].ResponsesCount);
        Assert.Equal(expected: 18, actual: segments[2].NpsScore);
    }

    [Fact]
    public async Task Cache_ShouldServeCachedValueAndInvalidateOnMutation()
    {
        // 1. Primeira consulta ao resumo
        using var request1 = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary", uriKind: UriKind.Relative));
        request1.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response1 = await _client.SendAsync(
            request: request1,
            cancellationToken: TestContext.Current.CancellationToken);

        AnalyticsSummaryResponse? initialSummary = await response1.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: initialSummary);
        Assert.Equal(expected: 1246, actual: initialSummary.ResponsesCount);

        // 2. Registrar uma nova resposta (mutação que invalida o cache)
        using var createRequest = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: new Uri(uriString: "/api/responses", uriKind: UriKind.Relative));
        createRequest.Headers.Add(name: "X-Company-Id", value: "1");
        createRequest.Content = JsonContent.Create(
            inputValue: new CreateFeedbackRequest
            {
                SurveyId = 1,
                ContactId = 1,
                Score = 10,
                Channel = "whatsapp",
                Comment = "Novo feedback para teste de invalidação de cache"
            },
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateFeedbackRequest);

        HttpResponseMessage createResponse = await _client.SendAsync(
            request: createRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Created, actual: createResponse.StatusCode);

        // 3. Consultar novamente o resumo e verificar que o valor reflete a inserção (1247)
        using var request2 = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary", uriKind: UriKind.Relative));
        request2.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response2 = await _client.SendAsync(
            request: request2,
            cancellationToken: TestContext.Current.CancellationToken);

        AnalyticsSummaryResponse? updatedSummary = await response2.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: updatedSummary);
        Assert.Equal(expected: 1247, actual: updatedSummary.ResponsesCount);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
