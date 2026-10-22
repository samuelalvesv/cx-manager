using CxManager.Contract.Requests.Feedbacks;

namespace CxManager.Application.UseCases.Feedbacks.Commands.DeleteFeedback;

public interface IDeleteFeedbackUseCase
{
    Task ExecuteAsync(DeleteFeedbackRequest request, CancellationToken cancellationToken = default);
}
