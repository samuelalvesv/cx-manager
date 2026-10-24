# Especificação Técnica Consolidada — CxManager

Documento canônico com os requisitos funcionais, não-funcionais, regras de banco, contratos da API, estratégias de cache e suíte de testes do CxManager, sintetizado a partir dos requisitos de produto, contratos REST e das ADRs acordadas (ADR-0001 a ADR-0005).

---

## 1. Schema do Banco de Dados (SQLite)

### Tabelas Principais
- **`companies`**:
  - `id INTEGER PRIMARY KEY AUTOINCREMENT`
  - `name TEXT NOT NULL`
  - `description TEXT`
  - `deleted_at TEXT` (ISO 8601 UTC, `NULL` quando ativa)
- **`surveys`**:
  - `id INTEGER PRIMARY KEY AUTOINCREMENT`
  - `company_id INTEGER NOT NULL REFERENCES companies(id)`
  - `name TEXT NOT NULL`
  - `type TEXT NOT NULL` (`'NPS'` ou `'CSAT'`)
  - `created_at TEXT NOT NULL` (ISO 8601 UTC)
  - `deleted_at TEXT` (ISO 8601 UTC, `NULL` quando ativa)
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
  - `deleted_at TEXT` (ISO 8601 UTC, `NULL` quando ativa)

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
5. `idx_surveys_company_active`:
   - `CREATE INDEX idx_surveys_company_active ON surveys(company_id) WHERE deleted_at IS NULL;`
   - *Justificativa*: Otimiza listagem de pesquisas ativas por empresa para filtros e cadastros (`GET /api/surveys`).

---

## 2. Invariantes de Dados e Regras de Negócio

1. **Soft Delete e Filtragem de Entidades Ativas**:
   - `deleted_at IS NOT NULL` exclui o registro de qualquer leitura operacional, detalhe ou relatório.
   - **Estratégia EXISTS vs INNER JOIN (ADR-0001)**:
     - `GET /api/analytics/summary` e `GET /api/analytics/nps-by-month`: utilizam `WHERE r.deleted_at IS NULL AND EXISTS (SELECT 1 FROM contacts c WHERE c.id = r.contact_id AND c.company_id = @CompanyId AND c.deleted_at IS NULL) AND EXISTS (SELECT 1 FROM surveys s WHERE s.id = r.survey_id AND s.deleted_at IS NULL)`. Essa semi-join é superior pois o SQLite faz busca pontual por índice sem alocar tuplas de junção para colunas de contatos que não são projetadas.
     - `GET /api/analytics/nps-by-segment`: utiliza `INNER JOIN contacts c ON c.id = r.contact_id WHERE c.deleted_at IS NULL AND c.company_id = @CompanyId AND r.deleted_at IS NULL` pois necessita de `c.segment` na cláusula `GROUP BY`.
     - `GET /api/responses/export`: utiliza `INNER JOIN contacts c ON c.id = r.contact_id INNER JOIN surveys s ON s.id = r.survey_id WHERE c.deleted_at IS NULL AND c.company_id = @CompanyId AND r.deleted_at IS NULL AND s.deleted_at IS NULL` para projetar o nome do contato e da pesquisa.
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
3. **Validação de Inserção de Respostas**:
   - Para pesquisas do tipo `NPS`, a nota deve ser um inteiro entre `0` e `10`.
   - Para pesquisas do tipo `CSAT`, a nota deve ser um inteiro entre `1` e `5`.
   - O canal deve ser restrito a `'whatsapp'`, `'email'` ou `'link'`.
   - A resposta deve referenciar um contato ativo e uma pesquisa ativa da mesma empresa.
4. **Filtros de Período**:
   - `from`: data inicial inclusiva (`yyyy-MM-dd`).
   - `to`: data final inclusiva (`yyyy-MM-dd`), convertida para `< date(to, '+1 day')` em UTC.
