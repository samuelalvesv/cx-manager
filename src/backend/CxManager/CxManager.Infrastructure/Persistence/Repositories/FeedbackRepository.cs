using System.Data.Common;
using System.Globalization;
using CxManager.Domain.Entities;
using CxManager.Domain.Interfaces;
using Dapper;

namespace CxManager.Infrastructure.Persistence.Repositories;

public sealed class FeedbackRepository(IDbConnectionFactory connectionFactory) : IFeedbackRepository
{
    public async Task<long> CreateAsync(Feedback feedback, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            INSERT INTO responses (survey_id, contact_id, score, comment, channel, responded_at, deleted_at)
            VALUES (@SurveyId, @ContactId, @Score, @Comment, @Channel, @RespondedAt, NULL);
            SELECT last_insert_rowid();
            """;

        long id = await connection.ExecuteScalarAsync<long>(
            sql: sql,
            param: new
            {
                feedback.SurveyId,
                feedback.ContactId,
                feedback.Score,
                feedback.Comment,
                feedback.Channel,
                RespondedAt = feedback.RespondedAt.ToString(format: "O")
            });

        return id;
    }

    public async Task<Feedback?> GetByIdAsync(long id, long companyId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT
                r.id AS Id,
                r.survey_id AS SurveyId,
                r.contact_id AS ContactId,
                r.score AS Score,
                r.comment AS Comment,
                r.channel AS Channel,
                r.responded_at AS RespondedAt,
                r.deleted_at AS DeletedAt
            FROM responses r
            INNER JOIN contacts c ON c.id = r.contact_id
            WHERE r.id = @Id
              AND c.company_id = @CompanyId
              AND r.deleted_at IS NULL
              AND c.deleted_at IS NULL;
            """;

        FeedbackDbModel? row = await connection.QuerySingleOrDefaultAsync<FeedbackDbModel>(
            sql: sql,
            param: new { Id = id, CompanyId = companyId });

        return row?.ToDomain();
    }

    public async Task UpdateAsync(Feedback feedback, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            UPDATE responses
            SET score = @Score, comment = @Comment
            WHERE id = @Id AND deleted_at IS NULL;
            """;

        await connection.ExecuteAsync(
            sql: sql,
            param: new
            {
                feedback.Id,
                feedback.Score,
                feedback.Comment
            });
    }

    public async Task DeleteAsync(long id, long companyId, DateTimeOffset deletedAt, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            UPDATE responses
            SET deleted_at = @DeletedAt
            WHERE id = @Id
              AND deleted_at IS NULL
              AND contact_id IN (SELECT id FROM contacts WHERE company_id = @CompanyId);
            """;

        await connection.ExecuteAsync(
            sql: sql,
            param: new
            {
                Id = id,
                CompanyId = companyId,
                DeletedAt = deletedAt.ToString(format: "O")
            });
    }

    public async Task<bool> ExistsActiveAsync(long id, long companyId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT COUNT(1)
            FROM responses r
            INNER JOIN contacts c ON c.id = r.contact_id
            WHERE r.id = @Id
              AND c.company_id = @CompanyId
              AND r.deleted_at IS NULL
              AND c.deleted_at IS NULL;
            """;

        int count = await connection.ExecuteScalarAsync<int>(
            sql: sql,
            param: new { Id = id, CompanyId = companyId });

        return count > 0;
    }

    private sealed class FeedbackDbModel
    {
        public long Id { get; init; }
        public long SurveyId { get; init; }
        public long ContactId { get; init; }
        public int Score { get; init; }
        public string? Comment { get; init; }
        public string Channel { get; init; } = string.Empty;
        public string RespondedAt { get; init; } = string.Empty;
        public string? DeletedAt { get; init; }

        public Feedback ToDomain()
        {
            var respondedAt = DateTimeOffset.Parse(
                input: RespondedAt,
                formatProvider: CultureInfo.InvariantCulture,
                styles: DateTimeStyles.RoundtripKind);

            DateTimeOffset? deletedAt = string.IsNullOrEmpty(value: DeletedAt)
                ? null
                : DateTimeOffset.Parse(
                    input: DeletedAt,
                    formatProvider: CultureInfo.InvariantCulture,
                    styles: DateTimeStyles.RoundtripKind);

            return Feedback.Reconstitute(
                id: Id,
                surveyId: SurveyId,
                contactId: ContactId,
                score: Score,
                comment: Comment,
                channel: Channel,
                respondedAt: respondedAt,
                deletedAt: deletedAt);
        }
    }
}
