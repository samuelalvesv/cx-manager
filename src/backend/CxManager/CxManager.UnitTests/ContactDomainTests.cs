using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;

namespace CxManager.UnitTests;

public sealed class ContactDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateContact()
    {
        var contact = Contact.Create(
            companyId: 1,
            name: "Fábio Costa",
            email: "fabio.costa@exemplo.com.br",
            segment: "Plano Fit");

        Assert.Equal(expected: 0, actual: contact.Id);
        Assert.Equal(expected: 1, actual: contact.CompanyId);
        Assert.Equal(expected: "Fábio Costa", actual: contact.Name);
        Assert.Equal(expected: "fabio.costa@exemplo.com.br", actual: contact.Email);
        Assert.Equal(expected: "Plano Fit", actual: contact.Segment);
        Assert.Null(@object: contact.DeletedAt);
        Assert.False(condition: contact.IsDeleted);
        Assert.True(condition: contact.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    public void Create_WithInvalidName_ShouldThrowValidationException(string invalidName)
    {
        Assert.Throws<ValidationException>(testCode: () => Contact.Create(
            companyId: 1,
            name: invalidName,
            email: "fabio.costa@exemplo.com.br",
            segment: "Plano Fit"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("sem-arroba")]
    [InlineData("@sem-usuario.com")]
    [InlineData("usuario@")]
    [InlineData("usuario com espaco@exemplo.com")]
    public void Create_WithInvalidEmail_ShouldThrowValidationException(string invalidEmail)
    {
        Assert.Throws<ValidationException>(testCode: () => Contact.Create(
            companyId: 1,
            name: "Fábio Costa",
            email: invalidEmail,
            segment: "Plano Fit"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceSegment_ShouldSetSegmentToNull(string? segment)
    {
        var contact = Contact.Create(
            companyId: 1,
            name: "Fábio Costa",
            email: "fabio.costa@exemplo.com.br",
            segment: segment);

        Assert.Null(@object: contact.Segment);
    }

    [Theory]
    [InlineData("A")]
    public void Create_WithInvalidSegmentLength_ShouldThrowValidationException(string invalidSegment)
    {
        Assert.Throws<ValidationException>(testCode: () => Contact.Create(
            companyId: 1,
            name: "Fábio Costa",
            email: "fabio.costa@exemplo.com.br",
            segment: invalidSegment));
    }

    [Fact]
    public void Create_WithSegmentExceedingMaximumLength_ShouldThrowValidationException()
    {
        string longSegment = new('S', count: 51);

        Assert.Throws<ValidationException>(testCode: () => Contact.Create(
            companyId: 1,
            name: "Fábio Costa",
            email: "fabio.costa@exemplo.com.br",
            segment: longSegment));
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateContactProperties()
    {
        var contact = Contact.Create(
            companyId: 1,
            name: "Nome Antigo",
            email: "antigo@exemplo.com.br",
            segment: "Plano Fit");

        contact.Update(
            name: "Nome Novo",
            email: "novo@exemplo.com.br",
            segment: "Plano Black");

        Assert.Equal(expected: "Nome Novo", actual: contact.Name);
        Assert.Equal(expected: "novo@exemplo.com.br", actual: contact.Email);
        Assert.Equal(expected: "Plano Black", actual: contact.Segment);
    }

    [Fact]
    public void Update_WithNullSegment_ShouldUpdateSegmentToNull()
    {
        var contact = Contact.Create(
            companyId: 1,
            name: "Nome Antigo",
            email: "antigo@exemplo.com.br",
            segment: "Plano Fit");

        contact.Update(
            name: "Nome Novo",
            email: "novo@exemplo.com.br",
            segment: null);

        Assert.Null(@object: contact.Segment);
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtAndIsDeleted()
    {
        var contact = Contact.Create(
            companyId: 1,
            name: "Fábio Costa",
            email: "fabio.costa@exemplo.com.br",
            segment: "Plano Fit");

        contact.SoftDelete();

        Assert.NotNull(@object: contact.DeletedAt);
        Assert.True(condition: contact.IsDeleted);
    }
}