5. **Agrupamentos Analíticos**:
   - `nps-by-month`: Ano-mês `"YYYY-MM"` em UTC. Meses sem respostas válidas no recorte não entram na série.
   - `nps-by-segment`: `contacts.segment` nulo vira `"Sem segmento"`. Ordenado por `npsScore DESC`.

---

## 3. Contratos REST da API

- **Padronização**: prefixo `/api`, payload JSON `camelCase`, datas em ISO 8601 UTC.
- **Envelope de Erro**: `{ "error": "mensagem legível em pt-BR" }` para status `400`, `404` e `409` (sem ProblemDetails).

### 3.1. Gestão de Empresas (`/api/companies`)
| Endpoint | Método | Status Sucesso | Descrição |
|---|---|---|---|
| `/api/companies` | GET | `200 OK` | Lista todas as empresas ativas para seleção no frontend. |
| `/api/companies/{id}` | GET | `200 OK` | Detalhes de uma empresa (`404` se inexistente ou desativada). |
| `/api/companies` | POST | `201 Created` | Cria nova empresa (`name` obrigatório). Retorna header `Location`. |
| `/api/companies/{id}` | PUT | `200 OK` | Atualiza nome e descrição da empresa. |
| `/api/companies/{id}` | DELETE | `204 NoContent` | Desativação/soft-delete de empresa. |

### 3.2. Gestão de Pesquisas (`/api/surveys`) — Escopado por `X-Company-Id`
| Endpoint | Método | Status Sucesso | Descrição |
|---|---|---|---|
| `/api/surveys` | GET | `200 OK` | Lista pesquisas ativas da empresa (`id`, `name`, `type`, `createdAt`). |
| `/api/surveys/{id}` | GET | `200 OK` | Detalhes da pesquisa (`404` se inexistente ou excluída). |
| `/api/surveys` | POST | `201 Created` | Cria pesquisa (`name`, `type: 'NPS' \| 'CSAT'`). Retorna header `Location`. Invalida cache analítico. |
| `/api/surveys/{id}` | PUT | `200 OK` | Atualiza nome da pesquisa. |
| `/api/surveys/{id}` | DELETE | `204 NoContent` | Soft delete da pesquisa (`deleted_at = UTC now`). Invalida cache analítico. |

### 3.3. Gestão de Contatos (`/api/contacts`) — Escopado por `X-Company-Id`
| Endpoint | Método | Status Sucesso | Descrição |
|---|---|---|---|
| `/api/contacts` | GET | `200 OK` | Lista paginada (`items`, `total`, `page`, `pageSize`) com busca FTS opcional por nome/e-mail. |
| `/api/contacts/{id}` | GET | `200 OK` | Detalha contato ativo; `404` se inexistente ou excluído. |
| `/api/contacts` | POST | `201 Created` | Cria contato; valida e-mail único ativo (`409`), retorna header `Location`. Invalida cache analítico. |
| `/api/contacts/{id}` | PUT | `200 OK` | Atualiza contato; valida e-mail único (`409` se pertencer a outro contato). Invalida cache analítico. |
| `/api/contacts/{id}` | DELETE | `204 NoContent` | Soft delete (`deleted_at = UTC now`). Libera e-mail. Invalida cache analítico. |
| `/api/contacts/{id}/responses` | GET | `200 OK` | Histórico cronológico de respostas do contato com JOIN da pesquisa. |

### 3.4. Gestão de Respostas (`/api/responses`) — Escopado por `X-Company-Id`
| Endpoint | Método | Status Sucesso | Descrição |
|---|---|---|---|
| `/api/responses/{id}` | GET | `200 OK` | Consulta detalhada de uma resposta ativa. |
| `/api/responses` | POST | `201 Created` | Submete nova resposta (`surveyId`, `contactId`, `score`, `comment`, `channel`). Valida escala da pesquisa (`400` se fora da escala). Invalida cache analítico. |
| `/api/responses/{id}` | PUT | `200 OK` | Atualiza comentário ou nota de uma resposta. Invalida cache analítico. |
| `/api/responses/{id}` | DELETE | `204 NoContent` | Soft delete de resposta (`deleted_at = UTC now`). Invalida cache analítico. |

