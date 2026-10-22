namespace CxManager.Contract.Requests.Analytics;

public sealed record GetNpsBySegmentRequest
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public long? SurveyId { get; init; }
}
