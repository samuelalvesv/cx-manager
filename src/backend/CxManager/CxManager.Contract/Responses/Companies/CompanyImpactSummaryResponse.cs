namespace CxManager.Contract.Responses.Companies;

public sealed record CompanyImpactSummaryResponse
{
    public required long CompanyId { get; init; }
    public required string CompanyName { get; init; }
    public required int ActiveContactsCount { get; init; }
    public required int TotalResponsesCount { get; init; }
    public required IReadOnlyList<CompanySurveyImpactItemResponse> Surveys { get; init; }
}
