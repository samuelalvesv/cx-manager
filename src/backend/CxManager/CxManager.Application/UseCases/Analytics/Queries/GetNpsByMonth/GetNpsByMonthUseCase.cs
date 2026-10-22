using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Application.Models;
using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Responses.Analytics;

namespace CxManager.Application.UseCases.Analytics.Queries.GetNpsByMonth;

public sealed class GetNpsByMonthUseCase(
    IAnalyticsQueryService queryService,
    IAnalyticsCache cache,
    ICompanyContext companyContext) : IGetNpsByMonthUseCase
{
    public async Task<IReadOnlyList<MonthlyNpsResponse>> ExecuteAsync(GetNpsByMonthRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;
        string cacheKey = $"month:{request.From:yyyy-MM-dd}:{request.To:yyyy-MM-dd}:{request.SurveyId}";

        IReadOnlyList<MonthlyNpsResponse>? cached = cache.Get<IReadOnlyList<MonthlyNpsResponse>>(companyId: companyId, key: cacheKey);

        if (cached is not null)
            return cached;

        IReadOnlyList<MonthlyNpsModel> models = await queryService.GetNpsByMonthAsync(
            companyId: companyId,
            from: request.From,
            to: request.To,
            surveyId: request.SurveyId,
            cancellationToken: cancellationToken);

        IReadOnlyList<MonthlyNpsResponse> response = AnalyticsMapper.ToResponseList(models: models);
        cache.Set(companyId: companyId, key: cacheKey, value: response);

        return response;
    }
}
