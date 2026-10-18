using System.Net;
using CxManager.Api.Endpoints;
using CxManager.Api.Extensions;
using CxManager.Api.Filters;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Companies;
using CxManager.Infrastructure.Persistence.Schema;
using CxManager.Infrastructure.Persistence.Seed;

namespace CxManager.IntegrationTests;

public sealed class CompanyEndpointsTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public CompanyEndpointsTests()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        string dbName = $"endpoint_test_{Guid.NewGuid():N}";
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

        RouteGroupBuilder apiGroup = _app.MapGroup(prefix: "/api");

        // Test tenant endpoint to verify RequireCompanyContextFilter
        apiGroup.MapGroup(prefix: "/tenant-test")
            .AddEndpointFilter<RequireCompanyContextFilter>()
            .MapGet(pattern: "/", handler: static () => TypedResults.NoContent());

        _app.StartAsync().GetAwaiter().GetResult();

        string baseAddress = _app.Urls.First();
        _client = new HttpClient { BaseAddress = new Uri(uriString: baseAddress) };
    }

    [Fact]
    public async Task GetCompanies_WithoutCompanyHeader_ShouldReturn200AndActiveCompanies()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/companies", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        IReadOnlyList<CompanyResponse>? companies = await response.Content.ReadFromJsonAsync<IReadOnlyList<CompanyResponse>>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: companies);
        Assert.NotEmpty(collection: companies);

        CompanyResponse company = companies[0];
        Assert.Equal(expected: 1, actual: company.Id);
        Assert.Equal(expected: "Vita Bem-Estar", actual: company.Name);
        Assert.Equal(expected: "Rede de academias com 12 unidades no Nordeste", actual: company.Description);
    }

    [Fact]
    public async Task TenantEndpoint_WithoutCompanyId_ShouldReturn400BadRequest()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/tenant-test", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync<ErrorResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.Equal(expected: "O identificador da empresa é obrigatório.", actual: error.Error);
    }

    [Fact]
    public async Task TenantEndpoint_WithXCompanyIdHeader_ShouldReturn204NoContent()
    {
        using HttpRequestMessage request = new(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/tenant-test", uriKind: UriKind.Relative));
        request.Headers.Add(name: "X-Company-Id", value: "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NoContent, actual: response.StatusCode);
    }

    [Fact]
    public async Task TenantEndpoint_WithCompanyIdQueryParam_ShouldReturn204NoContent()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/tenant-test?companyId=1", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NoContent, actual: response.StatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
