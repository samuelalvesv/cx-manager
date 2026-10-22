using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;

namespace CxManager.Application.UseCases.Feedbacks.Queries.GetContactFeedbacks;

public interface IGetContactFeedbacksUseCase
{
    Task<IReadOnlyList<FeedbackResponse>> ExecuteAsync(GetContactFeedbacksRequest request, CancellationToken cancellationToken = default);
}
