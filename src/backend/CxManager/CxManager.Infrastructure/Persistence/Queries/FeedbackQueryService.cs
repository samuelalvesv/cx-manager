using System.Data.Common;
using System.Globalization;
using CxManager.Application.Interfaces;
using CxManager.Application.Models;
using Dapper;

namespace CxManager.Infrastructure.Persistence.Queries;

public sealed class FeedbackQueryService(IDbConnectionFactory connectionFactory) : IFeedbackQueryService
{
    public async Task<FeedbackReadModel?> GetDetailByIdAsync(long id, long companyId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT
                r.id AS Id,
                r.survey_id AS SurveyId,
                r.contact_id AS ContactId,
                s.name AS SurveyName,
                s.type AS SurveyType,
                r.score AS Score,
                r.comment AS Comment,
                r.channel AS Channel,
                r.responded_at AS RespondedAt
            FROM responses r
            INNER JOIN surveys s ON s.id = r.survey_id
            INNER JOIN contacts c ON c.id = r.contact_id
            WHERE r.id = @Id
              AND c.company_id = @CompanyId
              AND r.deleted_at IS NULL
              AND s.deleted_at IS NULL
              AND c.deleted_at IS NULL;
            """;

        FeedbackReadDbModel? row = await connection.QuerySingleOrDefaultAsync<FeedbackReadDbModel>(
            sql: sql,
            param: new { Id = id, CompanyId = companyId });

        return row?.ToModel();
    }

    public async Task<IReadOnlyList<FeedbackReadModel>> GetActiveByContactIdAsync(long contactId, long companyId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT
                r.id AS Id,
                r.survey_id AS SurveyId,
                r.contact_id AS ContactId,
                s.name AS SurveyName,
                s.type AS SurveyType,
                r.score AS Score,
                r.comment AS Comment,
                r.channel AS Channel,
                r.responded_at AS RespondedAt
            FROM responses r
            INNER JOIN surveys s ON s.id = r.survey_id
            INNER JOIN contacts c ON c.id = r.contact_id
            WHERE r.contact_id = @ContactId
              AND c.company_id = @CompanyId
              AND r.deleted_at IS NULL
              AND s.deleted_at IS NULL
              AND c.deleted_at IS NULL
            ORDER BY r.responded_at DESC;
            """;

        IEnumerable<FeedbackReadDbModel> rows = await connection.QueryAsync<FeedbackReadDbModel>(
            sql: sql,
            param: new { ContactId = contactId, CompanyId = companyId });

        return [.. rows.Select(selector: static r => r.ToModel())];
    }

    public async Task<IReadOnlyList<FeedbackExportModel>> GetForExportAsync(
        long companyId,
        DateOnly? from,
        DateOnly? to,
        long? surveyId,
        CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT
                r.id AS Id,
                s.name AS Survey,
                c.name AS Contact,
                r.score AS Score,
                r.comment AS Comment,
                r.channel AS Channel,
                r.responded_at AS RespondedAt
            FROM responses r
            INNER JOIN surveys s ON s.id = r.survey_id AND s.company_id = @CompanyId AND s.deleted_at IS NULL
            INNER JOIN contacts c ON c.id = r.contact_id AND c.company_id = @CompanyId AND c.deleted_at IS NULL
            WHERE r.deleted_at IS NULL
              AND (@SurveyId IS NULL OR r.survey_id = @SurveyId)
              AND (@From IS NULL OR r.responded_at >= @From)
              AND (@To IS NULL OR r.responded_at < @To)
            ORDER BY r.responded_at DESC, r.id DESC;
            """;

        string? fromParam = from.HasValue
            ? new DateTimeOffset(date: from.Value, time: TimeOnly.MinValue, offset: TimeSpan.Zero)
                .ToString(format: "O", formatProvider: CultureInfo.InvariantCulture)
            : null;

        string? toParam = to.HasValue
            ? new DateTimeOffset(date: to.Value.AddDays(value: 1), time: TimeOnly.MinValue, offset: TimeSpan.Zero)
                .ToString(format: "O", formatProvider: CultureInfo.InvariantCulture)
            : null;

        var parameters = new
        {
            CompanyId = companyId,
            SurveyId = surveyId,
            From = fromParam,
            To = toParam
        };

        CommandDefinition command = new(
            commandText: sql,
            parameters: parameters,
            cancellationToken: cancellationToken);

        IEnumerable<FeedbackExportDbModel> rows = await connection.QueryAsync<FeedbackExportDbModel>(command: command);

        return [.. rows.Select(selector: static r => r.ToModel())];
    }

    private sealed class FeedbackExportDbModel
    {
        public long Id { get; init; }
        public string Survey { get; init; } = string.Empty;
        public string Contact { get; init; } = string.Empty;
        public int Score { get; init; }
        public string? Comment { get; init; }
        public string Channel { get; init; } = string.Empty;
        public string RespondedAt { get; init; } = string.Empty;

        public FeedbackExportModel ToModel()
        {
            var respondedAt = DateTimeOffset.Parse(
                input: RespondedAt,
                formatProvider: CultureInfo.InvariantCulture,
                styles: DateTimeStyles.RoundtripKind);

            return new FeedbackExportModel
            {
                Id = Id,
                Survey = Survey,
                Contact = Contact,
                Score = Score,
                Comment = Comment,
                Channel = Channel,
                RespondedAt = respondedAt
            };
        }
    }

    private sealed class FeedbackReadDbModel
    {
        public long Id { get; init; }
        public long SurveyId { get; init; }
        public long ContactId { get; init; }
        public string SurveyName { get; init; } = string.Empty;
        public string SurveyType { get; init; } = string.Empty;
        public int Score { get; init; }
        public string? Comment { get; init; }
        public string Channel { get; init; } = string.Empty;
        public string RespondedAt { get; init; } = string.Empty;

        public FeedbackReadModel ToModel()
        {
            var respondedAt = DateTimeOffset.Parse(
                input: RespondedAt,
                formatProvider: CultureInfo.InvariantCulture,
                styles: DateTimeStyles.RoundtripKind);

            return new FeedbackReadModel
            {
                Id = Id,
                SurveyId = SurveyId,
                ContactId = ContactId,
                SurveyName = SurveyName,
                SurveyType = SurveyType.Trim().ToUpperInvariant(),
                Score = Score,
                Comment = Comment,
                Channel = Channel,
                RespondedAt = respondedAt
            };
        }
    }
}
