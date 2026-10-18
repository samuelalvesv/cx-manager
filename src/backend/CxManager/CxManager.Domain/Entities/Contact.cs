using CxManager.Domain.Validation;

namespace CxManager.Domain.Entities;

public sealed class Contact : Entity
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 100;
    private const int MinEmailLength = 5;
    private const int MaxEmailLength = 150;
    private const int MinSegmentLength = 2;
    private const int MaxSegmentLength = 50;

    public long CompanyId { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Segment { get; private set; }

    private Contact(
        long id,
        long companyId,
        string name,
        string email,
        string segment,
        DateTimeOffset createdAt,
        DateTimeOffset? deletedAt) : base(id: id, createdAt: createdAt, deletedAt: deletedAt)
    {
        CompanyId = companyId;
        Name = name;
        Email = email;
        Segment = segment;
    }

    public static Contact Create(
        long companyId,
        string name,
        string email,
        string segment)
    {
        long validatedCompanyId = ValidateCompanyId(companyId: companyId);
        string validatedName = ValidateName(name: name);
        string validatedEmail = ValidateEmail(email: email);
        string validatedSegment = ValidateSegment(segment: segment);

        return new Contact(
            id: 0,
            companyId: validatedCompanyId,
            name: validatedName,
            email: validatedEmail,
            segment: validatedSegment,
            createdAt: DateTimeOffset.UtcNow,
            deletedAt: null);
    }

    public static Contact Reconstitute(
        long id,
        long companyId,
        string name,
        string email,
        string segment,
        DateTimeOffset createdAt,
        DateTimeOffset? deletedAt)
    {
        return new Contact(
            id: id,
            companyId: companyId,
            name: name,
            email: email,
            segment: segment,
            createdAt: createdAt,
            deletedAt: deletedAt);
    }

    public void Update(string name, string email, string segment)
    {
        Name = ValidateName(name: name);
        Email = ValidateEmail(email: email);
        Segment = ValidateSegment(segment: segment);
    }

    private static long ValidateCompanyId(long companyId) =>
        DomainValidation.ValidatePositiveId(value: companyId, fieldName: "empresa");

    private static string ValidateName(string name) =>
        DomainValidation.ValidateRequiredString(
            value: name,
            fieldName: "nome do contato",
            minLength: MinNameLength,
            maxLength: MaxNameLength);

    private static string ValidateEmail(string email)
    {
        string validated = DomainValidation.ValidateEmail(
            value: email,
            minLength: MinEmailLength,
            maxLength: MaxEmailLength);

        return validated.ToLowerInvariant();
    }

    private static string ValidateSegment(string segment) =>
        DomainValidation.ValidateRequiredString(
            value: segment,
            fieldName: "segmento",
            minLength: MinSegmentLength,
            maxLength: MaxSegmentLength);
}
