using System.Data.Common;
using System.Globalization;
using CxManager.Application.Interfaces;
using CxManager.Application.Models;
using CxManager.Domain.Analytics;
using Dapper;

namespace CxManager.Infrastructure.Persistence.Queries;

public sealed class AnalyticsQueryService(IDbConnectionFactory connectionFactory) : IAnalyticsQueryService
{
    public async Task<AnalyticsSummaryModel> GetSummaryAsync(
        long companyId,
        DateOnly? from,
        DateOnly? to,
        long? surveyId,
        CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT
                COUNT(1) AS TotalResponses,
                COUNT(CASE WHEN s.type = 'NPS' THEN 1 END) AS NpsResponses,
                COUNT(CASE WHEN s.type = 'NPS' AND r.score >= 9 AND r.score <= 10 THEN 1 END) AS PromotersCount,
                COUNT(CASE WHEN s.type = 'NPS' AND r.score >= 7 AND r.score <= 8 THEN 1 END) AS NeutralsCount,
                COUNT(CASE WHEN s.type = 'NPS' AND r.score >= 0 AND r.score <= 6 THEN 1 END) AS DetractorsCount,
                AVG(CASE WHEN s.type = 'CSAT' THEN r.score END) AS CsatAvg
            FROM responses r
            INNER JOIN surveys s ON s.id = r.survey_id AND s.company_id = @CompanyId AND s.deleted_at IS NULL
            WHERE r.deleted_at IS NULL
              AND EXISTS (
                  SELECT 1 FROM contacts c
                  WHERE c.id = r.contact_id
                    AND c.company_id = @CompanyId
                    AND c.deleted_at IS NULL
              )
              AND (@SurveyId IS NULL OR r.survey_id = @SurveyId)
              AND (@From IS NULL OR r.responded_at >= @From)
              AND (@To IS NULL OR r.responded_at < @To);
            """;

        object queryParams = BuildQueryParams(
            companyId: companyId,
            from: from,
            to: to,
            surveyId: surveyId);

        CommandDefinition command = new(
            commandText: sql,
            parameters: queryParams,
            cancellationToken: cancellationToken);

        AnalyticsSummaryDbRow row = await connection.QuerySingleAsync<AnalyticsSummaryDbRow>(command: command);

        int npsScore = NpsCalculator.CalculateScore(
            promotersCount: row.PromotersCount,
            detractorsCount: row.DetractorsCount,
            totalNpsResponses: row.NpsResponses);

        double promotersPercent = NpsCalculator.CalculatePercent(
            count: row.PromotersCount,
            totalNpsResponses: row.NpsResponses);

        double neutralsPercent = NpsCalculator.CalculatePercent(
            count: row.NeutralsCount,
            totalNpsResponses: row.NpsResponses);

        double detractorsPercent = NpsCalculator.CalculatePercent(
            count: row.DetractorsCount,
            totalNpsResponses: row.NpsResponses);

        double? csatAvg = NpsCalculator.RoundCsatAverage(rawAverage: row.CsatAvg);

        return new AnalyticsSummaryModel
        {
            ResponsesCount = row.TotalResponses,
            NpsResponses = row.NpsResponses,
            PromotersCount = row.PromotersCount,
            PromotersPercent = promotersPercent,
            NeutralsCount = row.NeutralsCount,
            NeutralsPercent = neutralsPercent,
            DetractorsCount = row.DetractorsCount,
            DetractorsPercent = detractorsPercent,
            NpsScore = npsScore,
            CsatAvg = csatAvg
        };
    }

    public async Task<IReadOnlyList<MonthlyNpsModel>> GetNpsByMonthAsync(
        long companyId,
        DateOnly? from,
        DateOnly? to,
        long? surveyId,
        CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT
                strftime('%Y-%m', r.responded_at) AS Month,
                COUNT(1) AS ResponsesCount,
                COUNT(CASE WHEN r.score >= 9 AND r.score <= 10 THEN 1 END) AS PromotersCount,
                COUNT(CASE WHEN r.score >= 7 AND r.score <= 8 THEN 1 END) AS NeutralsCount,
                COUNT(CASE WHEN r.score >= 0 AND r.score <= 6 THEN 1 END) AS DetractorsCount
            FROM responses r
            INNER JOIN surveys s ON s.id = r.survey_id AND s.company_id = @CompanyId AND s.deleted_at IS NULL
            WHERE r.deleted_at IS NULL
              AND s.type = 'NPS'
              AND EXISTS (
                  SELECT 1 FROM contacts c
                  WHERE c.id = r.contact_id
                    AND c.company_id = @CompanyId
                    AND c.deleted_at IS NULL
              )
              AND (@SurveyId IS NULL OR r.survey_id = @SurveyId)
              AND (@From IS NULL OR r.responded_at >= @From)
              AND (@To IS NULL OR r.responded_at < @To)
            GROUP BY Month
            HAVING ResponsesCount > 0
            ORDER BY Month ASC;
            """;

        object queryParams = BuildQueryParams(
            companyId: companyId,
            from: from,
            to: to,
            surveyId: surveyId);

        CommandDefinition command = new(
            commandText: sql,
            parameters: queryParams,
            cancellationToken: cancellationToken);

        IEnumerable<MonthlyNpsDbRow> rows = await connection.QueryAsync<MonthlyNpsDbRow>(command: command);

        return rows
            .Select(selector: static r =>
            {
                int npsScore = NpsCalculator.CalculateScore(
                    promotersCount: r.PromotersCount,
                    detractorsCount: r.DetractorsCount,
                    totalNpsResponses: r.ResponsesCount);

                return new MonthlyNpsModel
                {
                    Month = r.Month,
                    ResponsesCount = r.ResponsesCount,
                    PromotersCount = r.PromotersCount,
                    NeutralsCount = r.NeutralsCount,
                    DetractorsCount = r.DetractorsCount,
                    NpsScore = npsScore
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<SegmentNpsModel>> GetNpsBySegmentAsync(
        long companyId,
        DateOnly? from,
        DateOnly? to,
        long? surveyId,
        CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT
                COALESCE(c.segment, 'Sem segmento') AS Segment,
                COUNT(1) AS ResponsesCount,
                COUNT(CASE WHEN r.score >= 9 AND r.score <= 10 THEN 1 END) AS PromotersCount,
                COUNT(CASE WHEN r.score >= 7 AND r.score <= 8 THEN 1 END) AS NeutralsCount,
                COUNT(CASE WHEN r.score >= 0 AND r.score <= 6 THEN 1 END) AS DetractorsCount
            FROM responses r
            INNER JOIN surveys s ON s.id = r.survey_id AND s.company_id = @CompanyId AND s.deleted_at IS NULL
            INNER JOIN contacts c ON c.id = r.contact_id AND c.company_id = @CompanyId AND c.deleted_at IS NULL
            WHERE r.deleted_at IS NULL
              AND s.type = 'NPS'
              AND (@SurveyId IS NULL OR r.survey_id = @SurveyId)
              AND (@From IS NULL OR r.responded_at >= @From)
              AND (@To IS NULL OR r.responded_at < @To)
            GROUP BY Segment
            HAVING ResponsesCount > 0;
            """;

        object queryParams = BuildQueryParams(
            companyId: companyId,
            from: from,
            to: to,
            surveyId: surveyId);

        CommandDefinition command = new(
            commandText: sql,
            parameters: queryParams,
            cancellationToken: cancellationToken);

        IEnumerable<SegmentNpsDbRow> rows = await connection.QueryAsync<SegmentNpsDbRow>(command: command);

        return rows
            .Select(selector: static r =>
            {
                int npsScore = NpsCalculator.CalculateScore(
                    promotersCount: r.PromotersCount,
                    detractorsCount: r.DetractorsCount,
                    totalNpsResponses: r.ResponsesCount);

                return new SegmentNpsModel
                {
                    Segment = r.Segment,
                    ResponsesCount = r.ResponsesCount,
                    PromotersCount = r.PromotersCount,
                    NeutralsCount = r.NeutralsCount,
                    DetractorsCount = r.DetractorsCount,
                    NpsScore = npsScore
                };
            })
            .OrderByDescending(keySelector: static x => x.NpsScore)
            .ThenBy(keySelector: static x => x.Segment)
            .ToList();
    }

    private static object BuildQueryParams(long companyId, DateOnly? from, DateOnly? to, long? surveyId)
    {
        string? fromParam = from.HasValue
            ? new DateTimeOffset(date: from.Value, time: TimeOnly.MinValue, offset: TimeSpan.Zero)
                .ToString(format: "O", formatProvider: CultureInfo.InvariantCulture)
            : null;

        string? toParam = to.HasValue
            ? new DateTimeOffset(date: to.Value.AddDays(value: 1), time: TimeOnly.MinValue, offset: TimeSpan.Zero)
                .ToString(format: "O", formatProvider: CultureInfo.InvariantCulture)
            : null;

        return new
        {
            CompanyId = companyId,
            SurveyId = surveyId,
            From = fromParam,
            To = toParam
        };
    }

    private sealed class AnalyticsSummaryDbRow
    {
        public int TotalResponses { get; init; }
        public int NpsResponses { get; init; }
        public int PromotersCount { get; init; }
        public int NeutralsCount { get; init; }
        public int DetractorsCount { get; init; }
        public double? CsatAvg { get; init; }
    }

    private sealed class MonthlyNpsDbRow
    {
        public string Month { get; init; } = string.Empty;
        public int ResponsesCount { get; init; }
        public int PromotersCount { get; init; }
        public int NeutralsCount { get; init; }
        public int DetractorsCount { get; init; }
    }

    private sealed class SegmentNpsDbRow
    {
        public string Segment { get; init; } = string.Empty;
        public int ResponsesCount { get; init; }
        public int PromotersCount { get; init; }
        public int NeutralsCount { get; init; }
        public int DetractorsCount { get; init; }
    }
}
