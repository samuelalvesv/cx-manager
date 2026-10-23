using CxManager.Contract.Requests.Analytics;

namespace CxManager.Application.UseCases.Feedbacks.Queries.ExportFeedbacks;

public interface IExportFeedbacksUseCase
{
    Task<byte[]> ExecuteAsync(AnalyticsFilterRequest request, CancellationToken cancellationToken = default);
}
