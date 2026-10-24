namespace CxManager.Contract.Responses.Surveys;

public sealed record SurveyResponse
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required int ResponsesCount { get; init; }
}
