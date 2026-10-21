namespace CxManager.Contract.Requests.Contacts;

public sealed record GetContactsRequest : PagedRequest
{
    public override required int Page { get; init; }
    public override required int PageSize { get; init; }
    public string? Search { get; init; }
}
