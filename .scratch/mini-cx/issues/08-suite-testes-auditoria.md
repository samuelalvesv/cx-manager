# 08: Suíte de Testes Automatizados e Auditoria de Qualidade

**O que construir:**
Implementação das 4 suítes cirúrgicas de testes automatizados (`AnalyticsConferenceTests` com 100% de conferência do seed real, `ContactLifecycleAndSoftDeleteTests`, `NpsDomainCalculationTests`, `ArchitectureIsolationTests` com NetArchTest), verificação da compilação e execução via CLI `dotnet test`, e rodada de auditoria de qualidade final via `/code-review`.

**Bloqueado por:** 06: Painel Analítico de Satisfação (NPS e CSAT) com Cache Reativo, 07: Exportação de Respostas em CSV RFC 4180

**Status:** completed

**Ferramentas recomendadas:**
- Subagentes: `test-engineer`
- Skills: `run-tests`, `assertion-quality`, `audit-tests`, `code-review`, `audit-architecture`

**Critérios de aceitação:**
- [x] `AnalyticsConferenceTests` valida exatamente todos os números do seed: 1246 respostas, 978 NPS, 477/256/245 P/N/D, 48.8%/26.2%/25.1%, NPS 24, CSAT 3.91, meses e segmentos.
- [x] `ContactLifecycleAndSoftDeleteTests` valida ciclo completo de criação, erro 409 em e-mail duplicado, soft delete 204, 404 em busca, liberação de e-mail e expurgo imediato do analytics via cache invalidation.
- [x] `NpsDomainCalculationTests` valida regras de classificação NPS e arredondamento `AwayFromZero`.
- [x] `ArchitectureIsolationTests` garante limites arquiteturais estritos da Clean Architecture via `NetArchTest`.
- [x] Pipeline de auditoria `/code-review` executado com zero não-conformidades de arquitetura, contrato, performance e simplicidade.
