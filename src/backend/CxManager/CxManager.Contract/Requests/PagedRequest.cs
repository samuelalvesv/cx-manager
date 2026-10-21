namespace CxManager.Contract.Requests;

public abstract record PagedRequest
{
    public virtual required int Page { get; init; }
    public virtual required int PageSize { get; init; }
}
