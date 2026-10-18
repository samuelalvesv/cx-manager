# Especificação Técnica Consolidada — CxManager

Documento canônico com os requisitos funcionais, não-funcionais, regras de banco, contratos da API, estratégias de cache e suíte de testes do CxManager, sintetizado a partir do desafio técnico (`challenge-dev.local`) e das ADRs acordadas.

---

## 1. Schema do Banco de Dados (SQLite)

### Tabelas Principais
- **`companies`**:
  - `id INTEGER PRIMARY KEY` (BIGINT no C#)
  - `name TEXT NOT NULL`
  - `description TEXT`
- **`surveys`**:
  - `id INTEGER PRIMARY KEY`
  - `company_id INTEGER NOT NULL REFERENCES companies(id)`
  - `name TEXT NOT NULL`
  - `type TEXT NOT NULL` (`'NPS'` ou `'CSAT'`)
  - `created_at TEXT NOT NULL` (ISO 8601 UTC)
- **`contacts`**:
  - `id INTEGER PRIMARY KEY AUTOINCREMENT`
  - `company_id INTEGER NOT NULL REFERENCES companies(id)`
  - `name TEXT NOT NULL`
  - `email TEXT NOT NULL`
  - `segment TEXT`
  - `deleted_at TEXT` (ISO 8601 UTC, `NULL` quando ativo)
- **`responses`**:
  - `id INTEGER PRIMARY KEY AUTOINCREMENT`
  - `survey_id INTEGER NOT NULL REFERENCES surveys(id)`
  - `contact_id INTEGER NOT NULL REFERENCES contacts(id)`
  - `score INTEGER NOT NULL`
  - `comment TEXT`
  - `channel TEXT NOT NULL` (`'whatsapp'`, `'email'`, `'link'`)
  - `responded_at TEXT NOT NULL` (ISO 8601 UTC)
  - `deleted_at TEXT` (ISO 8601 UTC, `NULL` quando ativo)

### Tabela Virtual e Triggers FTS5 (Busca de Alta Performance)
- **`contacts_fts`**:
  - `CREATE VIRTUAL TABLE contacts_fts USING fts5(name, email, content='contacts', content_rowid='id', tokenize='trigram');`
- **Triggers**:
  - `AFTER INSERT ON contacts`: insere linha correspondente em `contacts_fts`.
  - `AFTER UPDATE ON contacts`: atualiza linha correspondente em `contacts_fts`.
  - `AFTER DELETE ON contacts`: remove linha correspondente em `contacts_fts`.

### Índices Justificados (com documentação SQL explícita)
1. `idx_contacts_company_email_active`:
   - `CREATE UNIQUE INDEX idx_contacts_company_email_active ON contacts(company_id, email COLLATE NOCASE) WHERE deleted_at IS NULL;`
   - *Justificativa*: Garante unicidade de e-mail por empresa no `POST` e `PUT /api/contacts` para contatos ativos e libera o e-mail após soft delete.
2. `idx_contacts_company_active`:
   - `CREATE INDEX idx_contacts_company_active ON contacts(company_id) WHERE deleted_at IS NULL;`
   - *Justificativa*: Otimiza listagem paginada padrão (`GET /api/contacts`) sem termo de busca.
3. `idx_responses_contact_active`:
   - `CREATE INDEX idx_responses_contact_active ON responses(contact_id, responded_at DESC) WHERE deleted_at IS NULL;`
   - *Justificativa*: Otimiza histórico de respostas do contato (`GET /api/contacts/{id}/responses`) ordenado por data decrescente.
4. `idx_responses_analytics_covering`:
   - `CREATE INDEX idx_responses_analytics_covering ON responses(survey_id, responded_at) WHERE deleted_at IS NULL;`
   - *Justificativa*: Acelera junções e filtros de período/pesquisa nas agregações de satisfação (`GET /api/analytics/summary`, `nps-by-month`).

---

## 2. Invariantes de Dados e Regras de Negócio

1. **Soft Delete e Filtragem de Contato Ativo**:
   - `deleted_at IS NOT NULL` exclui o registro de qualquer leitura, detalhe, contagem ou exportação.
   - **Estratégia EXISTS vs INNER JOIN (ADR-0001)**:
     - `GET /api/analytics/summary` e `GET /api/analytics/nps-by-month`: utilizam `WHERE r.deleted_at IS NULL AND EXISTS (SELECT 1 FROM contacts c WHERE c.id = r.contact_id AND c.company_id = @CompanyId AND c.deleted_at IS NULL)`. Essa semi-join é superior pois o SQLite faz busca pontual por índice sem alocar tuplas de junção para colunas de contatos que não são projetadas.
     - `GET /api/analytics/nps-by-segment`: utiliza `INNER JOIN contacts c ON c.id = r.contact_id WHERE c.deleted_at IS NULL AND c.company_id = @CompanyId AND r.deleted_at IS NULL` pois necessita de `c.segment` na cláusula `GROUP BY`.
     - `GET /api/responses/export`: utiliza `INNER JOIN contacts c ON c.id = r.contact_id WHERE c.deleted_at IS NULL AND c.company_id = @CompanyId AND r.deleted_at IS NULL` pois projeta o nome do contato (`c.name`) nas colunas do CSV.
2. **Escalas e Fórmulas**:
   - `responsesCount`: total de respostas válidas (NPS e CSAT).
   - NPS: exclusivo para `surveys.type = 'NPS'`.
     - Detrator: nota 0 a 6.
     - Neutro: nota 7 a 8.
     - Promotor: nota 9 a 10.
     - Percentuais: 1 casa decimal.
     - `npsScore`: `Round(promoters_pct_raw - detractors_pct_raw, MidpointRounding.AwayFromZero)`.
   - CSAT: exclusivo para `surveys.type = 'CSAT'`, média aritmética (`csatAvg`) com 2 casas decimais. Retorna `null` se não houver respostas CSAT.
   - Sem respostas NPS no recorte: contadores e percentuais retornam 0, `npsScore` retorna 0 (nunca NaN ou 500).
3. **Filtros de Período**:
   - `from`: data inicial inclusiva (`yyyy-MM-dd`).
   - `to`: data final inclusiva (`yyyy-MM-dd`), convertida para `< date(to, '+1 day')` em UTC.
4. **Agrupamento Mensal (`nps-by-month`)**:
   - Ano-mês `"YYYY-MM"` em UTC.
   - Somente meses com pelo menos 1 resposta NPS válida entram na série.
5. **Agrupamento por Segmento (`nps-by-segment`)**:
   - `contacts.segment` nulo vira `"Sem segmento"`.
   - Ordenado por `npsScore DESC`.

---

## 3. Contratos REST da API

- **Padronização**: prefixo `/api`, payload JSON `camelCase`, datas em ISO 8601 UTC.
- **Envelope de Erro**: `{ "error": "mensagem legível em pt-BR" }` com códigos `400`, `404`, `409` (sem ProblemDetails).

| Endpoint | Método | Status Sucesso | Descrição |
|---|---|---|---|
| `/api/contacts` | GET | `200 OK` | Lista paginada (`items`, `total`, `page`, `pageSize`) com busca FTS opcional por nome/e-mail. |
| `/api/contacts/{id}` | GET | `200 OK` | Detalha contato ativo; `404` se inexistente ou soft-deleted. |
| `/api/contacts` | POST | `201 Created` | Cria contato; valida e-mail único ativo (`409`), retorna header `Location`. Invalida cache analítico. |
| `/api/contacts/{id}` | PUT | `200 OK` | Atualiza contato; valida e-mail único (`409` se pertencer a outro contato). Invalida cache analítico. |
| `/api/contacts/{id}` | DELETE | `204 NoContent` | Soft delete (`deleted_at = UTC now`). Libera e-mail. Invalida cache analítico. |
| `/api/contacts/{id}/responses` | GET | `200 OK` | Histórico de respostas do contato com JOIN de pesquisa, mais recentes primeiro. |
| `/api/analytics/summary` | GET | `200 OK` | Resumo analítico geral agregado em SQL (NPS, distribuição, contagens, CSAT). Cached. |
| `/api/analytics/nps-by-month` | GET | `200 OK` | Série temporal mensal de NPS. Cached. |
| `/api/analytics/nps-by-segment` | GET | `200 OK` | NPS agregado por segmento de contato. Cached. |
| `/api/surveys` | GET | `200 OK` | Lista de pesquisas para filtro no frontend. |
| `/api/companies` | GET | `200 OK` | Lista de empresas ativas para seletor no frontend. |
| `/api/responses/export` | GET | `200 OK` | Exportação de respostas ativas em CSV RFC 4180 (`text/csv; charset=utf-8`). |

---

## 4. Estratégia de Cache e Resolução Multi-Empresa

1. **`ICompanyContext`**:
   - Lê obrigatoriamente o cabeçalho `X-Company-Id` (ou parâmetro de consulta `companyId`).
   - **Zero fallback ou valor padrão**: o identificador da empresa é obrigatório. Requisições que omitirem a identificação da empresa respondem `400 BadRequest` com `{ "error": "O identificador da empresa é obrigatório." }` (exceto `/api/companies`, que lista as empresas disponíveis para seleção).
   - Repositórios, Use Cases e instâncias de cache sempre operam vinculados ao `companyId` informado.
2. **Cache em Memória**:
   - Resumo e séries de analytics são cacheados por empresa e combinação de filtros.
   - Qualquer mutação de contato (`POST`, `PUT`, `DELETE /api/contacts/{id}`) purga imediatamente o cache de analytics da respectiva empresa (ADR-0004).

---

## 5. Frontend (React 19 + MUI v9 + TypeScript 7)

1. **Design System & Acessibilidade**:
   - MUI Material v9 com tokens semânticos centralizados em `src/theme/`.
   - Responsivo e acessível para todos os tamanhos de viewport.
2. **Páginas**:
   - **Painel de Satisfação**: Cards de KPIs (NPS Score com indicador de cor, CSAT, Total de respostas), barra de distribuição Promotores/Neutros/Detratores, filtros combinados (Período de/até e Pesquisa), tabela/gráfico de evolução mensal e ranking por segmento, botão de exportação CSV.
   - **Gestão de Contatos**: Tabela com busca FTS com debounce e paginação dinâmica, botão de novo contato, edição inline/modal, diálogo modal de confirmação de exclusão e drawer lateral para visualização do histórico de respostas.
   - **AppBar Superior**: Seletor dinâmico de empresa (`GET /api/companies`), alternância de abas e link de documentação.
3. **Estados de Interface**:
   - Skeleton nos cartões e tabelas durante carregamento.
   - Alerta amigável com botão de tentar novamente em caso de falha de requisição.
   - Empty state ilustrado com mensagem de orientação quando não houver registros.
4. **Isolamento de API**:
   - Todas as chamadas HTTP isoladas em `src/services/` com suporte a cancelamento via `AbortSignal`.

---

## 6. Suíte de Testes Automatizados (4 Pilares)

1. **`AnalyticsConferenceTests`**:
   - Validação dos valores de conferência do desafio com o banco SQLite carregado com `seed.json`:
     - Total de respostas ativas: `1246`
     - Respostas válidas NPS: `978`
     - Promotores / Neutros / Detratores: `477 / 256 / 245`
     - Percentuais: `48.8% / 26.2% / 25.1%`
     - Score NPS Geral: `24`
     - Média CSAT: `3.91`
     - Série mensal (Jan: 104/17, Fev: 112/26, Mar: 270/11, Abr: 96/14, Mai: 121/22, Jun: 275/42)
     - Segmentos (Plano Black: 415/31, Corporativo: 292/19, Plano Fit: 271/18)
     - Filtros de pesquisa (Pesquisa 1: 21, Pesquisa 2: 29)
2. **`ContactLifecycleAndSoftDeleteTests`**:
   - Criação de contato (`201` + `Location`).
   - Bloqueio de e-mail duplicado ativo (`409`).
   - Soft delete (`204`) e ocultação em buscas (`404`).
   - Liberação do mesmo e-mail para novo cadastro após exclusão (`201`).
   - Verificação de exclusão imediata das respostas do contato no resumo analítico e invalidação de cache.
3. **`NpsDomainCalculationTests`**:
   - Classificação das faixas NPS e arredondamento `AwayFromZero` do score cru.
4. **`ArchitectureIsolationTests`**:
   - Regras de fronteira e isolamento estrito de camadas via `NetArchTest`.
