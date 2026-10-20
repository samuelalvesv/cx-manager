namespace CxManager.Application.UseCases.Surveys.Commands.DeleteSurvey;

public interface IDeleteSurveyUseCase
{
    Task ExecuteAsync(long id, CancellationToken cancellationToken = default);
}
