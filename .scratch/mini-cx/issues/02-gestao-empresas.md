# 02: Gestão Completa de Empresas (Companies)

**O que construir:**
Operações completas de CRUD para a entidade Empresa no backend (`POST /api/companies`, `GET /api/companies/{id}`, `PUT /api/companies/{id}` e `DELETE /api/companies/{id}` com soft delete) e diálogo de criação/edição de empresas no frontend acessível a partir do seletor superior.

**Bloqueado por:** 01: Fundação Multi-Empresa e Carga Inicial de Dados

**Status:** ready-for-agent

**Ferramentas recomendadas:**
- Skills: `propagate-feature`, `audit-api`
- MCPs: `mui-mcp`

**Critérios de aceitação:**
- [ ] `POST /api/companies` cria nova empresa com validação de nome obrigatório e retorna `201 Created` com header `Location`.
- [ ] `GET /api/companies/{id}` retorna detalhes da empresa ativa ou `404` caso não exista ou esteja excluída logicamente.
- [ ] `PUT /api/companies/{id}` atualiza nome e descrição da empresa.
- [ ] `DELETE /api/companies/{id}` realiza soft delete preenchendo `deleted_at` com timestamp UTC e retorna `204 NoContent`.
- [ ] Interface no React permite abrir modal para cadastrar nova empresa e atualizar os dados da empresa ativa.
