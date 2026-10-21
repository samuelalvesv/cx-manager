namespace CxManager.Contract.Requests.Companies;

public sealed record DeleteCompanyRequest
{
    public required long Id { get; init; }
}
