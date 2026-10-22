namespace CxManager.Contract.Responses.Analytics;

public sealed record AnalyticsSummaryResponse
{
    public required int ResponsesCount { get; init; }
    public required int NpsResponses { get; init; }
    public required int PromotersCount { get; init; }
    public required double PromotersPercent { get; init; }
    public required int NeutralsCount { get; init; }
    public required double NeutralsPercent { get; init; }
    public required int DetractorsCount { get; init; }
    public required double DetractorsPercent { get; init; }
    public required int NpsScore { get; init; }
    public required double? CsatAvg { get; init; }
}
