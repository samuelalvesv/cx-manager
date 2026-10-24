# 04: Gestão de Contatos com Busca FTS5 Trigram e Soft Delete

**O que construir:**
CRUD completo de contatos (`GET /api/contacts` com busca FTS5 trigram e paginação, `GET /{id}`, `POST`, `PUT`, `DELETE` com soft delete, unicidade de e-mail ativo `409` e liberação de e-mail) integrado à tela de Contatos no React com busca debounced, tabela paginada, modais de cadastro/edição e diálogo de confirmação de exclusão.

**Bloqueado por:** 01: Fundação Multi-Empresa e Carga Inicial de Dados

**Status:** resolved

**Ferramentas recomendadas:**
- Skills: `propagate-feature`, `audit-performance`
- MCPs: `mui-mcp`, `chrome-devtools`

**Critérios de aceitação:**
- [x] `GET /api/contacts` pagina contatos no SQLite (`LIMIT/OFFSET`), retornando `{ items, total, page, pageSize }`.
- [x] Busca `?search=` utiliza a tabela virtual `contacts_fts` com tokenizer `trigram`, realizando correspondência parcial case-insensitive e acento-insensível por nome ou e-mail.
- [x] `POST /api/contacts` valida nome e formato de e-mail, rejeita e-mail duplicado na mesma empresa com `409 Conflict` e retorna `201 Created` com header `Location`.
- [x] `PUT /api/contacts/{id}` atualiza dados do contato e valida e-mail único sem colidir consigo mesmo.
- [x] `DELETE /api/contacts/{id}` executa soft delete (`deleted_at = UTC now`), liberando o e-mail para novos cadastros.
- [x] Tela de Contatos no frontend com busca textual com debounce, tabela paginada, modais de cadastro/edição e diálogo de confirmação de exclusão do MUI.

## Resposta

Implementação concluída com sucesso e verificada por testes automatizados ponta a ponta:
- Contratos REST de Contatos modelados em `CxManager.Contract` (`CreateContactRequest`, `UpdateContactRequest`, `ContactResponse`, `PagedResponse<ContactResponse>`).
- Repositório SQLite `ContactRepository` com SQL escrito à mão, paginação `LIMIT/OFFSET`, busca textual rápida com `contacts_fts` (tokenizer trigram acento e case-insensitive) e unicidade de e-mail ativo.
- Mapeador estático com Riok.Mapperly `ContactMapper` em tempo de compilação sem reflexão.
- Validadores fortemente tipados com FluentValidation (`CreateContactRequestValidator`, `UpdateContactRequestValidator`).
- Casos de uso de aplicação isolados em `CxManager.Application.UseCases.Contacts.*`.
- Endpoints Minimal API em `ContactEndpoints.cs` sob `/api/contacts` com filtro de contexto multi-empresa `RequireCompanyContextFilter` e serialização AOT em `AppJsonSerializerContext`.
- Camada Frontend em React 19 + TypeScript 7 + MUI Material v9: tipos em `types/contact.ts`, serviço com suporte a cancelamento `AbortSignal` em `services/contactService.ts`, modais `ContactDialog.tsx` e `DeleteContactDialog.tsx`, e tela completa `ContactsPage.tsx` com busca debounced, tabela paginada e tratamento dos estados loading, error e empty.
- Suíte abrangente de testes automatizados unitários e de integração validando 100% dos fluxos e regras de negócio.
