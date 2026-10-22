using System.Data.Common;
using System.Globalization;
using CxManager.Domain.Entities;
using CxManager.Domain.Interfaces;
using Dapper;

namespace CxManager.Infrastructure.Persistence.Repositories;

public sealed class ContactRepository(IDbConnectionFactory connectionFactory) : IContactRepository
{
    public async Task<(IReadOnlyList<Contact> Items, int Total)> GetPagedByCompanyAsync(
        long companyId,
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        int offset = (page - 1) * pageSize;
        string? trimmedSearch = search?.Trim();

        if (string.IsNullOrWhiteSpace(value: trimmedSearch))
        {
            const string countSql = """
                SELECT COUNT(1)
                FROM contacts
                WHERE company_id = @CompanyId AND deleted_at IS NULL;
                """;

            int total = await connection.ExecuteScalarAsync<int>(
                sql: countSql,
                param: new { CompanyId = companyId });

            if (total == 0)
                return ([], 0);

            const string selectSql = """
                SELECT id AS Id, company_id AS CompanyId, name AS Name, email AS Email, segment AS Segment, created_at AS CreatedAt, deleted_at AS DeletedAt
                FROM contacts
                WHERE company_id = @CompanyId AND deleted_at IS NULL
                ORDER BY name ASC
                LIMIT @PageSize OFFSET @Offset;
                """;

            IEnumerable<ContactDbModel> rows = await connection.QueryAsync<ContactDbModel>(
                sql: selectSql,
                param: new
                {
                    CompanyId = companyId,
                    PageSize = pageSize,
                    Offset = offset
                });

            return ([.. rows.Select(selector: static r => r.ToDomain())], total);
        }

        string[] tokens = trimmedSearch.Split(separator: ' ', options: StringSplitOptions.RemoveEmptyEntries);
        string ftsQuery = string.Join(
            separator: " ",
            values: tokens.Select(selector: static t => $"\"{t.Replace(oldValue: "\"", newValue: "\"\"", comparisonType: StringComparison.Ordinal)}\""));

        const string countFtsSql = """
            SELECT COUNT(1)
            FROM contacts c
            INNER JOIN contacts_fts fts ON fts.rowid = c.id
            WHERE contacts_fts MATCH @FtsQuery
              AND c.company_id = @CompanyId
              AND c.deleted_at IS NULL;
            """;

        int ftsTotal = await connection.ExecuteScalarAsync<int>(
            sql: countFtsSql,
            param: new
            {
                CompanyId = companyId,
                FtsQuery = ftsQuery
            });

        if (ftsTotal == 0)
            return ([], 0);

        const string selectFtsSql = """
            SELECT c.id AS Id, c.company_id AS CompanyId, c.name AS Name, c.email AS Email, c.segment AS Segment, c.created_at AS CreatedAt, c.deleted_at AS DeletedAt
            FROM contacts c
            INNER JOIN contacts_fts fts ON fts.rowid = c.id
            WHERE contacts_fts MATCH @FtsQuery
              AND c.company_id = @CompanyId
              AND c.deleted_at IS NULL
            ORDER BY c.name ASC
            LIMIT @PageSize OFFSET @Offset;
            """;

        IEnumerable<ContactDbModel> ftsRows = await connection.QueryAsync<ContactDbModel>(
            sql: selectFtsSql,
            param: new
            {
                CompanyId = companyId,
                FtsQuery = ftsQuery,
                PageSize = pageSize,
                Offset = offset
            });

        return ([.. ftsRows.Select(selector: static r => r.ToDomain())], ftsTotal);
    }

    public async Task<Contact?> GetByIdAsync(long id, long companyId, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            SELECT id AS Id, company_id AS CompanyId, name AS Name, email AS Email, segment AS Segment, created_at AS CreatedAt, deleted_at AS DeletedAt
            FROM contacts
            WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL;
            """;

        ContactDbModel? row = await connection.QuerySingleOrDefaultAsync<ContactDbModel>(
            sql: sql,
            param: new { Id = id, CompanyId = companyId });

        return row?.ToDomain();
    }

    public async Task<long> CreateAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            INSERT INTO contacts (company_id, name, email, segment, created_at, deleted_at)
            VALUES (@CompanyId, @Name, @Email, @Segment, @CreatedAt, NULL);
            SELECT last_insert_rowid();
            """;

        long id = await connection.ExecuteScalarAsync<long>(
            sql: sql,
            param: new
            {
                contact.CompanyId,
                contact.Name,
                contact.Email,
                contact.Segment,
                CreatedAt = contact.CreatedAt.ToString(format: "O")
            });

        return id;
    }

    public async Task UpdateAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            UPDATE contacts
            SET name = @Name, email = @Email, segment = @Segment
            WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL;
            """;

        await connection.ExecuteAsync(
            sql: sql,
            param: new
            {
                contact.Name,
                contact.Email,
                contact.Segment,
                contact.Id,
                contact.CompanyId
            });
    }

    public async Task DeleteAsync(long id, long companyId, DateTimeOffset deletedAt, CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        const string sql = """
            UPDATE contacts
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

        const string sql = "SELECT COUNT(1) FROM contacts WHERE id = @Id AND company_id = @CompanyId AND deleted_at IS NULL;";

        int count = await connection.ExecuteScalarAsync<int>(
            sql: sql,
            param: new { Id = id, CompanyId = companyId });

        return count > 0;
    }

    public async Task<bool> ExistsActiveEmailAsync(
        long companyId,
        string email,
        long? excludeContactId = null,
        CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        if (excludeContactId.HasValue)
        {
            const string sqlWithExclude = """
                SELECT COUNT(1)
                FROM contacts
                WHERE company_id = @CompanyId
                  AND LOWER(email) = LOWER(@Email)
                  AND id != @ExcludeId
                  AND deleted_at IS NULL;
                """;

            int count = await connection.ExecuteScalarAsync<int>(
                sql: sqlWithExclude,
                param: new
                {
                    CompanyId = companyId,
                    Email = email,
                    ExcludeId = excludeContactId.Value
                });

            return count > 0;
        }

        const string sql = """
            SELECT COUNT(1)
            FROM contacts
            WHERE company_id = @CompanyId
              AND LOWER(email) = LOWER(@Email)
              AND deleted_at IS NULL;
            """;

        int directCount = await connection.ExecuteScalarAsync<int>(
            sql: sql,
            param: new
            {
                CompanyId = companyId,
                Email = email
            });

        return directCount > 0;
    }

    private sealed class ContactDbModel
    {
        public long Id { get; init; }
        public long CompanyId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? Segment { get; init; }
        public string CreatedAt { get; init; } = string.Empty;
        public string? DeletedAt { get; init; }

        public Contact ToDomain()
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

            return Contact.Reconstitute(
                id: Id,
                companyId: CompanyId,
                name: Name,
                email: Email,
                segment: Segment,
                createdAt: createdAt,
                deletedAt: deletedAt);
        }
    }
}
