namespace CxManager.Contract.Requests.Surveys;

public sealed record DeleteSurveyRequest
{
    public required long Id { get; init; }
}
