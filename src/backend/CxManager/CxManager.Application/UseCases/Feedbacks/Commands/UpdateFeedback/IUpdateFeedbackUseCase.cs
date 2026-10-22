using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;

namespace CxManager.Application.UseCases.Feedbacks.Commands.UpdateFeedback;

public interface IUpdateFeedbackUseCase
{
    Task<FeedbackResponse> ExecuteAsync(UpdateFeedbackRequest request, CancellationToken cancellationToken = default);
}
