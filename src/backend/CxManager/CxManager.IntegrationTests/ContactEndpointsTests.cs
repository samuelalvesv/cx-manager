using System.Net;
using CxManager.Api.Extensions;
using CxManager.Api.Serialization;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Contacts;

namespace CxManager.IntegrationTests;

public sealed class ContactEndpointsTests : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly HttpClient _client;

    public ContactEndpointsTests()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        string dbName = $"contact_endpoint_test_{Guid.NewGuid():N}";
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
    public async Task GetContacts_WithoutCompanyHeader_ShouldFallbackToDefaultCompanyAndReturn200()
    {
        HttpResponseMessage response = await _client.GetAsync(
            requestUri: new Uri(uriString: "/api/contacts?page=1&pageSize=10", uriKind: UriKind.Relative),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        PagedResponse<ContactResponse>? paged = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.PagedResponseContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: paged);
        Assert.Equal(expected: 320, actual: paged.Total);
    }

    [Fact]
    public async Task GetContacts_WithInvalidCompanyHeader_ShouldReturn400BadRequest()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/contacts?page=1&pageSize=10");
        request.Headers.Add("X-Company-Id", "invalido");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);
    }

    [Fact]
    public async Task GetContacts_WithValidCompanyHeader_ShouldReturn200AndPagedContacts()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: "/api/contacts?page=1&pageSize=10");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        PagedResponse<ContactResponse>? paged = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.PagedResponseContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: paged);
        Assert.True(condition: paged.Total > 0);
        Assert.Equal(expected: 1, actual: paged.Page);
        Assert.Equal(expected: 10, actual: paged.PageSize);
        Assert.NotEmpty(collection: paged.Items);
    }

    [Fact]
    public async Task GetContacts_WithSortingParameters_ShouldReturn200AndSortedItems()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: "/api/contacts?page=1&pageSize=10&sortBy=name&sortDir=desc");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        PagedResponse<ContactResponse>? paged = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.PagedResponseContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: paged);
        Assert.True(condition: paged.Items.Count >= 2);
        Assert.True(
            condition: string.Compare(
                strA: paged.Items[0].Name,
                strB: paged.Items[1].Name,
                comparisonType: StringComparison.OrdinalIgnoreCase) >= 0);
    }

    [Fact]
    public async Task GetContacts_WithInvalidSortingParameters_ShouldReturn400BadRequest()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: "/api/contacts?page=1&pageSize=10&sortBy=invalido&sortDir=DESC");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);
    }

    [Fact]
    public async Task GetContacts_WithFtsSearch_ShouldFilterResults()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: "/api/contacts?page=1&pageSize=20&search=costa");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        PagedResponse<ContactResponse>? paged = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.PagedResponseContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: paged);
        Assert.True(condition: paged.Total >= 1);
        Assert.All(
            collection: paged.Items,
            action: static c => Assert.True(
                c.Name.Contains(value: "Costa", comparisonType: StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(value: "costa", comparisonType: StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task GetContacts_WithoutPage_ShouldReturn400BadRequest()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: "/api/contacts?pageSize=20");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: error);
        Assert.Equal(expected: "O número da página deve ser maior ou igual a 1.", actual: error.Error);
    }

    [Fact]
    public async Task GetContacts_WithoutPageSize_ShouldReturn400BadRequest()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: "/api/contacts?page=1");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);

        ErrorResponse? error = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: error);
        Assert.Equal(expected: "O tamanho da página deve estar entre 1 e 100.", actual: error.Error);
    }

    [Fact]
    public async Task GetContactById_WithValidId_ShouldReturn200OkAndContactResponse()
    {
        using var request = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: "/api/contacts/1");
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: response.StatusCode);

        ContactResponse? contact = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: contact);
        Assert.Equal(expected: 1, actual: contact.Id);
        Assert.False(string.IsNullOrWhiteSpace(contact.Name));
        Assert.False(string.IsNullOrWhiteSpace(contact.Email));
        Assert.False(string.IsNullOrWhiteSpace(contact.Segment));
    }

    [Fact]
    public async Task CreateContact_WithValidData_ShouldReturn201CreatedAndLocationHeader()
    {
        CreateContactRequest payload = new()
        {
            Name = "Novo Contato Teste",
            Email = "novo.contato.teste@exemplo.com.br",
            Segment = "Plano Black"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/contacts");
        request.Content = JsonContent.Create(
            inputValue: payload,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Created, actual: response.StatusCode);
        Assert.NotNull(@object: response.Headers.Location);

        ContactResponse? created = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);
        Assert.True(condition: created.Id > 0);
        Assert.Equal(expected: "Novo Contato Teste", actual: created.Name);
        Assert.Equal(expected: "novo.contato.teste@exemplo.com.br", actual: created.Email);
        Assert.Equal(expected: "Plano Black", actual: created.Segment);
        Assert.Equal(expected: $"/api/contacts/{created.Id}", actual: response.Headers.Location.OriginalString);
    }

    [Fact]
    public async Task CreateContact_WithNullSegment_ShouldReturn201CreatedAndNullSegment()
    {
        CreateContactRequest payload = new()
        {
            Name = "Contato Sem Segmento",
            Email = "sem.segmento@exemplo.com.br",
            Segment = null
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/contacts");
        request.Content = JsonContent.Create(
            inputValue: payload,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);
        request.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage response = await _client.SendAsync(
            request: request,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Created, actual: response.StatusCode);

        ContactResponse? created = await response.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: created);
        Assert.Null(@object: created.Segment);
    }

    [Fact]
    public async Task CreateContact_WithDuplicateEmailOnSameCompany_ShouldReturn409Conflict()
    {
        CreateContactRequest payload1 = new()
        {
            Name = "Contato Original",
            Email = "duplicado@exemplo.com.br",
            Segment = "Fit"
        };

        using var req1 = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: "/api/contacts");
        req1.Content = JsonContent.Create(
            inputValue: payload1,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);
        req1.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage resp1 = await _client.SendAsync(
            request: req1,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.Created, actual: resp1.StatusCode);

        // Second creation with same email
        CreateContactRequest payload2 = new()
        {
            Name = "Contato Clonado",
            Email = "DUPLICADO@exemplo.com.br",
            Segment = "Black"
        };

        using var req2 = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: "/api/contacts");
        req2.Content = JsonContent.Create(
            inputValue: payload2,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);
        req2.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage resp2 = await _client.SendAsync(
            request: req2,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.Conflict, actual: resp2.StatusCode);

        ErrorResponse? error = await resp2.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: error);
        Assert.Equal(expected: "Já existe um contato ativo cadastrado com o e-mail informado para esta empresa.", actual: error.Error);
    }

    [Fact]
    public async Task PutContact_WhenSameEmailOnSelf_ShouldSucceed()
    {
        // 1. Create contact
        CreateContactRequest payload = new()
        {
            Name = "Contato Auto Update",
            Email = "auto.update@exemplo.com.br",
            Segment = "Fit"
        };

        using var createReq = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: "/api/contacts");
        createReq.Content = JsonContent.Create(
            inputValue: payload,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);
        createReq.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage createResp = await _client.SendAsync(
            request: createReq,
            cancellationToken: TestContext.Current.CancellationToken);
        ContactResponse? created = await createResp.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: created);

        // 2. Update same contact with same email
        UpdateContactRequest updatePayload = new()
        {
            Name = "Contato Auto Update Modificado",
            Email = "auto.update@exemplo.com.br",
            Segment = "Black"
        };

        using var putReq = new HttpRequestMessage(
            method: HttpMethod.Put,
            requestUri: $"/api/contacts/{created.Id}");
        putReq.Content = JsonContent.Create(
            inputValue: updatePayload,
            jsonTypeInfo: AppJsonSerializerContext.Default.UpdateContactRequest);
        putReq.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage putResp = await _client.SendAsync(
            request: putReq,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(expected: HttpStatusCode.OK, actual: putResp.StatusCode);

        ContactResponse? updated = await putResp.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: updated);
        Assert.Equal(expected: "Contato Auto Update Modificado", actual: updated.Name);
        Assert.Equal(expected: "Black", actual: updated.Segment);
    }

    [Fact]
    public async Task DeleteContact_SoftDelete_ShouldFreeEmailForReuse()
    {
        // 1. Create contact
        CreateContactRequest payload = new()
        {
            Name = "Contato Efêmero",
            Email = "efemero@exemplo.com.br",
            Segment = "Fit"
        };

        using var createReq1 = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: "/api/contacts");
        createReq1.Content = JsonContent.Create(
            inputValue: payload,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);
        createReq1.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage createResp1 = await _client.SendAsync(
            request: createReq1,
            cancellationToken: TestContext.Current.CancellationToken);
        ContactResponse? created = await createResp1.Content.ReadFromJsonAsync(
            jsonTypeInfo: AppJsonSerializerContext.Default.ContactResponse,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(@object: created);
        long contactId = created.Id;

        // 2. Delete contact
        using var deleteReq = new HttpRequestMessage(
            method: HttpMethod.Delete,
            requestUri: $"/api/contacts/{contactId}");
        deleteReq.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage deleteResp = await _client.SendAsync(
            request: deleteReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.NoContent, actual: deleteResp.StatusCode);

        // 3. GET should return 404
        using var getReq = new HttpRequestMessage(
            method: HttpMethod.Get,
            requestUri: $"/api/contacts/{contactId}");
        getReq.Headers.Add("X-Company-Id", "1");
        HttpResponseMessage getResp = await _client.SendAsync(
            request: getReq,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.NotFound, actual: getResp.StatusCode);

        // 4. Re-create contact with same email - should succeed with 201 Created!
        CreateContactRequest payloadReuse = new()
        {
            Name = "Novo Contato com Mesmo Email",
            Email = "efemero@exemplo.com.br",
            Segment = "Black"
        };

        using var createReq2 = new HttpRequestMessage(
            method: HttpMethod.Post,
            requestUri: "/api/contacts");
        createReq2.Content = JsonContent.Create(
            inputValue: payloadReuse,
            jsonTypeInfo: AppJsonSerializerContext.Default.CreateContactRequest);
        createReq2.Headers.Add("X-Company-Id", "1");

        HttpResponseMessage createResp2 = await _client.SendAsync(
            request: createReq2,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected: HttpStatusCode.Created, actual: createResp2.StatusCode);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }
}
