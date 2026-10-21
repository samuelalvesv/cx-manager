namespace CxManager.Contract.Requests.Companies;

public sealed record GetCompanyByIdRequest
{
    public required long Id { get; init; }
}
