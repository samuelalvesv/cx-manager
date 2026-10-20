using CxManager.Contract.Requests.Surveys;
using CxManager.Contract.Responses.Surveys;

namespace CxManager.Application.UseCases.Surveys.Commands.CreateSurvey;

public interface ICreateSurveyUseCase
{
    Task<SurveyResponse> ExecuteAsync(CreateSurveyRequest request, CancellationToken cancellationToken = default);
}
