# 03: Gestão Completa de Pesquisas (Surveys)

**O que construir:**
Operações de CRUD de pesquisas no backend (`POST /api/surveys`, `GET /api/surveys`, `GET /api/surveys/{id}`, `PUT`, `DELETE`) isoladas pela empresa ativa, validando tipos de métrica (`NPS` escala 0–10 ou `CSAT` escala 1–5), com visão dedicada de Gestão de Pesquisas no frontend.

**Bloqueado por:** 01: Fundação Multi-Empresa e Carga Inicial de Dados

**Status:** ready-for-agent

**Ferramentas recomendadas:**
- Skills: `propagate-feature`, `csharp-expert`
- MCPs: `mui-mcp`

**Critérios de aceitação:**
- [ ] `POST /api/surveys` valida campos obrigatórios, restringe `type` a `NPS` ou `CSAT`, associa ao `company_id` ativo e retorna `201 Created` com header `Location`.
- [ ] `GET /api/surveys` lista pesquisas ativas da empresa (`id`, `name`, `type`, `createdAt`).
- [ ] `GET /api/surveys/{id}` detalha pesquisa ativa da empresa ou retorna `404`.
- [ ] `PUT /api/surveys/{id}` atualiza o nome da pesquisa.
- [ ] `DELETE /api/surveys/{id}` executa soft delete da pesquisa (`deleted_at = UTC now`).
- [ ] Tela de Gestão de Pesquisas no React lista os questionários da empresa com identificação de tipo/escala e formulário modal para criar nova pesquisa.
