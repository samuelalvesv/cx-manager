using CxManager.Application.Validators.Contacts;
using CxManager.Contract.Requests.Contacts;
using FluentValidation.Results;

namespace CxManager.UnitTests;

public sealed class ContactRequestValidatorTests
{
    private readonly CreateContactRequestValidator _createValidator = new();
    private readonly UpdateContactRequestValidator _updateValidator = new();

    [Fact]
    public async Task CreateContact_WithValidData_ShouldBeValid()
    {
        CreateContactRequest request = new()
        {
            Name = "Carlos Silva",
            Email = "carlos.silva@exemplo.com.br",
            Segment = "Plano Black"
        };

        ValidationResult result = await _createValidator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public async Task CreateContact_WithInvalidName_ShouldFail(string invalidName)
    {
        CreateContactRequest request = new()
        {
            Name = invalidName,
            Email = "carlos.silva@exemplo.com.br",
            Segment = "Plano Black"
        };

        ValidationResult result = await _createValidator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(CreateContactRequest.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba")]
    [InlineData("@sem-usuario.com")]
    [InlineData("usuario@")]
    public async Task CreateContact_WithInvalidEmail_ShouldFail(string invalidEmail)
    {
        CreateContactRequest request = new()
        {
            Name = "Carlos Silva",
            Email = invalidEmail,
            Segment = "Plano Black"
        };

        ValidationResult result = await _createValidator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(CreateContactRequest.Email));
    }

    [Fact]
    public async Task CreateContact_WithNullSegment_ShouldBeValid()
    {
        CreateContactRequest request = new()
        {
            Name = "Carlos Silva",
            Email = "carlos.silva@exemplo.com.br",
            Segment = null
        };

        ValidationResult result = await _createValidator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Theory]
    [InlineData("A")]
    public async Task CreateContact_WithInvalidSegment_ShouldFail(string invalidSegment)
    {
        CreateContactRequest request = new()
        {
            Name = "Carlos Silva",
            Email = "carlos.silva@exemplo.com.br",
            Segment = invalidSegment
        };

        ValidationResult result = await _createValidator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(CreateContactRequest.Segment));
    }

    [Fact]
    public async Task UpdateContact_WithValidData_ShouldBeValid()
    {
        UpdateContactRequest request = new()
        {
            Name = "Ana Pereira",
            Email = "ana.pereira@exemplo.com.br",
            Segment = "Corporativo"
        };

        ValidationResult result = await _updateValidator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public async Task UpdateContact_WithInvalidData_ShouldFail()
    {
        UpdateContactRequest request = new()
        {
            Name = "A",
            Email = "email-invalido",
            Segment = "A"
        };

        ValidationResult result = await _updateValidator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(UpdateContactRequest.Name));
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(UpdateContactRequest.Email));
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(UpdateContactRequest.Segment));
    }

    [Fact]
    public async Task CreateContact_WithExceededLengths_ShouldFail()
    {
        CreateContactRequest request = new()
        {
            Name = new string('A', 101),
            Email = $"{new string('a', 145)}@exemplo.com",
            Segment = new string('S', 51)
        };

        ValidationResult result = await _createValidator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(CreateContactRequest.Name));
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(CreateContactRequest.Email));
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(CreateContactRequest.Segment));
    }

    [Fact]
    public async Task GetContacts_WithValidData_ShouldBeValid()
    {
        GetContactsRequestValidator validator = new();
        GetContactsRequest request = new()
        {
            Page = 2,
            PageSize = 50,
            Search = "Carlos"
        };

        ValidationResult result = await validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public async Task GetContacts_WithInvalidPage_ShouldFail()
    {
        GetContactsRequestValidator validator = new();
        GetContactsRequest request = new()
        {
            Page = 0,
            PageSize = 20
        };

        ValidationResult result = await validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(GetContactsRequest.Page));
    }

    [Fact]
    public async Task GetContacts_WithInvalidPageSize_ShouldFail()
    {
        GetContactsRequestValidator validator = new();
        GetContactsRequest request = new()
        {
            Page = 1,
            PageSize = 101
        };

        ValidationResult result = await validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(GetContactsRequest.PageSize));
    }

    [Fact]
    public async Task GetContacts_WithOversizedSearch_ShouldFail()
    {
        GetContactsRequestValidator validator = new();
        GetContactsRequest request = new()
        {
            Page = 1,
            PageSize = 20,
            Search = new string('A', 101)
        };

        ValidationResult result = await validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(GetContactsRequest.Search));
    }
}
