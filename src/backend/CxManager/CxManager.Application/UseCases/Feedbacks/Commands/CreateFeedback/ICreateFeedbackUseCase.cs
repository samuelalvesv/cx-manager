using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;

namespace CxManager.Application.UseCases.Feedbacks.Commands.CreateFeedback;

public interface ICreateFeedbackUseCase
{
    Task<FeedbackResponse> ExecuteAsync(CreateFeedbackRequest request, CancellationToken cancellationToken = default);
}
