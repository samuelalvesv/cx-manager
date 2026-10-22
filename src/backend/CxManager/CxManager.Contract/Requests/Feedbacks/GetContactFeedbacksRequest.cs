namespace CxManager.Contract.Requests.Feedbacks;

public sealed record GetContactFeedbacksRequest
{
    public required long Id { get; init; }
}
