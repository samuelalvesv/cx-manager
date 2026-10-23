using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Responses.Analytics;

namespace CxManager.Application.UseCases.Analytics.Queries.GetNpsBySegment;

public interface IGetNpsBySegmentUseCase
{
    Task<IReadOnlyList<SegmentNpsResponse>> ExecuteAsync(AnalyticsFilterRequest request, CancellationToken cancellationToken = default);
}
