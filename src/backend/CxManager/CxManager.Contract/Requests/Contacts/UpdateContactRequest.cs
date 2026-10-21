namespace CxManager.Contract.Requests.Contacts;

public sealed record UpdateContactRequest
{
    public long Id { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Segment { get; init; }
}
