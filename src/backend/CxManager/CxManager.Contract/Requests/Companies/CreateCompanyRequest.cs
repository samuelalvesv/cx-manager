namespace CxManager.Contract.Requests.Companies;

public sealed record CreateCompanyRequest
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}
