using CxManager.Application.Validators.Analytics;
using CxManager.Contract.Requests.Analytics;
using FluentValidation.Results;

namespace CxManager.UnitTests;

public sealed class AnalyticsRequestValidatorTests
{
    private readonly AnalyticsFilterRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WithDefaultValues_ShouldBeValid()
    {
        AnalyticsFilterRequest request = new();
        ValidationResult result = await _validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public async Task Validate_WithNonPositiveSurveyId_ShouldFail(long surveyId)
    {
        AnalyticsFilterRequest request = new() { SurveyId = surveyId };
        ValidationResult result = await _validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(
            collection: result.Errors,
            filter: static e => e.PropertyName == nameof(AnalyticsFilterRequest.SurveyId) &&
                                e.ErrorMessage == "O identificador da pesquisa deve ser positivo.");
    }

    [Fact]
    public async Task Validate_WithPositiveSurveyId_ShouldBeValid()
    {
        AnalyticsFilterRequest request = new() { SurveyId = 1 };
        ValidationResult result = await _validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }

    [Fact]
    public async Task Validate_WithFromDateAfterToDate_ShouldFail()
    {
        AnalyticsFilterRequest request = new()
        {
            From = new DateOnly(year: 2026, month: 6, day: 30),
            To = new DateOnly(year: 2026, month: 1, day: 1)
        };

        ValidationResult result = await _validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.False(condition: result.IsValid);
        Assert.Contains(
            collection: result.Errors,
            filter: static e => e.PropertyName == nameof(AnalyticsFilterRequest.From) &&
                                e.ErrorMessage == "A data inicial não pode ser posterior à data final.");
    }

    [Fact]
    public async Task Validate_WithFromDateBeforeOrEqualToDate_ShouldBeValid()
    {
        AnalyticsFilterRequest request = new()
        {
            From = new DateOnly(year: 2026, month: 1, day: 1),
            To = new DateOnly(year: 2026, month: 6, day: 30)
        };

        ValidationResult result = await _validator.ValidateAsync(
            instance: request,
            cancellation: TestContext.Current.CancellationToken);

        Assert.True(condition: result.IsValid);
        Assert.Empty(collection: result.Errors);
    }
}
