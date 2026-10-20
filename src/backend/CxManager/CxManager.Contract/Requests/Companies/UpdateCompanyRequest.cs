namespace CxManager.Contract.Requests.Companies;

public sealed record UpdateCompanyRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
