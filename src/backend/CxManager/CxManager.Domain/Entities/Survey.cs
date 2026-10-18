using CxManager.Domain.Enums;
using CxManager.Domain.Validation;

namespace CxManager.Domain.Entities;

public sealed class Survey : Entity
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 100;

    public long CompanyId { get; private set; }
    public string Name { get; private set; }
    public SurveyType Type { get; private set; }

    private Survey(
        long id,
        long companyId,
        string name,
        SurveyType type,
        DateTimeOffset createdAt,
        DateTimeOffset? deletedAt) : base(id: id, createdAt: createdAt, deletedAt: deletedAt)
    {
        CompanyId = companyId;
        Name = name;
        Type = type;
    }

    public static Survey Create(
        long companyId,
        string name,
        SurveyType type)
    {
        long validatedCompanyId = ValidateCompanyId(companyId: companyId);
        string validatedName = ValidateName(name: name);

        return new Survey(
            id: 0,
            companyId: validatedCompanyId,
            name: validatedName,
            type: type,
            createdAt: DateTimeOffset.UtcNow,
            deletedAt: null);
    }

    public static Survey Reconstitute(
        long id,
        long companyId,
        string name,
        SurveyType type,
        DateTimeOffset createdAt,
        DateTimeOffset? deletedAt)
    {
        return new Survey(
            id: id,
            companyId: companyId,
            name: name,
            type: type,
            createdAt: createdAt,
            deletedAt: deletedAt);
    }

    public void Update(string name)
        => Name = ValidateName(name: name);

    private static long ValidateCompanyId(long companyId) =>
        DomainValidation.ValidatePositiveId(value: companyId, fieldName: "empresa");

    private static string ValidateName(string name) =>
        DomainValidation.ValidateRequiredString(
            value: name,
            fieldName: "nome da pesquisa",
            minLength: MinNameLength,
            maxLength: MaxNameLength);
}
