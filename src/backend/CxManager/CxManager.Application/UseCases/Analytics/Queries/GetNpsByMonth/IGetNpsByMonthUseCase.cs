using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Responses.Analytics;

namespace CxManager.Application.UseCases.Analytics.Queries.GetNpsByMonth;

public interface IGetNpsByMonthUseCase
{
    Task<IReadOnlyList<MonthlyNpsResponse>> ExecuteAsync(AnalyticsFilterRequest request, CancellationToken cancellationToken = default);
}
