using CxManager.Contract.Responses.Surveys;

namespace CxManager.Application.UseCases.Surveys.Queries.GetSurveyById;

public interface IGetSurveyByIdUseCase
{
    Task<SurveyResponse> ExecuteAsync(long id, CancellationToken cancellationToken = default);
}
