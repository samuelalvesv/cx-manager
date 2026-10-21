namespace CxManager.Contract.Requests.Surveys;

public sealed record UpdateSurveyRequest
{
    public long Id { get; init; }
    public required string Name { get; init; }
}
