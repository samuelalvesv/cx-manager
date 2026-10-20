namespace CxManager.Contract.Requests.Surveys;

public sealed record CreateSurveyRequest
{
    public required string Name { get; init; }
    public required string Type { get; init; }
}
