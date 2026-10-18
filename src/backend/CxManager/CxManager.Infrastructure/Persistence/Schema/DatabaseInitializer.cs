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

        -- Índice para otimizar listagem e filtros de pesquisas ativas por empresa
        CREATE INDEX IF NOT EXISTS ix_surveys_company_active ON surveys(company_id, deleted_at);

        -- Tabela de contatos vinculada à empresa
        CREATE TABLE IF NOT EXISTS contacts (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            company_id INTEGER NOT NULL REFERENCES companies(id),
            name TEXT NOT NULL,
            email TEXT NOT NULL,
            segment TEXT NOT NULL,
            created_at TEXT NOT NULL,
            deleted_at TEXT
        );

        -- Índice exclusivo parcial garantindo unicidade de e-mail ativo por empresa (liberado após soft delete)
        CREATE UNIQUE INDEX IF NOT EXISTS ix_contacts_company_email_active ON contacts(company_id, LOWER(email)) WHERE deleted_at IS NULL;

        -- Índice para busca e paginação de contatos ativos por empresa
        CREATE INDEX IF NOT EXISTS ix_contacts_company_active ON contacts(company_id, deleted_at);

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

        -- Índice para listagem cronológica do histórico de respostas ativas por contato
        CREATE INDEX IF NOT EXISTS ix_responses_contact_active ON responses(contact_id, deleted_at, responded_at DESC);

        -- Índice para agregações analíticas de respostas ativas por pesquisa e período
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

        -- Trigger para atualização da tabela virtual FTS5 em edição cadastral
        CREATE TRIGGER IF NOT EXISTS trg_contacts_au AFTER UPDATE ON contacts BEGIN
            DELETE FROM contacts_fts WHERE rowid = old.id;
            INSERT INTO contacts_fts(rowid, name, email) VALUES (new.id, new.name, new.email);
        END;
        """;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using DbConnection connection = await connectionFactory.CreateConnectionAsync(cancellationToken: cancellationToken);
        await connection.ExecuteAsync(sql: SchemaSql);
    }
}
