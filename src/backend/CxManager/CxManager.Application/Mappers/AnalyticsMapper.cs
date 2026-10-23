using CxManager.Application.Models;
using CxManager.Contract.Responses.Analytics;

namespace CxManager.Application.Mappers;

public static class AnalyticsMapper
{
    public static AnalyticsSummaryResponse ToResponse(AnalyticsSummaryModel model) => new()
    {
        NpsScore = model.NpsScore,
        NpsResponses = model.NpsResponses,
        Promoters = new AnalyticsClassResponse
        {
            Count = model.PromotersCount,
            Pct = model.PromotersPercent
        },
        Neutrals = new AnalyticsClassResponse
        {
            Count = model.NeutralsCount,
            Pct = model.NeutralsPercent
        },
        Detractors = new AnalyticsClassResponse
        {
            Count = model.DetractorsCount,
            Pct = model.DetractorsPercent
        },
        ResponsesCount = model.ResponsesCount,
        CsatAvg = model.CsatAvg
    };

    public static MonthlyNpsResponse ToResponse(MonthlyNpsModel model) => new()
    {
        Month = model.Month,
        Responses = model.ResponsesCount,
        NpsScore = model.NpsScore
    };

    public static IReadOnlyList<MonthlyNpsResponse> ToResponseList(IReadOnlyList<MonthlyNpsModel> models)
    {
        var result = new MonthlyNpsResponse[models.Count];

        for (int i = 0; i < models.Count; i++)
            result[i] = ToResponse(models[i]);

        return result;
    }

    public static SegmentNpsResponse ToResponse(SegmentNpsModel model) => new()
    {
        Segment = model.Segment,
        Responses = model.ResponsesCount,
        NpsScore = model.NpsScore
    };

    public static IReadOnlyList<SegmentNpsResponse> ToResponseList(IReadOnlyList<SegmentNpsModel> models)
    {
        var result = new SegmentNpsResponse[models.Count];

        for (int i = 0; i < models.Count; i++)
            result[i] = ToResponse(models[i]);

        return result;
    }
}
