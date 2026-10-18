---
name: data-integrity-rules
description: End-to-end type and nullability symmetry (Domain <-> Contract <-> SQLite DDL <-> TypeScript), soft delete invariants (deleted_at IS NULL), survey scale isolation (NPS vs CSAT), rounding precision, and contact uniqueness constraints. Use this rule whenever writing domain entities, DTO contracts, SQLite DDL/queries, or frontend types.
---

# Integridade de Dados, Soft Delete & Escalas Analíticas — CxManager

## 1. Isolamento de Exclusão Lógica (Soft Delete) — Regra Inviolável

Tanto contatos quanto respostas de pesquisa possuem indicador temporal de exclusão lógica (`deleted_at`). Quando esse valor **não é nulo**, o registro foi excluído logicamente e **jamais pode participar de leituras, listagens, buscas, detalhes, cálculos analíticos, validações de unicidade ou exportações**.

> **Invariante Número 1 do Banco de Dados**: Toda consulta SQL sobre contatos ou respostas deve conter obrigatoriamente o predicado `deleted_at IS NULL`. Em consultas que realizam `JOIN` entre respostas e contatos, o filtro de registros ativos deve ser aplicado para garantir que nenhum registro excluído contamine o resultado.

### Comportamento do Soft Delete em Contatos
- A operação de exclusão de contato (`DELETE`) nunca remove fisicamente a linha da tabela; ela preenche a coluna `deleted_at` com o timestamp UTC atual (`DateTimeOffset.UtcNow`).
- Contatos excluídos deixam imediatamente de aparecer em listagens, buscas e consultas por identificador (retornando `404`).
- **Liberação de E-mail**: A restrição de unicidade de e-mail aplica-se **exclusivamente aos contatos ativos (`deleted_at IS NULL`)**. Uma vez que um contato é excluído logicamente, seu endereço de e-mail fica imediatamente disponível para um novo cadastro sem gerar conflito (`409`).

---

## 2. Isolamento Estrito de Escalas de Pesquisa (`NPS` vs `CSAT`)

O sistema opera com tipos de pesquisa que possuem escalas matemáticas incompatíveis entre si:

| Tipo de Pesquisa | Escala | Métricas Aplicáveis |
| --- | --- | --- |
| **NPS** | `0` a `10` | Classificação em Detrator (`0–6`), Neutro (`7–8`) e Promotor (`9–10`), percentuais por classe e score NPS. |
| **CSAT** | `1` a `5` | Média aritmética simples das notas CSAT. |
| **Todas (Agnóstico)** | Qualquer | Contagem global de respostas válidas no recorte. |

### Regras de Segregação em Consultas Analíticas
- **Proibição de Mistura de Escalas**: Cálculos de promotores, neutros, detratores e score NPS devem considerar **estritamente** respostas vinculadas a pesquisas do tipo `NPS`. Cálculos de média CSAT devem considerar **estritamente** respostas vinculadas a pesquisas do tipo `CSAT`.
- **Contagem Global**: É a única métrica agnóstica de escala, contabilizando todas as respostas ativas (`deleted_at IS NULL`) dentro do recorte filtrado.
- **Ausência de Respostas no Recorte**:
  - Se um recorte filtrado não possuir nenhuma resposta `NPS` válida, os contadores, percentuais e o score NPS devem retornar zerados (`0`), nunca `NaN`, infinito ou erro de divisão por zero.
  - Se um recorte filtrado não possuir nenhuma resposta `CSAT` válida, a média CSAT deve retornar `null`.
  - Em séries temporais mensais, meses sem nenhuma resposta `NPS` válida no recorte **não devem entrar na série retornada**.

---

## 3. Onde Calcular e Como Arredondar

- **Agregações 100% no Banco de Dados**: Contagens, somas condicionais (`CASE WHEN`), médias (`AVG`), agrupamentos (`GROUP BY`) e junções (`JOIN`) devem acontecer integralmente em SQL no banco de dados. É estritamente proibido carregar linhas de respostas para a memória da aplicação para agregá-las via LINQ.
- **Precisão Total e Arredondamento Apenas na Formatação**:
  - **Percentuais de distribuição NPS**: Calculados em ponto flutuante e arredondados para **1 casa decimal** apenas na representação final.
  - **Score NPS**: Calculado obrigatoriamente a partir da **diferença das porcentagens cruas (não arredondadas)** e arredondado para o **inteiro** mais próximo apenas no momento final (`MidpointRounding.AwayFromZero`). Arredondar os percentuais antes de subtrair introduz erro acumulado e é proibido.
  - **Média CSAT**: Calculada sobre notas `CSAT` válidas e arredondada para **2 casas decimais**.

---

## 4. Regras de Filtros e Agrupamentos Analíticos

