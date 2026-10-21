namespace CxManager.Contract.Requests.Contacts;

public sealed record CreateContactRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Segment { get; init; }
}
