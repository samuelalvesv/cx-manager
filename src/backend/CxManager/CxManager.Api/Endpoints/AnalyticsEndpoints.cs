using CxManager.Api.Filters;
using CxManager.Application.UseCases.Analytics.Queries.GetAnalyticsSummary;
using CxManager.Application.UseCases.Analytics.Queries.GetNpsByMonth;
using CxManager.Application.UseCases.Analytics.Queries.GetNpsBySegment;
using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Responses.Analytics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CxManager.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static RouteGroupBuilder MapAnalyticsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(
            pattern: "/summary",
            handler: async Task<Ok<AnalyticsSummaryResponse>> (
                [AsParameters] GetAnalyticsSummaryRequest request,
                IGetAnalyticsSummaryUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                AnalyticsSummaryResponse response = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: response);
            })
            .WithName("GetAnalyticsSummary")
            .WithSummary("Calcula e retorna o resumo analítico de satisfação (NPS e CSAT).")
            .WithValidation<GetAnalyticsSummaryRequest>();

        group.MapGet(
            pattern: "/nps-by-month",
            handler: async Task<Ok<IReadOnlyList<MonthlyNpsResponse>>> (
                [AsParameters] GetNpsByMonthRequest request,
                IGetNpsByMonthUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                IReadOnlyList<MonthlyNpsResponse> response = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: response);
            })
            .WithName("GetNpsByMonth")
            .WithSummary("Agrupa e retorna a série temporal mensal de NPS.")
            .WithValidation<GetNpsByMonthRequest>();

        group.MapGet(
            pattern: "/nps-by-segment",
            handler: async Task<Ok<IReadOnlyList<SegmentNpsResponse>>> (
                [AsParameters] GetNpsBySegmentRequest request,
                IGetNpsBySegmentUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                IReadOnlyList<SegmentNpsResponse> response = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: response);
            })
            .WithName("GetNpsBySegment")
            .WithSummary("Agrupa e retorna os indicadores de NPS por segmento de contato.")
            .WithValidation<GetNpsBySegmentRequest>();

        return group;
    }
}
