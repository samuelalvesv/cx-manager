using CxManager.Domain.Validation;

namespace CxManager.Domain.Entities;

public sealed class Company : Entity
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 100;
    private const int MinDescriptionLength = 2;
    private const int MaxDescriptionLength = 500;

    public string Name { get; private set; }
    public string Description { get; private set; }

    private Company(
        long id,
        string name,
        string description,
        DateTimeOffset createdAt,
        DateTimeOffset? deletedAt) : base(id: id, createdAt: createdAt, deletedAt: deletedAt)
    {
        Name = name;
        Description = description;
    }

    public static Company Create(
        string name,
        string description)
    {
        string validatedName = ValidateName(name: name);
        string validatedDescription = ValidateDescription(description: description);

        return new Company(
            id: 0,
            name: validatedName,
            description: validatedDescription,
            createdAt: DateTimeOffset.UtcNow,
            deletedAt: null);
    }

    public static Company Reconstitute(
        long id,
        string name,
        string description,
        DateTimeOffset createdAt,
        DateTimeOffset? deletedAt)
    {
        return new Company(
            id: id,
            name: name,
            description: description,
            createdAt: createdAt,
            deletedAt: deletedAt);
    }

    public void Update(string name, string description)
    {
        Name = ValidateName(name: name);
        Description = ValidateDescription(description: description);
    }

    private static string ValidateName(string name) =>
        DomainValidation.ValidateRequiredString(
            value: name,
            fieldName: "nome da empresa",
            minLength: MinNameLength,
            maxLength: MaxNameLength);

    private static string ValidateDescription(string description) =>
        DomainValidation.ValidateRequiredString(
            value: description,
            fieldName: "descrição da empresa",
            minLength: MinDescriptionLength,
            maxLength: MaxDescriptionLength);
}
