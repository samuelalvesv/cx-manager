using System.Data.Common;
using Dapper;

namespace CxManager.Infrastructure.Persistence.Schema;

public sealed class DatabaseInitializer(IDbConnectionFactory connectionFactory) : IDatabaseInitializer
{
    private const string SchemaSql = """
        -- Tabela de empresas para suporte nativo a isolamento multi-empresa
        CREATE TABLE IF NOT EXISTS companies (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            description TEXT NOT NULL,
            created_at TEXT NOT NULL,
            deleted_at TEXT
        );

        -- Tabela de pesquisas vinculada à empresa
        CREATE TABLE IF NOT EXISTS surveys (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            company_id INTEGER NOT NULL REFERENCES companies(id),
            name TEXT NOT NULL,
            type TEXT NOT NULL,
            created_at TEXT NOT NULL,
            deleted_at TEXT
        );

        -- Atende a GET /api/surveys e JOINs de pesquisas ativas:
        -- Otimiza a listagem e os filtros de pesquisas ativas (deleted_at IS NULL) escopadas pela empresa.
        CREATE INDEX IF NOT EXISTS ix_surveys_company_active ON surveys(company_id, deleted_at);

        -- Tabela de contatos vinculada à empresa
        CREATE TABLE IF NOT EXISTS contacts (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            company_id INTEGER NOT NULL REFERENCES companies(id),
            name TEXT NOT NULL,
            email TEXT NOT NULL,
            segment TEXT,
            created_at TEXT NOT NULL,
            deleted_at TEXT
        );

        -- Atende a POST e PUT /api/contacts:
        -- Garante no nível do banco a unicidade case-insensitive de e-mail entre contatos ativos da mesma empresa,
        -- liberando o endereço automaticamente para novos cadastros caso o contato seja excluído logicamente (deleted_at IS NOT NULL).
        CREATE UNIQUE INDEX IF NOT EXISTS ix_contacts_company_email_active ON contacts(company_id, LOWER(email)) WHERE deleted_at IS NULL;

        -- Atende a GET /api/contacts (contagem total e paginação sem busca FTS):
        -- Otimiza a filtragem de contatos ativos escopados pela empresa.
        CREATE INDEX IF NOT EXISTS ix_contacts_company_active ON contacts(company_id, deleted_at);

        -- Atende a GET /api/contacts (ordenação padrão por nome):
        -- Evita custo de ordenação temporária em memória (TEMP B-TREE) na listagem paginada padrão com ORDER BY name ASC.
        CREATE INDEX IF NOT EXISTS ix_contacts_company_name_active ON contacts(company_id, name ASC) WHERE deleted_at IS NULL;

        -- Tabela de respostas vinculada à pesquisa e ao contato
        CREATE TABLE IF NOT EXISTS responses (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            survey_id INTEGER NOT NULL REFERENCES surveys(id),
            contact_id INTEGER NOT NULL REFERENCES contacts(id),
            score INTEGER NOT NULL,
            comment TEXT,
            channel TEXT NOT NULL,
            responded_at TEXT NOT NULL,
            deleted_at TEXT
        );

        -- Atende a GET /api/contacts/{id}/responses:
        -- Permite recuperar o histórico de respostas ativas do contato diretamente na ordem cronológica decrescente (responded_at DESC).
        CREATE INDEX IF NOT EXISTS ix_responses_contact_active ON responses(contact_id, deleted_at, responded_at DESC);

        -- Atende a GET /api/analytics/summary, GET /api/analytics/nps-by-month, GET /api/analytics/nps-by-segment e GET /api/responses/export:
        -- Acelera junções e filtros combinados por pesquisa (survey_id), exclusão lógica e recorte temporal (responded_at) nas agregações SQL.
        CREATE INDEX IF NOT EXISTS ix_responses_survey_active ON responses(survey_id, deleted_at, responded_at);

        -- Tabela virtual FTS5 para busca textual rápida e sem acentuação em nome e e-mail
        CREATE VIRTUAL TABLE IF NOT EXISTS contacts_fts USING fts5(
            name,
            email,
            tokenize = 'trigram'
        );

        -- Trigger para sincronização imediata de novos contatos na tabela virtual FTS5
        CREATE TRIGGER IF NOT EXISTS trg_contacts_ai AFTER INSERT ON contacts BEGIN
            INSERT INTO contacts_fts(rowid, name, email) VALUES (new.id, new.name, new.email);
        END;

        -- Trigger para remoção da tabela virtual FTS5 em exclusão física
        CREATE TRIGGER IF NOT EXISTS trg_contacts_ad AFTER DELETE ON contacts BEGIN
            DELETE FROM contacts_fts WHERE rowid = old.id;
        END;

        -- Trigger para atualização da tabela virtual FTS5 em edição cadastral (limpa contatos com soft delete)
        CREATE TRIGGER IF NOT EXISTS trg_contacts_au AFTER UPDATE ON contacts BEGIN
            DELETE FROM contacts_fts WHERE rowid = old.id;
            INSERT INTO contacts_fts(rowid, name, email)
            SELECT new.id, new.name, new.email
            WHERE new.deleted_at IS NULL;
        END;
        """;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
        await connection.ExecuteAsync(sql: SchemaSql);
    }
}
