namespace CxManager.Contract.Requests.Feedbacks;

public sealed record GetFeedbackByIdRequest
{
    public required long Id { get; init; }
}
