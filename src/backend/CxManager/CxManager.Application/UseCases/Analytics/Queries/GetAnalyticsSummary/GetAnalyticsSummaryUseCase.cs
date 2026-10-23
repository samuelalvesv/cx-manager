using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Application.Models;
using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Responses.Analytics;

namespace CxManager.Application.UseCases.Analytics.Queries.GetAnalyticsSummary;

public sealed class GetAnalyticsSummaryUseCase(
    IAnalyticsQueryService queryService,
    IAnalyticsCache cache,
    ICompanyContext companyContext) : IGetAnalyticsSummaryUseCase
{
    public async Task<AnalyticsSummaryResponse> ExecuteAsync(AnalyticsFilterRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;
        string cacheKey = $"summary:{request.From:yyyy-MM-dd}:{request.To:yyyy-MM-dd}:{request.SurveyId}";

        AnalyticsSummaryResponse? cached = cache.Get<AnalyticsSummaryResponse>(companyId: companyId, key: cacheKey);

        if (cached is not null)
            return cached;

        AnalyticsSummaryModel model = await queryService.GetSummaryAsync(
            companyId: companyId,
            from: request.From,
            to: request.To,
            surveyId: request.SurveyId,
            cancellationToken: cancellationToken);

        AnalyticsSummaryResponse response = AnalyticsMapper.ToResponse(model: model);
        cache.Set(companyId: companyId, key: cacheKey, value: response);

        return response;
    }
}