- **Filtro de Período**: Filtra pelo instante de resposta em UTC, sendo **inclusivo nas duas pontas** (todo o dia final até `23:59:59.9999999Z` em UTC deve ser incluído).
- **Agrupamento Mensal**: Agrupado pelo ano e mês de resposta em UTC no formato `"YYYY-MM"`, ordenado cronologicamente de forma ascendente.
- **Agrupamento por Segmento**: Obtido via `JOIN` entre respostas válidas e contatos ativos; contatos cujo segmento seja nulo devem ser agrupados sob o rótulo `"Sem segmento"`, ordenando o resultado pelo score NPS de forma decrescente.

---

## 5. Cadeia Canônica de Simetria de Tipos e Nulabilidade (Ponta a Ponta)

Para garantir a integridade absoluta dos dados e prevenir divergências de contrato entre a persistência, o transporte e a interface, todas as propriedades devem preservar paridade estrita de tipos e nulabilidade através dos 4 elos da aplicação: **C# Domain**, **C# Contract**, **SQLite DDL** e **Frontend TypeScript**.

### Regra Inviolável de Nulabilidade e Obrigatoriedade
- **Campos Obrigatórios no Domínio**: Se uma propriedade for obrigatória no modelo de domínio (`string`, `long`, `DateTimeOffset`), ela **deve** ser:
  1. Definida obrigatoriamente com a constraint `NOT NULL` no schema DDL do SQLite.
  2. Declarada como `required` e não-anulável no DTO de contrato (`required string Prop { get; init; }`).
  3. Mapeada no TypeScript do frontend como propriedade não-anulável (`readonly prop: string;`), sem união `| null`.
- **Campos Opcionais no Domínio**: Se uma propriedade for opcional no modelo de domínio (`string?`, `DateTimeOffset?`), ela **deve** ser:
  1. Permitida como anulável no schema DDL do SQLite (sem a constraint `NOT NULL`).
  2. Declarada como `required` com tipo anulável explícito no DTO de contrato (`required string? Prop { get; init; }`).
  3. Mapeada no TypeScript do frontend como propriedade explicitamente anulável (`readonly prop: string | null;`). É expressamente proibido utilizar `prop?: string` para campos de resposta vindos da API, forçando a presença explícita de `null` no transporte JSON.

### Matriz Canônica de Correspondência de Tipos

| Semântica de Dados | 1. C# Domain | 2. C# Contract | 3. SQLite DDL | 4. Frontend TypeScript |
| :--- | :--- | :--- | :--- | :--- |
| **Texto Obrigatório** | `string` (validado `[min, max]`) | `required string Prop { get; init; }` | `TEXT NOT NULL` | `readonly prop: string;` |
| **Texto Opcional** | `string?` (validado `[min, max]`) | `required string? Prop { get; init; }` | `TEXT` | `readonly prop: string \| null;` |
| **Identificador / Chave Primária** | `long` (validado `> 0`) | `required long Prop { get; init; }` | `INTEGER PRIMARY KEY AUTOINCREMENT` | `readonly prop: number;` |
| **Chave Estrangeira Obrigatória** | `long` (validado `> 0`) | `required long Prop { get; init; }` | `INTEGER NOT NULL REFERENCES ...` | `readonly prop: number;` |
| **Instantâneo Temporal UTC** | `DateTimeOffset` (UTC) | `required DateTimeOffset Prop { get; init; }` | `TEXT NOT NULL` (ISO 8601 UTC) | `readonly prop: string;` |
| **Instantâneo Opcional UTC** | `DateTimeOffset?` (UTC) | `required DateTimeOffset? Prop { get; init; }` | `TEXT` (ISO 8601 UTC ou NULL) | `readonly prop: string \| null;` |
| **Data Pura (sem componente de hora)**| `DateOnly` | `required DateOnly Prop { get; init; }` | `TEXT NOT NULL` (YYYY-MM-DD) | `readonly prop: string;` |
| **Enumeração de Negócio** | `enum DomainEnum` | `required string Prop { get; init; }` | `TEXT NOT NULL` | `readonly prop: 'A' \| 'B';` (Union) |
| **Booleano** | `bool` | `required bool Prop { get; init; }` | `INTEGER NOT NULL` (0 ou 1) | `readonly prop: boolean;` |
| **Métrica Decimal / Flutuante** | `double` / `decimal` | `required double Prop { get; init; }` | `REAL NOT NULL` | `readonly prop: number;` |
| **Métrica Decimal Opcional** | `double?` / `decimal?` | `required double? Prop { get; init; }` | `REAL` | `readonly prop: number \| null;` |

## Ferramentas Globais

- **Plugin `dotnet-diag`**: subagente `optimizing-dotnet-performance`, skill `analyzing-dotnet-performance`
- **Plugin `ponytail`**: skills `ponytail`, `ponytail-review`
