namespace CxManager.Contract.Responses.Companies;

public sealed record CompanyResponse
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
