namespace CxManager.Contract.Responses.Companies;

public sealed record CompanySurveyImpactItemResponse
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required int ResponsesCount { get; init; }
}
