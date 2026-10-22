using CxManager.Application.Models;
using CxManager.Contract.Responses.Analytics;
using Riok.Mapperly.Abstractions;

namespace CxManager.Application.Mappers;

[Mapper]
public static partial class AnalyticsMapper
{
    public static partial AnalyticsSummaryResponse ToResponse(AnalyticsSummaryModel model);

    public static partial MonthlyNpsResponse ToResponse(MonthlyNpsModel model);

    public static partial IReadOnlyList<MonthlyNpsResponse> ToResponseList(IReadOnlyList<MonthlyNpsModel> models);

    public static partial SegmentNpsResponse ToResponse(SegmentNpsModel model);

    public static partial IReadOnlyList<SegmentNpsResponse> ToResponseList(IReadOnlyList<SegmentNpsModel> models);
}
