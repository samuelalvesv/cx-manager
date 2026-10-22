namespace CxManager.Contract.Responses.Analytics;

public sealed record SegmentNpsResponse
{
    public required string Segment { get; init; }
    public required int ResponsesCount { get; init; }
    public required int PromotersCount { get; init; }
    public required int NeutralsCount { get; init; }
    public required int DetractorsCount { get; init; }
    public required int NpsScore { get; init; }
}