### 3.5. Analytics de Satisfação e Exportação — Escopado por `X-Company-Id`
| Endpoint | Método | Status Sucesso | Descrição |
|---|---|---|---|
| `/api/analytics/summary` | GET | `200 OK` | Resumo analítico geral agregado em SQL (NPS, distribuição, contagens, CSAT). Cached. |
| `/api/analytics/nps-by-month` | GET | `200 OK` | Série temporal mensal de NPS. Cached. |
| `/api/analytics/nps-by-segment` | GET | `200 OK` | NPS agregado por segmento de contato. Cached. |
| `/api/responses/export` | GET | `200 OK` | Exportação de respostas ativas em CSV RFC 4180 (`text/csv; charset=utf-8`). |

---

## 4. Estratégia de Cache e Resolução Multi-Empresa

1. **`ICompanyContext`**:
   - Lê obrigatoriamente o cabeçalho `X-Company-Id` (ou parâmetro de consulta `companyId`).
   - Zero fallback ou valor padrão: o identificador da empresa é obrigatório. Requisições que omitirem a identificação da empresa respondem `400 BadRequest` com `{ "error": "O identificador da empresa é obrigatório." }` (exceto `/api/companies`).
   - Repositórios, Use Cases e instâncias de cache sempre operam vinculados ao `companyId` informado.
2. **Cache em Memória e Invalidação Reativa (ADR-0004)**:
   - Resumo e séries de analytics são cacheados por empresa e combinação de filtros.
   - Qualquer mutação em contatos (`POST/PUT/DELETE /api/contacts`), respostas (`POST/PUT/DELETE /api/responses`) ou pesquisas (`POST/PUT/DELETE /api/surveys`) purga imediatamente o cache de analytics da respectiva empresa.

---

## 5. Frontend (React 19 + MUI v9 + TypeScript 7)

1. **Visões da Aplicação**:
   - **Painel de Satisfação**: Cards de KPIs (NPS Score, CSAT, Total de respostas), barra de distribuição Promotores/Neutros/Detratores, filtros combinados (Período de/até e Pesquisa), tabela/gráfico de evolução mensal, ranking por segmento e botão de exportação CSV.
   - **Gestão de Contatos**: Tabela com busca FTS com debounce e paginação dinâmica, formulário modal de criação/edição, diálogo de confirmação de exclusão e drawer lateral para visualização do histórico de respostas.
   - **Gestão de Pesquisas**: Listagem das pesquisas cadastradas com tipo e data de criação, com botão para adicionar nova pesquisa.
   - **AppBar Superior**: Seletor dinâmico de empresa (`GET /api/companies`), alternância de abas e link de documentação.
2. **Estados de Interface**:
   - Skeleton nos cartões e tabelas durante carregamento.
   - Alerta amigável com botão de tentar novamente em caso de falha de requisição.
   - Empty state ilustrado com mensagem de orientação quando não houver registros.
3. **Isolamento de API**:
   - Todas as chamadas HTTP isoladas em `src/services/` com suporte a cancelamento via `AbortSignal`.

---

## 6. Suíte de Testes Automatizados (4 Pilares)

1. **`AnalyticsConferenceTests`**:
   - Validação dos valores analíticos de conferência e baseline com o banco SQLite carregado com `seed.json`:
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
   - Bloqueio de e-mail duplicado ativo na mesma empresa (`409`).
   - Soft delete (`204`) e conferência de `404` na busca subsequente.
   - Liberação do mesmo e-mail para novo cadastro após exclusão (`201`).
   - Exclusão das respostas do contato no resumo analítico e confirmação da invalidação do cache.
3. **`NpsDomainCalculationTests`**:
   - Classificação das faixas NPS e arredondamento `AwayFromZero` do score cru.
4. **`ArchitectureIsolationTests`**:
   - Regras com `NetArchTest` garantindo que Domain e Contract não possuam dependências externas, que Infrastructure não conheça API nem Contract, e que Application dependa exclusivamente de Domain e Contract.
