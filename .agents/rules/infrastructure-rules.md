---
name: infrastructure-rules
description: SQLite database schema, handwritten SQL queries, Dapper and Dapper.AOT rules, named parameters security, SQL aggregations, and seed importer guidelines.
applyTo: "src/backend/CxManager/CxManager.Infrastructure/**/*.{cs,sql}"
---

# Regras de Infraestrutura e Banco de Dados (CxManager.Infrastructure)

## Estrutura de Pastas
- `Persistence/SqliteConnectionFactory.cs`: Fábrica de conexões SQLite (`IDbConnectionFactory`).
- `Persistence/Schema/`: Inicializador do schema relacional SQLite (tabelas, chaves estrangeiras e índices documentados).
- `Persistence/Seed/`: Importador reproduzível e idempotente do arquivo de carga inicial (`seed.json`) utilizando `System.Text.Json` Source Generation e transação única.
- `Persistence/Repositories/`: Implementações concretas dos repositórios utilizando **Dapper + Dapper.AOT** e SQL escrito à mão.
- `DependencyInjection.cs`: Registro unificado dos serviços e repositórios de infraestrutura.

## Diretrizes de Acesso a Dados (Dapper + SQL Escrito à Mão)
- **Proibição Absoluta de ORMs Completos**: É estritamente proibido o uso de Entity Framework Core, NHibernate ou qualquer gerador automático de queries. Todo acesso a dados deve ser realizado via **Dapper** com SQL escrito explicitamente.
- **Compatibilidade com Native AOT (`Dapper.AOT`)**: Habilitar `[module: DapperAot]` no nível do assembly de infraestrutura para que o mapeamento de parâmetros e colunas seja gerado em tempo de compilação via Source Generator em todos os repositórios, sem reflexão ou `Reflection.Emit` em runtime.
- **Segurança Inviolável de Parâmetros Nomeados**:
  - 100% dos valores dinâmicos e filtros de usuário (`search`, `id`, `from`, `to`, `surveyId`, `page`, `pageSize`, dados cadastrais) devem ser passados via **parâmetros SQL nomeados (`@param`)**.
  - É expressamente proibido montar queries SQL por concatenação (`+`) ou interpolação de strings (`$""`). A regra `CA2100` é tratada como erro de compilação.
- **Agregações e Paginação 100% no Banco**:
  - Cálculos analíticos (`COUNT`, `SUM`, `AVG`, `CASE WHEN`, `GROUP BY`, `JOIN`) e paginação (`LIMIT @pageSize OFFSET @offset`) devem ocorrer integralmente no motor SQLite.
  - Repositórios nunca devem retornar `IQueryable` nem carregar tabelas inteiras para a memória para filtrar, paginar ou agregar com LINQ no C#.
  - Consultas de coleção devem ser materializadas na camada de infraestrutura retornando `IReadOnlyList<T>` e consultas unitárias retornando `T?`.
- **Integridade de Dados, Soft Delete e Simetria de Nulabilidade**:
  - Seguir integralmente [.agents/rules/data-integrity-rules.md](data-integrity-rules.md).
  - O schema DDL do SQLite e as queries devem respeitar estritamente a Cadeia Canônica de Simetria de Tipos e Nulabilidade definida em `data-integrity-rules.md`, sendo mandatório o uso de `NOT NULL` em colunas correspondentes a propriedades obrigatórias de domínio.
  - Toda query sobre contatos ou respostas deve filtrar explicitamente `deleted_at IS NULL`.

## Convenções de Modelagem e Escrita SQL (SQLite)
- **Palavras-chave SQL**: Escrever sempre em `MAIÚSCULAS` (`SELECT`, `FROM`, `INNER JOIN`, `WHERE`, `GROUP BY`, `ORDER BY`, `CASE WHEN`, `LIMIT`, `OFFSET`).
- **Identificadores (Tabelas, Colunas e Índices)**: Sempre em minúsculas e `snake_case` (`surveys`, `contacts`, `responses`, `survey_id`, `contact_id`, `responded_at`, `deleted_at`).
- **Persistência de Datas em UTC**: Colunas temporais no SQLite devem ser armazenadas como `TEXT` em formato ISO 8601 UTC (`YYYY-MM-DDTHH:mm:ssZ`).
- **Integridade Referencial**: Habilitar `PRAGMA foreign_keys = ON;` em toda conexão aberta pela fábrica de conexões e definir `FOREIGN KEY` explícitas no schema.
- **Índices Documentados**: Criar índices explícitos onde há impacto real de consulta (busca/unicidade de e-mail ativo em contatos, listagem de respostas por contato, filtros de respostas ativas por pesquisa e período `responded_at`), acompanhados de comentário SQL explicando a razão técnica de cada índice.
- **Importação do Seed (`seed.json`) e Autossuficiência de Dados**:
  - A carga inicial deve ser idempotente, executada automaticamente na inicialização caso o banco esteja vazio, dentro de uma transação única e preservando fielmente todos os campos originais do seed (incluindo `id` e `deletedAt`).
  - Os conjuntos de dados utilizados para a inicialização e conferência analítica da base devem ser distribuídos como recursos versionados e autossuficientes da camada de persistência, assegurando que o provisionamento do banco e a execução das suítes de testes automatizados ocorram de maneira determinística e independente de caminhos externos ao repositório.

## Ferramentas Globais

- **Plugin `dotnet-diag`**: subagente `optimizing-dotnet-performance`, skill `analyzing-dotnet-performance`
- **Plugin `dotnet`**: skills `csharp-expert`, `csharp-refactoring`
- **Plugin `ponytail`**: skills `ponytail`, `ponytail-review`
