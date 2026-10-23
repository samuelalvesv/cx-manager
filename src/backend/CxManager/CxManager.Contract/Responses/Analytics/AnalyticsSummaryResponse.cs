namespace CxManager.Contract.Responses.Analytics;

public sealed record AnalyticsSummaryResponse
{
    public required int NpsScore { get; init; }
    public required int NpsResponses { get; init; }
    public required AnalyticsClassResponse Promoters { get; init; }
    public required AnalyticsClassResponse Neutrals { get; init; }
    public required AnalyticsClassResponse Detractors { get; init; }
    public required int ResponsesCount { get; init; }
    public required double? CsatAvg { get; init; }
}
