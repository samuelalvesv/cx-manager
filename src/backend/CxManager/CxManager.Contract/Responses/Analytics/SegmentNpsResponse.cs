namespace CxManager.Contract.Responses.Analytics;

public sealed record SegmentNpsResponse
{
    public required string Segment { get; init; }
    public required int Responses { get; init; }
    public required int NpsScore { get; init; }
}
