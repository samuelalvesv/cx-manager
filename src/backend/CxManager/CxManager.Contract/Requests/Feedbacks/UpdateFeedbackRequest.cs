namespace CxManager.Contract.Requests.Feedbacks;

public sealed record UpdateFeedbackRequest
{
    public long Id { get; init; }
    public required int Score { get; init; }
    public string? Comment { get; init; } = null;
}
