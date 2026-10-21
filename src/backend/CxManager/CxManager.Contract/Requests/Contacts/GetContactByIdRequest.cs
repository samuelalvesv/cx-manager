namespace CxManager.Contract.Requests.Contacts;

public sealed record GetContactByIdRequest
{
    public required long Id { get; init; }
}
