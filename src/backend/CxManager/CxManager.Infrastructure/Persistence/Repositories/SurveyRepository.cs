using System.Data.Common;
using System.Globalization;
using CxManager.Domain.Entities;
using CxManager.Domain.Enums;
using CxManager.Domain.Interfaces;
using Dapper;

namespace CxManager.Infrastructure.Persistence.Repositories;

public sealed class SurveyRepository(IDbConnectionFactory connectionFactory) : ISurveyRepository
{
    public async Task<IReadOnlyList<Survey>> GetAllActiveByCompanyAsync(long companyId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT id AS Id, company_id AS CompanyId, name AS Name, type AS Type, created_at AS CreatedAt, deleted_at AS DeletedAt
            FROM surveys
            WHERE company_id = @CompanyId AND deleted_at IS NULL
            ORDER BY name ASC;
            """;

        IEnumerable<SurveyDbModel> rows = await connection.QueryAsync<SurveyDbModel>(
            sql: sql,
            param: new { CompanyId = companyId });

        return [.. rows.Select(selector: static r => r.ToDomain())];
    }

    public async Task<Survey?> GetByIdAsync(long id, long companyId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT id AS Id, company_id AS CompanyId, name AS Name, type AS Type, created_at AS CreatedAt, deleted_at AS DeletedAt
            FROM surveys
            WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL;
            """;

        SurveyDbModel? row = await connection.QuerySingleOrDefaultAsync<SurveyDbModel>(
            sql: sql,
            param: new { Id = id, CompanyId = companyId });

        return row?.ToDomain();
    }

    public async Task<long> CreateAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            INSERT INTO surveys (company_id, name, type, created_at, deleted_at)
            VALUES (@CompanyId, @Name, @Type, @CreatedAt, NULL);
            SELECT last_insert_rowid();
            """;

        string typeString = survey.Type == SurveyType.Nps ? "NPS" : "CSAT";

        long id = await connection.ExecuteScalarAsync<long>(
            sql: sql,
            param: new
            {
                survey.CompanyId,
                survey.Name,
                Type = typeString,
                CreatedAt = survey.CreatedAt.ToString(format: "O")
            });

        return id;
    }

    public async Task UpdateAsync(Survey survey, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            UPDATE surveys
            SET name = @Name
            WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL;
            """;

        await connection.ExecuteAsync(
            sql: sql,
            param: new
            {
                survey.Name,
                survey.Id,
                survey.CompanyId
            });
    }

    public async Task DeleteAsync(long id, long companyId, DateTimeOffset deletedAt, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            UPDATE surveys
            SET deleted_at = @DeletedAt
            WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL;
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

        const string sql = "SELECT COUNT(1) FROM surveys WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL;";

        int count = await connection.ExecuteScalarAsync<int>(
            sql: sql,
            param: new { Id = id, CompanyId = companyId });

        return count > 0;
    }

    private sealed class SurveyDbModel
    {
        public long Id { get; init; }
        public long CompanyId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string CreatedAt { get; init; } = string.Empty;
        public string? DeletedAt { get; init; }

        public Survey ToDomain()
        {
            var createdAt = DateTimeOffset.Parse(
                input: CreatedAt,
                formatProvider: CultureInfo.InvariantCulture,
                styles: DateTimeStyles.RoundtripKind);

            DateTimeOffset? deletedAt = string.IsNullOrEmpty(value: DeletedAt)
                ? null
                : DateTimeOffset.Parse(
                    input: DeletedAt,
                    formatProvider: CultureInfo.InvariantCulture,
                    styles: DateTimeStyles.RoundtripKind);

            SurveyType surveyType = Type.Trim().ToUpperInvariant() switch
            {
                "NPS" => SurveyType.Nps,
                "CSAT" => SurveyType.Csat,
                _ => Enum.TryParse(value: Type, ignoreCase: true, out SurveyType parsed)
                    ? parsed
                    : throw new InvalidOperationException(message: $"Tipo de pesquisa desconhecido no banco de dados: '{Type}'.")
            };

            return Survey.Reconstitute(
                id: Id,
                companyId: CompanyId,
                name: Name,
                type: surveyType,
                createdAt: createdAt,
                deletedAt: deletedAt);
        }
    }
}
