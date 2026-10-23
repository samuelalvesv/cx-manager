using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Application.Models;
using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Responses.Analytics;

namespace CxManager.Application.UseCases.Analytics.Queries.GetNpsBySegment;

public sealed class GetNpsBySegmentUseCase(
    IAnalyticsQueryService queryService,
    IAnalyticsCache cache,
    ICompanyContext companyContext) : IGetNpsBySegmentUseCase
{
    public async Task<IReadOnlyList<SegmentNpsResponse>> ExecuteAsync(AnalyticsFilterRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;
        string cacheKey = $"segment:{request.From:yyyy-MM-dd}:{request.To:yyyy-MM-dd}:{request.SurveyId}";

        IReadOnlyList<SegmentNpsResponse>? cached = cache.Get<IReadOnlyList<SegmentNpsResponse>>(companyId: companyId, key: cacheKey);

        if (cached is not null)
            return cached;

        IReadOnlyList<SegmentNpsModel> models = await queryService.GetNpsBySegmentAsync(
            companyId: companyId,
            from: request.From,
            to: request.To,
            surveyId: request.SurveyId,
            cancellationToken: cancellationToken);

        IReadOnlyList<SegmentNpsResponse> response = AnalyticsMapper.ToResponseList(models: models);
        cache.Set(companyId: companyId, key: cacheKey, value: response);

        return response;
    }
}
