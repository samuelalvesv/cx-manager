using CxManager.Contract.Responses.Surveys;

namespace CxManager.Application.UseCases.Surveys.Queries;

public interface IGetSurveysUseCase
{
    Task<IReadOnlyList<SurveyResponse>> ExecuteAsync(CancellationToken cancellationToken = default);
}
