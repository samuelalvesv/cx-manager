# 01: Fundação Multi-Empresa e Carga Inicial de Dados

**O que construir:**
Inicialização do schema relacional no SQLite com suporte nativo a multi-empresa e tabela virtual FTS5, importação idempotente e atômica do arquivo de seed com vínculo dinâmico da empresa lida do arquivo, injeção de contexto `ICompanyContext` com exigência estrita do cabeçalho `X-Company-Id` (400 Bad Request caso ausente), endpoint público `GET /api/companies` e seletor dinâmico de empresas no cabeçalho superior (AppBar do MUI) do frontend.

**Bloqueado por:** Nenhum (pode iniciar imediatamente).

**Status:** ready-for-agent

**Ferramentas recomendadas:**
- Skills: `csharp-expert`, `analyzing-dotnet-performance`
- Subagentes: `optimizing-dotnet-performance`
- MCPs: `mui-mcp`

**Critérios de aceitação:**
- [ ] Schema SQLite criado com `companies`, `surveys`, `contacts`, `responses` e tabela virtual `contacts_fts` com triggers de sincronização.
- [ ] Importador do seed carrega dados do JSON em transação única no startup se o banco estiver vazio, associando ao ID dinâmico da empresa.
- [ ] `ICompanyContext` resolve `X-Company-Id` (ou query param `companyId`) e retorna 400 `{ "error": "O identificador da empresa é obrigatório." }` caso omitido.
- [ ] `GET /api/companies` retorna lista de empresas ativas sem exigir cabeçalho prévio.
- [ ] AppBar no frontend exibe o seletor de empresa e armazena o ID selecionado para incluir no cabeçalho `X-Company-Id` de todas as chamadas HTTP via `services/`.
