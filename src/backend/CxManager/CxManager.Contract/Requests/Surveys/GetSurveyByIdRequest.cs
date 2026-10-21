namespace CxManager.Contract.Requests.Surveys;

public sealed record GetSurveyByIdRequest
{
    public required long Id { get; init; }
}
