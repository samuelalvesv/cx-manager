using CxManager.Domain.Entities;
using CxManager.Domain.Enums;
using CxManager.Domain.Exceptions;

namespace CxManager.UnitTests;

public sealed class SurveyDomainTests
{
    [Fact]
    public void Create_WithValidParametersNps_ShouldCreateSurvey()
    {
        var survey = Survey.Create(
            companyId: 1,
            name: "NPS Pós-Treino",
            type: SurveyType.Nps);

        Assert.Equal(expected: 0, actual: survey.Id);
        Assert.Equal(expected: 1, actual: survey.CompanyId);
        Assert.Equal(expected: "NPS Pós-Treino", actual: survey.Name);
        Assert.Equal(expected: SurveyType.Nps, actual: survey.Type);
        Assert.Null(@object: survey.DeletedAt);
        Assert.False(condition: survey.IsDeleted);
        Assert.True(condition: survey.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Create_WithValidParametersCsat_ShouldCreateSurvey()
    {
        var survey = Survey.Create(
            companyId: 2,
            name: "CSAT Atendimento",
            type: SurveyType.Csat);

        Assert.Equal(expected: 0, actual: survey.Id);
        Assert.Equal(expected: 2, actual: survey.CompanyId);
        Assert.Equal(expected: "CSAT Atendimento", actual: survey.Name);
        Assert.Equal(expected: SurveyType.Csat, actual: survey.Type);
        Assert.Null(@object: survey.DeletedAt);
        Assert.False(condition: survey.IsDeleted);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidCompanyId_ShouldThrowValidationException(long invalidCompanyId)
    {
        Assert.Throws<ValidationException>(testCode: () => Survey.Create(
            companyId: invalidCompanyId,
            name: "Pesquisa Teste",
            type: SurveyType.Nps));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Create_WithInvalidName_ShouldThrowValidationException(string invalidName)
    {
        Assert.Throws<ValidationException>(testCode: () => Survey.Create(
            companyId: 1,
            name: invalidName,
            type: SurveyType.Nps));
    }

    [Fact]
    public void Create_WithNameExceedingMaximumLength_ShouldThrowValidationException()
    {
        string longName = new('A', count: 101);

        Assert.Throws<ValidationException>(testCode: () => Survey.Create(
            companyId: 1,
            name: longName,
            type: SurveyType.Nps));
    }

    [Fact]
    public void Create_WithUndefinedSurveyType_ShouldThrowValidationException()
    {
        Assert.Throws<ValidationException>(testCode: () => Survey.Create(
            companyId: 1,
            name: "Pesquisa Teste",
            type: (SurveyType)999));
    }

    [Fact]
    public void Update_WithValidName_ShouldUpdateSurveyName()
    {
        var survey = Survey.Create(
            companyId: 1,
            name: "Nome Original",
            type: SurveyType.Nps);

        survey.Update(name: "Nome Atualizado");

        Assert.Equal(expected: "Nome Atualizado", actual: survey.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Update_WithInvalidName_ShouldThrowValidationException(string invalidName)
    {
        var survey = Survey.Create(
            companyId: 1,
            name: "Nome Original",
            type: SurveyType.Nps);

        Assert.Throws<ValidationException>(testCode: () => survey.Update(name: invalidName));
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtAndIsDeleted()
    {
        var survey = Survey.Create(
            companyId: 1,
            name: "Pesquisa para Deletar",
            type: SurveyType.Nps);

        survey.SoftDelete();

        Assert.NotNull(@object: survey.DeletedAt);
        Assert.True(condition: survey.IsDeleted);
    }

    [Fact]
    public void Reconstitute_ShouldPreserveProperties()
    {
        DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddDays(days: -5);
        DateTimeOffset? deletedAt = null;

        var survey = Survey.Reconstitute(
            id: 42,
            companyId: 10,
            name: "Pesquisa Reconstituída",
            type: SurveyType.Csat,
            createdAt: createdAt,
            deletedAt: deletedAt);

        Assert.Equal(expected: 42, actual: survey.Id);
        Assert.Equal(expected: 10, actual: survey.CompanyId);
        Assert.Equal(expected: "Pesquisa Reconstituída", actual: survey.Name);
        Assert.Equal(expected: SurveyType.Csat, actual: survey.Type);
        Assert.Equal(expected: createdAt, actual: survey.CreatedAt);
        Assert.Null(@object: survey.DeletedAt);
        Assert.False(condition: survey.IsDeleted);
    }
}
