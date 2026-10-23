namespace CxManager.Application.Models;

public sealed record FeedbackExportModel
{
    public required long Id { get; init; }
    public required string Survey { get; init; }
    public required string Contact { get; init; }
    public required int Score { get; init; }
    public required string? Comment { get; init; }
    public required string Channel { get; init; }
    public required DateTimeOffset RespondedAt { get; init; }
}
