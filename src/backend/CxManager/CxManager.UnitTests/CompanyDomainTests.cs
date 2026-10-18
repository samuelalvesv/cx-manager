using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;

namespace CxManager.UnitTests;

public sealed class CompanyDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateCompany()
    {
        var company = Company.Create(
            name: "Vita Bem-Estar",
            description: "Rede de academias com 12 unidades no Nordeste");

        Assert.Equal(expected: 0, actual: company.Id);
        Assert.Equal(expected: "Vita Bem-Estar", actual: company.Name);
        Assert.Equal(expected: "Rede de academias com 12 unidades no Nordeste", actual: company.Description);
        Assert.Null(@object: company.DeletedAt);
        Assert.False(condition: company.IsDeleted);
        Assert.True(condition: company.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Create_WithInvalidName_ShouldThrowValidationException(string invalidName)
    {
        Assert.Throws<ValidationException>(testCode: () => Company.Create(
            name: invalidName,
            description: "Descrição válida"));
    }

    [Fact]
    public void Create_WithNameExceedingMaximumLength_ShouldThrowValidationException()
    {
        string longName = new('A', count: 101);

        Assert.Throws<ValidationException>(testCode: () => Company.Create(
            name: longName,
            description: "Descrição válida"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Create_WithInvalidDescription_ShouldThrowValidationException(string invalidDescription)
    {
        Assert.Throws<ValidationException>(testCode: () => Company.Create(
            name: "Nome Válido",
            description: invalidDescription));
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaximumLength_ShouldThrowValidationException()
    {
        string longDescription = new('A', count: 501);

        Assert.Throws<ValidationException>(testCode: () => Company.Create(
            name: "Nome Válido",
            description: longDescription));
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateCompanyProperties()
    {
        var company = Company.Create(
            name: "Nome Original",
            description: "Descrição Original");

        company.Update(name: "Nome Atualizado", description: "Nova Descrição Válida");

        Assert.Equal(expected: "Nome Atualizado", actual: company.Name);
        Assert.Equal(expected: "Nova Descrição Válida", actual: company.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Update_WithInvalidName_ShouldThrowValidationException(string invalidName)
    {
        var company = Company.Create(
            name: "Nome Original",
            description: "Descrição Original");

        Assert.Throws<ValidationException>(testCode: () => company.Update(
            name: invalidName,
            description: "Descrição Válida"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Update_WithInvalidDescription_ShouldThrowValidationException(string invalidDescription)
    {
        var company = Company.Create(
            name: "Nome Original",
            description: "Descrição Original");

        Assert.Throws<ValidationException>(testCode: () => company.Update(
            name: "Nome Válido",
            description: invalidDescription));
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtAndIsDeleted()
    {
        var company = Company.Create(
            name: "Nome Original",
            description: "Descrição Original");

        company.SoftDelete();

        Assert.NotNull(@object: company.DeletedAt);
        Assert.True(condition: company.IsDeleted);
    }

    [Fact]
    public void Reconstitute_ShouldPreserveProperties()
    {
        DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddDays(days: -10);
        DateTimeOffset? deletedAt = null;

        var company = Company.Reconstitute(
            id: 1,
            name: "Vita Bem-Estar",
            description: "Rede de academias",
            createdAt: createdAt,
            deletedAt: deletedAt);

        Assert.Equal(expected: 1, actual: company.Id);
        Assert.Equal(expected: "Vita Bem-Estar", actual: company.Name);
        Assert.Equal(expected: "Rede de academias", actual: company.Description);
        Assert.Equal(expected: createdAt, actual: company.CreatedAt);
        Assert.Null(@object: company.DeletedAt);
        Assert.False(condition: company.IsDeleted);
    }
}
