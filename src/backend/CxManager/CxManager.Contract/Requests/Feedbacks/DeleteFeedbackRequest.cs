namespace CxManager.Contract.Requests.Feedbacks;

public sealed record DeleteFeedbackRequest
{
    public required long Id { get; init; }
}
