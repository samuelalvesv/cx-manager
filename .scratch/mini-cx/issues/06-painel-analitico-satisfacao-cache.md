# 06: Painel Analítico de Satisfação (NPS e CSAT) com Cache Reativo

**O que construir:**
Endpoints analíticos agregados em SQL (`GET /api/analytics/summary` via semi-join `WHERE EXISTS`, `GET /api/analytics/nps-by-month`, `GET /api/analytics/nps-by-segment`, filtros de período e pesquisa), cache em memória particionado por empresa e invalidação reativa em mutações (contatos, pesquisas, respostas), e Dashboard de Satisfação completo no React com cards de KPIs, barra de distribuição, gráfico/tabela mensal e ranking por segmento.

**Bloqueado por:** 05: Histórico e Submissão de Respostas (Responses)

**Status:** done

**Ferramentas recomendadas:**
- Subagentes: `optimizing-dotnet-performance`
- Skills: `analyzing-dotnet-performance`, `audit-performance`
- MCPs: `mui-mcp`

**Critérios de aceitação:**
- [x] `GET /api/analytics/summary` calcula em SQL no SQLite: `responsesCount` total, `npsResponses`, contagem e percentuais de promotores/neutros/detratores (1 casa decimal), score NPS inteiro arredondado com `AwayFromZero` da diferença crua, e média CSAT (2 casas decimais), filtrando contatos e pesquisas ativas via `EXISTS`.
- [x] `GET /api/analytics/nps-by-month` agrupa respostas NPS válidas por mês em UTC (`"YYYY-MM"`), omitindo meses com zero respostas.
- [x] `GET /api/analytics/nps-by-segment` agrupa por `contacts.segment` via `INNER JOIN`, mapeando nulos para `"Sem segmento"` e ordenando por score NPS decrescente.
- [x] Filtros `from`, `to` e `surveyId` validados e aplicáveis a todas as consultas analíticas.
- [x] Respostas analíticas cacheadas em memória por empresa e filtros; qualquer escrita em contatos, pesquisas ou respostas purga imediatamente o cache analítico da empresa.
- [x] Dashboard no frontend com cards de métricas (NPS com cor semântica, CSAT, total de respostas), barra de distribuição, seletores de filtro, série mensal e tabela de segmentos com skeletons e empty states.
