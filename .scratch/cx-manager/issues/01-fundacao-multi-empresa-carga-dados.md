# 01: Fundação Multi-Empresa e Carga Inicial de Dados

**O que construir:**
Inicialização do schema relacional no SQLite com suporte nativo a multi-empresa e tabela virtual FTS5, importação idempotente e atômica do arquivo de seed com vínculo dinâmico da empresa lida do arquivo, injeção de contexto `ICompanyContext` com exigência estrita do cabeçalho `X-Company-Id` (400 Bad Request caso ausente), endpoint público `GET /api/companies` e seletor dinâmico de empresas no cabeçalho superior (AppBar do MUI) do frontend.

**Bloqueado por:** Nenhum (pode iniciar imediatamente).

**Status:** resolved

**Ferramentas recomendadas:**
- Skills: `csharp-expert`, `analyzing-dotnet-performance`
- Subagentes: `optimizing-dotnet-performance`
- MCPs: `mui-mcp`

**Critérios de aceitação:**
- [x] Schema SQLite criado com `companies`, `surveys`, `contacts`, `responses` e tabela virtual `contacts_fts` com triggers de sincronização.
- [x] Importador do seed carrega dados do JSON em transação única no startup se o banco estiver vazio, associando ao ID dinâmico da empresa.
- [x] `ICompanyContext` resolve `X-Company-Id` (ou query param `companyId`) e retorna 400 `{ "error": "O identificador da empresa é obrigatório." }` caso omitido.
- [x] `GET /api/companies` retorna lista de empresas ativas sem exigir cabeçalho prévio.
- [x] AppBar no frontend exibe o seletor de empresa e armazena o ID selecionado para incluir no cabeçalho `X-Company-Id` de todas as chamadas HTTP via `services/`.

## Resposta

Implementação concluída com sucesso e verificada por testes automatizados ponta a ponta:

1. **Schema Relacional SQLite e FTS5 com Triggers**:
   - Criadas tabelas `companies`, `surveys`, `contacts`, `responses` com constraints de chave estrangeira (`PRAGMA foreign_keys = ON;`) e índices documentados.
   - Criada tabela virtual FTS5 `contacts_fts` com o tokenizer `trigram` e triggers de sincronização imediata (`AFTER INSERT`, `AFTER DELETE`, `AFTER UPDATE`).
2. **Carga Inicial Atômica e Idempotente (`SeedImporter`)**:
   - Desserialização com `System.Text.Json` Source Generation (`SeedJsonSerializerContext`) compatível com Native AOT.
   - Importação em transação atômica única vinculando surveys e contacts ao `company.id` dinâmico lido do JSON (1 empresa, 3 pesquisas, 320 contatos, 1284 respostas persistidas).
3. **Resolução de Contexto Multi-Empresa (`ICompanyContext` e `HttpCompanyContext`)**:
   - Inspeção estrita do cabeçalho `X-Company-Id` (com suporte a fallback secundário via query param `companyId`).
   - Rejeição na entrada com `400 Bad Request` e `{ "error": "O identificador da empresa é obrigatório." }` caso omitido.
4. **Endpoint Público `GET /api/companies`**:
   - Minimal API REST com `TypedResults.Ok<IReadOnlyList<CompanyResponse>>` e serialização Source Generated via `AppJsonSerializerContext`.
5. **Frontend React 19 + TypeScript 7 + MUI Material v9**:
   - Cliente HTTP tipado em `services/apiClient.ts` com injeção automática do cabeçalho `X-Company-Id` e cancelamento por `AbortSignal`.
   - Seletor dinâmico de empresas integrado ao `AppBar`, com estados de carregamento (`Skeleton`), persistência no `localStorage` e tipagem estrita (zero erros e zero warnings em `oxlint`, `tsc -b` e `vite build`).
6. **Suíte de Testes Automatizados**:
   - 14 testes unitários em memória (`CxManager.UnitTests`).
   - 6 testes de arquitetura com NetArchTest (`CxManager.ArchitectureTests`).
   - 9 testes de integração contra SQLite real e Kestrel HTTP (`CxManager.IntegrationTests`).
