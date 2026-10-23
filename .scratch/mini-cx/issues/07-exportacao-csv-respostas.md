# 07: Exportação de Respostas em CSV RFC 4180

**O que construir:**
Endpoint `GET /api/responses/export` com streaming de respostas ativas em CSV (`text/csv; charset=utf-8`) compatível com RFC 4180, tratando corretamente vírgulas, quebras de linha e aspas nos comentários, e botão de download direto com feedback visual no painel do frontend.

**Bloqueado por:** 06: Painel Analítico de Satisfação (NPS e CSAT) com Cache Reativo

**Status:** ready-for-agent

**Ferramentas recomendadas:**
- Skills: `audit-api`, `web-design-guidelines`
- MCPs: `chrome-devtools`

**Critérios de aceitação:**
- [x] `GET /api/responses/export` projeta `id,survey,contact,score,comment,channel,respondedAt` para respostas válidas da empresa ativa respeitando filtros de data/pesquisa.
- [x] Tratamento estrito de RFC 4180: campos contendo vírgulas, quebras de linha ou aspas são envolvidos por aspas duplas, com aspas internas duplicadas (`""`).
- [x] Resposta com header `Content-Disposition: attachment; filename=respostas.csv` e `Content-Type: text/csv; charset=utf-8`.
- [x] Botão "Exportar CSV" no frontend dispara download assíncrono mantendo feedback visual de carregamento.
