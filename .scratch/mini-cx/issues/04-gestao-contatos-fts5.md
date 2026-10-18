# 04: Gestão de Contatos com Busca FTS5 Trigram e Soft Delete

**O que construir:**
CRUD completo de contatos (`GET /api/contacts` com busca FTS5 trigram e paginação, `GET /{id}`, `POST`, `PUT`, `DELETE` com soft delete, unicidade de e-mail ativo `409` e liberação de e-mail) integrado à tela de Contatos no React com busca debounced, tabela paginada, modais de cadastro/edição e diálogo de confirmação de exclusão.

**Bloqueado por:** 01: Fundação Multi-Empresa e Carga Inicial de Dados

**Status:** ready-for-agent

**Ferramentas recomendadas:**
- Skills: `propagate-feature`, `audit-performance`
- MCPs: `mui-mcp`, `chrome-devtools`

**Critérios de aceitação:**
- [ ] `GET /api/contacts` pagina contatos no SQLite (`LIMIT/OFFSET`), retornando `{ items, total, page, pageSize }`.
- [ ] Busca `?search=` utiliza a tabela virtual `contacts_fts` com tokenizer `trigram`, realizando correspondência parcial case-insensitive e acento-insensível por nome ou e-mail.
- [ ] `POST /api/contacts` valida nome e formato de e-mail, rejeita e-mail duplicado na mesma empresa com `409 Conflict` e retorna `201 Created` com header `Location`.
- [ ] `PUT /api/contacts/{id}` atualiza dados do contato e valida e-mail único sem colidir consigo mesmo.
- [ ] `DELETE /api/contacts/{id}` executa soft delete (`deleted_at = UTC now`), liberando o e-mail para novos cadastros.
- [ ] Tela de Contatos no frontend com busca textual com debounce, tabela paginada, modais de cadastro/edição e diálogo de confirmação de exclusão do MUI.
