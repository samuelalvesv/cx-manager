using CxManager.Contract.Requests.Surveys;
using CxManager.Contract.Responses.Surveys;

namespace CxManager.Application.UseCases.Surveys.Queries.GetSurveyById;

public interface IGetSurveyByIdUseCase
{
    Task<SurveyResponse> ExecuteAsync(GetSurveyByIdRequest request, CancellationToken cancellationToken = default);
}
