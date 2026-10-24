using System.Net;
using CxManager.Api.Extensions;
using CxManager.Api.Filters;
using CxManager.Api.Serialization;
using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Companies;

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
    public async Task CreateCompany_WithValidData_ShouldReturn201CreatedAndLocationHeader()
    {
        CreateCompanyRequest request = new()
        {
            Name = "Nova Rede Fitness",
            Description = "Rede inovadora de academias inteligentes"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            requestUri: "/api/companies",
            value: request,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateCompanyRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Created, actual: response.StatusCode);
        Assert.NotNull(@object: response.Headers.Location);

        CompanyResponse? created = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.CompanyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);
        Assert.True(condition: created.Id > 0);
        Assert.Equal(expected: "Nova Rede Fitness", actual: created.Name);
        Assert.Equal(expected: "Rede inovadora de academias inteligentes", actual: created.Description);
        Assert.Equal(expected: $"/api/companies/{created.Id}", actual: response.Headers.Location.OriginalString);
    }

    [Fact]
    public async Task CreateCompany_WithInvalidName_ShouldReturn400BadRequest()
    {
        CreateCompanyRequest request = new()
        {
            Name = "",
            Description = "Descrição válida"
        };

        HttpResponseMessage response = await _client.PostAsJsonAsync(
            requestUri: "/api/companies",
            value: request,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateCompanyRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.False(condition: string.IsNullOrWhiteSpace(value: error.Error));
    }

    [Fact]
    public async Task GetCompanyById_WhenExistsAndActive_ShouldReturn200AndCompanyDetails()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/companies/1", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        CompanyResponse? company = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.CompanyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: company);
        Assert.Equal(expected: 1, actual: company.Id);
        Assert.Equal(expected: "Vita Bem-Estar", actual: company.Name);
    }

    [Fact]
    public async Task GetCompanyById_WhenNotExists_ShouldReturn404NotFound()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/companies/999999", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.Equal(expected: "Empresa não encontrada.", actual: error.Error);
    }

    [Fact]
    public async Task UpdateCompany_WhenExistsAndValid_ShouldReturn200AndUpdatedCompany()
    {
        UpdateCompanyRequest request = new()
        {
            Name = "Vita Bem-Estar Premium",
            Description = "Rede expandida com novas modalidades e pilates"
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            requestUri: "/api/companies/1",
            value: request,
            jsonTypeInfo: AppJsonSerializerContext.Default.UpdateCompanyRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        CompanyResponse? updated = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.CompanyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: updated);
        Assert.Equal(expected: 1, actual: updated.Id);
        Assert.Equal(expected: "Vita Bem-Estar Premium", actual: updated.Name);
        Assert.Equal(expected: "Rede expandida com novas modalidades e pilates", actual: updated.Description);
    }

    [Fact]
    public async Task UpdateCompany_WhenNotExists_ShouldReturn404NotFound()
    {
        UpdateCompanyRequest request = new()
        {
            Name = "Empresa Fantasma",
            Description = "Descrição válida para inexistente"
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            requestUri: "/api/companies/999999",
            value: request,
            jsonTypeInfo: AppJsonSerializerContext.Default.UpdateCompanyRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.Equal(expected: "Empresa não encontrada.", actual: error.Error);
    }

    [Fact]
    public async Task UpdateCompany_WithInvalidName_ShouldReturn400BadRequest()
    {
        UpdateCompanyRequest request = new()
        {
            Name = "",
            Description = "Descrição válida"
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync(
            requestUri: "/api/companies/1",
            value: request,
            jsonTypeInfo: AppJsonSerializerContext.Default.UpdateCompanyRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.False(condition: string.IsNullOrWhiteSpace(value: error.Error));
    }

    [Fact]
    public async Task DeleteCompany_WhenExists_ShouldReturn204NoContent_AndSubsequentGetShouldReturn404()
    {
        CreateCompanyRequest request = new()
        {
            Name = "Empresa Para Excluir",
            Description = "Empresa temporária para validação de soft delete"
        };

        HttpResponseMessage createResponse = await _client.PostAsJsonAsync(
            requestUri: "/api/companies",
            value: request,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateCompanyRequest,
            cancellationToken: TestContext.Current.CancellationToken);

        CompanyResponse? created = await createResponse.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.CompanyResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);

        HttpResponseMessage deleteResponse = await _client.DeleteAsync(
            requestUri: new Uri(uriString: $"/api/companies/{created.Id}", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NoContent, actual: deleteResponse.StatusCode);

        HttpResponseMessage getResponse = await _client.GetAsync(
            requestUri: new Uri(uriString: $"/api/companies/{created.Id}", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: getResponse.StatusCode);

        HttpResponseMessage listResponse = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/companies", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        IReadOnlyList<CompanyResponse>? list = await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<CompanyResponse>>(
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: list);
        Assert.DoesNotContain(collection: list, filter: c => c.Id == created.Id);
    }

    [Fact]
    public async Task DeleteCompany_WhenNotExists_ShouldReturn404NotFound()
    {
        HttpResponseMessage response = await _client.DeleteAsync(
            requestUri: new Uri(uriString: "/api/companies/999999", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: error);
        Assert.Equal(expected: "Empresa não encontrada.", actual: error.Error);
    }

    [Fact]
    public async Task TenantEndpoint_WithoutCompanyId_ShouldFallbackToDefaultCompanyAndReturn204NoContent()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/tenant-test", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NoContent, actual: response.StatusCode);
    }

    [Fact]
    public async Task TenantEndpoint_WithInvalidXCompanyIdHeader_ShouldReturn400BadRequest()
    {
        using HttpRequestMessage request = new(
            method: HttpMethod.Get,
            requestUri: new Uri(uriString: "/api/tenant-test", uriKind: UriKind.Relative));
        request.Headers.Add(name: "X-Company-Id", value: "invalido");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
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

    [Fact]
    public async Task GetCompanyImpact_WithValidCompanyId_ShouldReturnImpactSummary()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/companies/1/impact", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        CompanyImpactSummaryResponse? impact = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.CompanyImpactSummaryResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: impact);
        Assert.Equal(expected: 1, actual: impact.CompanyId);
        Assert.NotEmpty(collection: impact.CompanyName);
        Assert.True(condition: impact.ActiveContactsCount >= 0);
        Assert.True(condition: impact.TotalResponsesCount >= 0);
        Assert.NotNull(@object: impact.Surveys);
    }

    [Fact]
    public async Task GetCompanyImpact_WithNonExistentCompanyId_ShouldReturn404()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/companies/99999/impact", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.NotFound, actual: response.StatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
