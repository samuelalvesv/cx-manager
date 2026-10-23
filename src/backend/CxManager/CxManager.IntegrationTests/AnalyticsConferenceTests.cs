using System.Net;
using CxManager.Api.Extensions;
using CxManager.Api.Serialization;
using CxManager.Contract.Responses.Analytics;

namespace CxManager.IntegrationTests;

public sealed class AnalyticsConferenceTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public AnalyticsConferenceTests()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        string dbName = $"analytics_conference_{Guid.NewGuid():N}";
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

        // Respostas ativas totais no seed (1284 total - 38 soft deleted = 1246)
        Assert.Equal(expected: 1246, actual: summary.ResponsesCount);

        // Respostas de pesquisas do tipo NPS (Pesquisa 1 + Pesquisa 2)
        Assert.Equal(expected: 978, actual: summary.NpsResponses);

        // Promotores (notas 9 e 10): 477 (48.8%)
        Assert.Equal(expected: 477, actual: summary.Promoters.Count);
        Assert.Equal(expected: 48.8, actual: summary.Promoters.Pct);

        // Neutros (notas 7 e 8): 256 (26.2%)
        Assert.Equal(expected: 256, actual: summary.Neutrals.Count);
        Assert.Equal(expected: 26.2, actual: summary.Neutrals.Pct);

        // Detratores (notas 0 a 6): 245 (25.1%)
        Assert.Equal(expected: 245, actual: summary.Detractors.Count);
        Assert.Equal(expected: 25.1, actual: summary.Detractors.Pct);

        // Score NPS arredondado AwayFromZero: 24 (48.773% - 25.051% = 23.722% -> 24)
        Assert.Equal(expected: 24, actual: summary.NpsScore);

        // Média CSAT sobre pesquisas do tipo CSAT (Pesquisa 3): 3.91
        Assert.Equal(expected: 3.91, actual: summary.CsatAvg);
    }

    [Fact]
    public async Task GetNpsByMonth_ShouldMatchSeedMonthlySeriesChronologically()
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

        // Jan/2026: 104 respostas, NPS 17
        Assert.Equal(expected: "2026-01", actual: series[0].Month);
        Assert.Equal(expected: 104, actual: series[0].Responses);
        Assert.Equal(expected: 17, actual: series[0].NpsScore);

        // Fev/2026: 112 respostas, NPS 26
        Assert.Equal(expected: "2026-02", actual: series[1].Month);
        Assert.Equal(expected: 112, actual: series[1].Responses);
        Assert.Equal(expected: 26, actual: series[1].NpsScore);

        // Mar/2026: 270 respostas, NPS 11
        Assert.Equal(expected: "2026-03", actual: series[2].Month);
        Assert.Equal(expected: 270, actual: series[2].Responses);
        Assert.Equal(expected: 11, actual: series[2].NpsScore);

        // Abr/2026: 96 respostas, NPS 14
        Assert.Equal(expected: "2026-04", actual: series[3].Month);
        Assert.Equal(expected: 96, actual: series[3].Responses);
        Assert.Equal(expected: 14, actual: series[3].NpsScore);

        // Mai/2026: 121 respostas, NPS 22
        Assert.Equal(expected: "2026-05", actual: series[4].Month);
        Assert.Equal(expected: 121, actual: series[4].Responses);
        Assert.Equal(expected: 22, actual: series[4].NpsScore);

        // Jun/2026: 275 respostas, NPS 42
        Assert.Equal(expected: "2026-06", actual: series[5].Month);
        Assert.Equal(expected: 275, actual: series[5].Responses);
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

        // 1. Plano Black: 415 respostas, NPS 31
        Assert.Equal(expected: "Plano Black", actual: segments[0].Segment);
        Assert.Equal(expected: 415, actual: segments[0].Responses);
        Assert.Equal(expected: 31, actual: segments[0].NpsScore);

        // 2. Corporativo: 292 respostas, NPS 19
        Assert.Equal(expected: "Corporativo", actual: segments[1].Segment);
        Assert.Equal(expected: 292, actual: segments[1].Responses);
        Assert.Equal(expected: 19, actual: segments[1].NpsScore);

        // 3. Plano Fit: 271 respostas, NPS 18
        Assert.Equal(expected: "Plano Fit", actual: segments[2].Segment);
        Assert.Equal(expected: 271, actual: segments[2].Responses);
        Assert.Equal(expected: 18, actual: segments[2].NpsScore);
    }

    [Fact]
    public async Task GetSummary_FilteredBySurvey1_ShouldMatchSeedValues()
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
    public async Task GetSummary_FilteredBySurvey2_ShouldMatchSeedValues()
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
    public async Task GetSummary_FilteredBySurvey3Csat_ShouldReturnNullNpsAndValidCsatAvg()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/analytics/summary?surveyId=3", uriKind: UriKind.Relative));
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        AnalyticsSummaryResponse? summary = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.AnalyticsSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: summary);
        Assert.Equal(expected: 0, actual: summary.NpsScore);
        Assert.Equal(expected: 0, actual: summary.NpsResponses);
        Assert.Equal(expected: 3.91, actual: summary.CsatAvg);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
