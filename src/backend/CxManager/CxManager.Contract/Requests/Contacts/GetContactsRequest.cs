namespace CxManager.Contract.Requests.Contacts;

public sealed record GetContactsRequest : PagedRequest
{
    public override required int Page { get; init; }
    public override required int PageSize { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
}
