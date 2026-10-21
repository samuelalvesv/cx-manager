using CxManager.Contract.Requests.Surveys;

namespace CxManager.Application.UseCases.Surveys.Commands.DeleteSurvey;

public interface IDeleteSurveyUseCase
{
    Task ExecuteAsync(DeleteSurveyRequest request, CancellationToken cancellationToken = default);
}
