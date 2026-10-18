using System.Data.Common;
using System.Globalization;
using CxManager.Domain.Entities;
using CxManager.Domain.Interfaces;
using Dapper;

namespace CxManager.Infrastructure.Persistence.Repositories;

public sealed class CompanyRepository(IDbConnectionFactory connectionFactory) : ICompanyRepository
{
    public async Task<IReadOnlyList<Company>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT id AS Id, name AS Name, description AS Description, created_at AS CreatedAt, deleted_at AS DeletedAt
            FROM companies
            WHERE deleted_at IS NULL
            ORDER BY name ASC;
            """;

        IEnumerable<CompanyDbModel> rows = await connection.QueryAsync<CompanyDbModel>(sql: sql);

        return [.. rows.Select(selector: static r => r.ToDomain())];
    }

    public async Task<Company?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT id AS Id, name AS Name, description AS Description, created_at AS CreatedAt, deleted_at AS DeletedAt
            FROM companies
            WHERE id = @Id AND deleted_at IS NULL;
            """;

        CompanyDbModel? row = await connection.QuerySingleOrDefaultAsync<CompanyDbModel>(
            sql: sql,
            param: new { Id = id });

        return row?.ToDomain();
    }

    public async Task<long> CreateAsync(Company company, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            INSERT INTO companies (name, description, created_at, deleted_at)
            VALUES (@Name, @Description, @CreatedAt, NULL);
            SELECT last_insert_rowid();
            """;

        long id = await connection.ExecuteScalarAsync<long>(
            sql: sql,
            param: new
            {
                company.Name,
                company.Description,
                CreatedAt = company.CreatedAt.ToString(format: "O")
            });

        return id;
    }

    public async Task<bool> ExistsActiveAsync(long id, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = "SELECT COUNT(1) FROM companies WHERE id = @Id AND deleted_at IS NULL;";

        int count = await connection.ExecuteScalarAsync<int>(
            sql: sql,
            param: new { Id = id });

        return count > 0;
    }

    private sealed class CompanyDbModel
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string CreatedAt { get; init; } = string.Empty;
        public string? DeletedAt { get; init; }

        public Company ToDomain()
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

            return Company.Reconstitute(
                id: Id,
                name: Name,
                description: Description,
                createdAt: createdAt,
                deletedAt: deletedAt);
        }
    }
}
