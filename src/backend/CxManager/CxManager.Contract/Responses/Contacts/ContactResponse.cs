namespace CxManager.Contract.Responses.Contacts;

public sealed record ContactResponse
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string? Segment { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
