namespace CxManager.Contract.Responses.Feedbacks;

public sealed record FeedbackResponse
{
    public required long Id { get; init; }
    public required long SurveyId { get; init; }
    public required long ContactId { get; init; }
    public required string SurveyName { get; init; }
    public required string SurveyType { get; init; }
    public required int Score { get; init; }
    public required string? Comment { get; init; }
    public required string Channel { get; init; }
    public required DateTimeOffset RespondedAt { get; init; }
}
