namespace CxManager.Contract.Requests.Surveys;

public sealed record UpdateSurveyRequest
{
    public required string Name { get; init; }
}
