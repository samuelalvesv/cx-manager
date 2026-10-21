using CxManager.Application.Validators.Surveys;
using CxManager.Contract.Requests.Surveys;
using FluentValidation.Results;

namespace CxManager.UnitTests;

public sealed class SurveyRequestValidatorTests
{
    private readonly CreateSurveyRequestValidator _createValidator = new();
    private readonly UpdateSurveyRequestValidator _updateValidator = new();

    [Theory]
    [InlineData("NPS")]
    [InlineData("CSAT")]
    [InlineData("nps")]
    [InlineData("csat")]
    public void CreateSurveyRequest_WithValidFields_ShouldBeValid(string type)
    {
        CreateSurveyRequest request = new()
        {
            Name = "Pesquisa de Satisfação",
            Type = type
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void CreateSurveyRequest_WithInvalidName_ShouldBeInvalid(string invalidName)
    {
        CreateSurveyRequest request = new()
        {
            Name = invalidName,
            Type = "NPS"
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(CreateSurveyRequest.Name));
    }

    [Fact]
    public void CreateSurveyRequest_WithNameExceeding100Chars_ShouldBeInvalid()
    {
        CreateSurveyRequest request = new()
        {
            Name = new string(c: 'A', count: 101),
            Type = "NPS"
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(CreateSurveyRequest.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("CES")]
    [InlineData("INVALID")]
    public void CreateSurveyRequest_WithInvalidType_ShouldBeInvalid(string invalidType)
    {
        CreateSurveyRequest request = new()
        {
            Name = "Pesquisa Válida",
            Type = invalidType
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(CreateSurveyRequest.Type));
    }

    [Fact]
    public void UpdateSurveyRequest_WithValidFields_ShouldBeValid()
    {
        UpdateSurveyRequest request = new()
        {
            Name = "Pesquisa Atualizada"
        };

        ValidationResult result = _updateValidator.Validate(instance: request);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void UpdateSurveyRequest_WithInvalidName_ShouldBeInvalid(string invalidName)
    {
        UpdateSurveyRequest request = new()
        {
            Name = invalidName
        };

        ValidationResult result = _updateValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(UpdateSurveyRequest.Name));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void GetSurveyByIdRequest_WithValidId_ShouldBeValid(long id)
    {
        GetSurveyByIdRequestValidator validator = new();
        GetSurveyByIdRequest request = new() { Id = id };

        ValidationResult result = validator.Validate(instance: request);

        Assert.True(condition: result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void GetSurveyByIdRequest_WithInvalidId_ShouldBeInvalid(long id)
    {
        GetSurveyByIdRequestValidator validator = new();
        GetSurveyByIdRequest request = new() { Id = id };

        ValidationResult result = validator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(GetSurveyByIdRequest.Id));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeleteSurveyRequest_WithInvalidId_ShouldBeInvalid(long id)
    {
        DeleteSurveyRequestValidator validator = new();
        DeleteSurveyRequest request = new() { Id = id };

        ValidationResult result = validator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(collection: result.Errors, filter: e => e.PropertyName == nameof(DeleteSurveyRequest.Id));
    }
}
