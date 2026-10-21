namespace CxManager.Contract.Requests.Contacts;

public sealed record DeleteContactRequest
{
    public required long Id { get; init; }
}
