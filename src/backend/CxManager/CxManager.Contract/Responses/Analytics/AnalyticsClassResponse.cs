namespace CxManager.Contract.Responses.Analytics;

public sealed record AnalyticsClassResponse
{
    public required int Count { get; init; }
    public required double Pct { get; init; }
}
