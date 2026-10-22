namespace CxManager.Contract.Requests.Feedbacks;

public sealed record CreateFeedbackRequest
{
    public required long SurveyId { get; init; }
    public required long ContactId { get; init; }
    public required int Score { get; init; }
    public required string Channel { get; init; }
    public string? Comment { get; init; } = null;
}
