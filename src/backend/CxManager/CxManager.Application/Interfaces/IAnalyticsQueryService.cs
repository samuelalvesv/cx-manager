using CxManager.Application.Models;

namespace CxManager.Application.Interfaces;

public interface IAnalyticsQueryService
{
    Task<AnalyticsSummaryModel> GetSummaryAsync(
        long companyId,
        DateOnly? from,
        DateOnly? to,
        long? surveyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthlyNpsModel>> GetNpsByMonthAsync(
        long companyId,
        DateOnly? from,
        DateOnly? to,
        long? surveyId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SegmentNpsModel>> GetNpsBySegmentAsync(
        long companyId,
        DateOnly? from,
        DateOnly? to,
        long? surveyId,
        CancellationToken cancellationToken = default);
}
