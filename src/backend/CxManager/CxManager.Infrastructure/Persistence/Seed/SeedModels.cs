namespace CxManager.Infrastructure.Persistence.Seed;

public sealed record SeedCompany
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
}

public sealed record SeedSurvey
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed record SeedContact
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Segment { get; init; }
}

public sealed record SeedResponse
{
    public required long Id { get; init; }
    public required long SurveyId { get; init; }
    public required long ContactId { get; init; }
    public required int Score { get; init; }
    public required string? Comment { get; init; }
    public required string Channel { get; init; }
    public required DateTimeOffset RespondedAt { get; init; }
    public required DateTimeOffset? DeletedAt { get; init; }
}

public sealed record SeedData
{
    public required SeedCompany Company { get; init; }
    public required IReadOnlyList<SeedSurvey> Surveys { get; init; }
    public required IReadOnlyList<SeedContact> Contacts { get; init; }
    public required IReadOnlyList<SeedResponse> Responses { get; init; }
}
