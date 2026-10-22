using CxManager.Application.Validators.Companies;
using CxManager.Contract.Requests.Companies;
using FluentValidation.Results;

namespace CxManager.UnitTests;

public sealed class CompanyRequestValidatorTests
{
    private readonly CreateCompanyRequestValidator _createValidator = new();
    private readonly UpdateCompanyRequestValidator _updateValidator = new();

    [Fact]
    public void CreateCompanyRequest_WithValidFields_ShouldBeValid()
    {
        CreateCompanyRequest request = new()
        {
            Name = "Academia Fit Mais",
            Description = "Rede de musculação e crossfit"
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void CreateCompanyRequest_WithInvalidName_ShouldBeInvalid(string invalidName)
    {
        CreateCompanyRequest request = new()
        {
            Name = invalidName,
            Description = "Descrição válida"
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(CreateCompanyRequest.Name));
    }

    [Fact]
    public void CreateCompanyRequest_WithNameExceeding100Chars_ShouldBeInvalid()
    {
        CreateCompanyRequest request = new()
        {
            Name = new string(c: 'A', count: 101),
            Description = "Descrição válida"
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(CreateCompanyRequest.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("D")]
    public void CreateCompanyRequest_WithInvalidDescription_ShouldBeInvalid(string invalidDescription)
    {
        CreateCompanyRequest request = new()
        {
            Name = "Nome Válido",
            Description = invalidDescription
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(CreateCompanyRequest.Description));
    }

    [Fact]
    public void CreateCompanyRequest_WithDescriptionExceeding500Chars_ShouldBeInvalid()
    {
        CreateCompanyRequest request = new()
        {
            Name = "Nome Válido",
            Description = new string(c: 'D', count: 501)
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(CreateCompanyRequest.Description));
    }

    [Fact]
    public void UpdateCompanyRequest_WithValidFields_ShouldBeValid()
    {
        UpdateCompanyRequest request = new()
        {
            Name = "Academia Fit Mais Atualizada",
            Description = "Rede de musculação e pilates atualizada"
        };

        ValidationResult result = _updateValidator.Validate(instance: request);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void UpdateCompanyRequest_WithInvalidName_ShouldBeInvalid(string invalidName)
    {
        UpdateCompanyRequest request = new()
        {
            Name = invalidName,
            Description = "Descrição válida"
        };

        ValidationResult result = _updateValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(UpdateCompanyRequest.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("D")]
    public void UpdateCompanyRequest_WithInvalidDescription_ShouldBeInvalid(string invalidDescription)
    {
        UpdateCompanyRequest request = new()
        {
            Name = "Nome Válido",
            Description = invalidDescription
        };

        ValidationResult result = _updateValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(UpdateCompanyRequest.Description));
    }
}
