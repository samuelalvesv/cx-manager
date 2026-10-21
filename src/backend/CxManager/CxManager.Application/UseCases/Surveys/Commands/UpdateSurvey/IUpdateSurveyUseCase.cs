using CxManager.Contract.Requests.Surveys;
using CxManager.Contract.Responses.Surveys;

namespace CxManager.Application.UseCases.Surveys.Commands.UpdateSurvey;

public interface IUpdateSurveyUseCase
{
    Task<SurveyResponse> ExecuteAsync(UpdateSurveyRequest request, CancellationToken cancellationToken = default);
}
