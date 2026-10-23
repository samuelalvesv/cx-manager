using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Responses.Analytics;

namespace CxManager.Application.UseCases.Analytics.Queries.GetAnalyticsSummary;

public interface IGetAnalyticsSummaryUseCase
{
    Task<AnalyticsSummaryResponse> ExecuteAsync(AnalyticsFilterRequest request, CancellationToken cancellationToken = default);
}
