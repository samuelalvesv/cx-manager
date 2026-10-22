using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;

namespace CxManager.Application.UseCases.Feedbacks.Queries.GetFeedbackById;

public interface IGetFeedbackByIdUseCase
{
    Task<FeedbackResponse> ExecuteAsync(GetFeedbackByIdRequest request, CancellationToken cancellationToken = default);
}
