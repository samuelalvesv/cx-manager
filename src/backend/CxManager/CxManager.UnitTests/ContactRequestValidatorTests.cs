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

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
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
            Segment = ""
        };

        ValidationResult result = await _updateValidator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.True(condition: result.Errors.Count >= 3);
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

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task GetContactById_WithValidId_ShouldBeValid(long id)
    {
        GetContactByIdRequestValidator validator = new();
        GetContactByIdRequest request = new() { Id = id };

        ValidationResult result = await validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.True(condition: result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetContactById_WithInvalidId_ShouldFail(long id)
    {
        GetContactByIdRequestValidator validator = new();
        GetContactByIdRequest request = new() { Id = id };

        ValidationResult result = await validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(GetContactByIdRequest.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task DeleteContact_WithInvalidId_ShouldFail(long id)
    {
        DeleteContactRequestValidator validator = new();
        DeleteContactRequest request = new() { Id = id };

        ValidationResult result = await validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: static e => e.PropertyName == nameof(DeleteContactRequest.Id));
    }
}
