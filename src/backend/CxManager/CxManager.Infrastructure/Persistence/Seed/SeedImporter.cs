using System.Data.Common;
using System.Text.Json;
using Dapper;

namespace CxManager.Infrastructure.Persistence.Seed;

public sealed class SeedImporter(IDbConnectionFactory connectionFactory) : ISeedImporter
{
    private const string SeedResourceName = "CxManager.Infrastructure.Data.seed.json";

    public async Task ImportAsync(CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);

        int count = await connection.ExecuteScalarAsync<int>(sql: "SELECT COUNT(1) FROM companies;");
        if (count > 0)
        {
            return;
        }

        using Stream? stream = InfrastructureMarker.Assembly.GetManifestResourceStream(name: SeedResourceName) ?? throw new InvalidOperationException(message: $"O recurso embutido '{SeedResourceName}' não foi encontrado no assembly de infraestrutura.");

        SeedData? seedData = await JsonSerializer.DeserializeAsync(
            utf8Json: stream,
            jsonTypeInfo: SeedJsonSerializerContext.Default.SeedData,
            cancellationToken: cancellationToken) ?? throw new InvalidOperationException(message: "Falha ao desserializar os dados do arquivo de seed.");

        using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken: cancellationToken);

        try
        {
            // 1. Inserir empresa dinâmica
            const string insertCompanySql = """
                INSERT INTO companies (id, name, description, created_at, deleted_at)
                VALUES (@Id, @Name, @Description, @CreatedAt, NULL);
                """;

            await connection.ExecuteAsync(
                sql: insertCompanySql,
                param: new
                {
                    seedData.Company.Id,
                    seedData.Company.Name,
                    seedData.Company.Description,
                    CreatedAt = DateTimeOffset.UtcNow.ToString(format: "O")
                },
                transaction: transaction);

            long dynamicCompanyId = seedData.Company.Id;

            // 2. Inserir pesquisas vinculadas à empresa
            const string insertSurveySql = """
                INSERT INTO surveys (id, company_id, name, type, created_at, deleted_at)
                VALUES (@Id, @CompanyId, @Name, @Type, @CreatedAt, NULL);
                """;

            foreach (SeedSurvey survey in seedData.Surveys)
            {
                await connection.ExecuteAsync(
                    sql: insertSurveySql,
                    param: new
                    {
                        survey.Id,
                        CompanyId = dynamicCompanyId,
                        survey.Name,
                        survey.Type,
                        CreatedAt = survey.CreatedAt.ToString(format: "O")
                    },
                    transaction: transaction);
            }

            // 3. Inserir contatos vinculados à empresa
            const string insertContactSql = """
                INSERT INTO contacts (id, company_id, name, email, segment, created_at, deleted_at)
                VALUES (@Id, @CompanyId, @Name, @Email, @Segment, @CreatedAt, NULL);
                """;

            string contactCreatedAt = DateTimeOffset.UtcNow.ToString(format: "O");
            foreach (SeedContact contact in seedData.Contacts)
            {
                await connection.ExecuteAsync(
                    sql: insertContactSql,
                    param: new
                    {
                        contact.Id,
                        CompanyId = dynamicCompanyId,
                        contact.Name,
                        contact.Email,
                        contact.Segment,
                        CreatedAt = contactCreatedAt
                    },
                    transaction: transaction);
            }

            // 4. Inserir respostas das pesquisas
            const string insertResponseSql = """
                INSERT INTO responses (id, survey_id, contact_id, score, comment, channel, responded_at, deleted_at)
                VALUES (@Id, @SurveyId, @ContactId, @Score, @Comment, @Channel, @RespondedAt, @DeletedAt);
                """;

            foreach (SeedResponse response in seedData.Responses)
            {
                await connection.ExecuteAsync(
                    sql: insertResponseSql,
                    param: new
                    {
                        response.Id,
                        response.SurveyId,
                        response.ContactId,
                        response.Score,
                        response.Comment,
                        response.Channel,
                        RespondedAt = response.RespondedAt.ToString(format: "O"),
                        DeletedAt = response.DeletedAt?.ToString(format: "O")
                    },
                    transaction: transaction);
            }

            await transaction.CommitAsync(cancellationToken: cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken: cancellationToken);
            throw;
        }
    }
}
