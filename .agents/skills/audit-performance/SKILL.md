---
name: audit-performance
description: Diagnóstico e auditoria técnica de performance, alocações de memória, compilação AOT, eficiência de queries SQL/Dapper no SQLite, indexação, renderização React e benchmarking no CxManager.
---

# Auditoria de Performance (Audit Performance)

Runbook operacional especializado para guiar a identificação, diagnóstico e resolução de gargalos de performance, alocações desnecessárias no heap, latência de execução, compatibilidade Native AOT, eficiência de consultas SQL/Dapper e índices no SQLite, performance de renderização React e benchmarking no ecossistema do CxManager.

---

## Modos de Operação Transparentes

A skill opera em dois modos distintos de execução:

### 1. Modo Amplo (Full Audit)
Invocação manual para análise aprofundada da solução completa ou de uma camada/projeto específico:
```bash
/audit-performance
/audit-performance [caminho_ou_camada]
```
- Produz o relatório visual completo de auditoria de performance.

### 2. Modo Cirúrgico / Focado (Scoped Audit)
Invocação com escopo restrito aos arquivos alterados no diff (utilizado primordialmente pelo orquestrador de `code-review` via subagente):
```bash
/audit-performance --diff
/audit-performance --diff [lista_de_arquivos]
```
- Analisa exclusivamente as alterações do diff com isolamento de contexto e retorna os achados estritamente dentro do **Contrato de Achados Padronizados**.

---

## Regras de Governança Aplicáveis

A avaliação de performance e eficiência técnica é regida integralmente pelas regras documentadas em [.agents/rules/](../../rules/):

- **Convenções C# & AOT**: Seguir integralmente [.agents/rules/csharp-conventions.md](../../rules/csharp-conventions.md).
- **Infraestrutura & Banco SQLite**: Seguir integralmente [.agents/rules/infrastructure-rules.md](../../rules/infrastructure-rules.md).
- **Integridade de Dados & Agregações SQL**: Seguir integralmente [.agents/rules/data-integrity-rules.md](../../rules/data-integrity-rules.md).
- **Frontend React & TypeScript**: Seguir integralmente [.agents/rules/frontend-rules.md](../../rules/frontend-rules.md).

---

## Orquestração de Recursos Globais por Plugin

A skill `audit-performance` é a dona exclusiva de toda a eficiência técnica de máquina e performance, orquestrando as seguintes ferramentas especializadas instaladas globalmente:

### Plugin `dotnet-diag`
- Subagente `optimizing-dotnet-performance`
- Skill `analyzing-dotnet-performance`
- Skill `microbenchmarking`
- Skills `dotnet-trace-collect`, `dump-collect`

### Skills Globais de Frontend
- Skill `vercel-react-best-practices`

---

## Fluxo de Execução da Auditoria

```mermaid
flowchart TD
    A[Invocação da Skill] --> B{Modo de Operação}
    B -->|Modo Amplo /audit-performance| C[1. Delimitação de Hot Paths Backend e Frontend]
    C --> D[2. Revisão de Código C# e Source Generators AOT]
    D --> E[3. Deep Scan de Anti-Patterns .NET]
    E --> F[4. Auditoria de Queries SQL Dapper e Índices SQLite]
    F --> G[5. Auditoria de Performance React e Benchmarking]
    G --> H[6. Relatório Completo de Auditoria]
    B -->|Modo Cirúrgico --diff| I[1. Delimitação Restrita aos Arquivos do Diff]
    I --> J[2. Scan Direcionado nos Arquivos Alterados]
    J --> K[3. Avaliação de Queries SQL e Renderização React do Diff]
    K --> L[4. Retorno no Contrato de Achados Padronizados]
```

---

## Passos de Execução: Modo Amplo (Full Audit)

### Passo 1: Delimitação de Hot Paths
Identificar fluxos críticos de execução no escopo avaliado (agregações analíticas, listagens paginadas, importação de seed e renderização de tabelas/cards).

### Passo 2: Avaliação Técnica de Código C# e Native AOT
Conduzir análise técnica dos fluxos críticos em conformidade estrita com [.agents/rules/csharp-conventions.md](../../rules/csharp-conventions.md), acionando o subagente `optimizing-dotnet-performance`.

### Passo 3: Deep Scan de Anti-Patterns .NET
Invocar a skill `analyzing-dotnet-performance` nos arquivos C# do escopo avaliado para detecção sistemática de anti-patterns de máquina.

### Passo 4: Auditoria de Queries SQL Dapper e Índices SQLite
Seguir integralmente [.agents/rules/infrastructure-rules.md](../../rules/infrastructure-rules.md) e [.agents/rules/data-integrity-rules.md](../../rules/data-integrity-rules.md), verificando se agregações e paginação ocorrem integralmente no banco SQLite com uso adequado de índices e `Dapper.AOT`.

### Passo 5: Performance de Frontend React e Benchmarking
No frontend (`src/frontend`), acionar `vercel-react-best-practices` e seguir [.agents/rules/frontend-rules.md](../../rules/frontend-rules.md). Para validação empírica de backend, acionar `microbenchmarking` ou `dotnet-trace-collect`.

---

## Passos de Execução: Modo Cirúrgico / Focado (`--diff`)

Quando invocado via subagente a partir do `code-review` ou diretamente com `--diff`:

1. **Delimitação Cirúrgica**: Filtrar estritamente os arquivos modificados na sessão/diff pertencentes a código de produção.
2. **Scan Focado de Código C#**: Executar `analyzing-dotnet-performance` exclusivamente sobre os arquivos `.cs` alterados.
3. **Auditoria de Persistência SQL no Diff**: Caso o diff contenha repositórios, schema ou consultas SQLite, verificar conformidade com [.agents/rules/infrastructure-rules.md](../../rules/infrastructure-rules.md) e [.agents/rules/data-integrity-rules.md](../../rules/data-integrity-rules.md).
4. **Auditoria de Performance Frontend no Diff**: Caso o diff contenha arquivos `.ts`/`.tsx` em `src/frontend`, acionar `vercel-react-best-practices`.
5. **Emissão de Retorno Padronizado**: Estruturar a resposta estritamente conforme o **Contrato de Achados Padronizados**.

---

## Contrato de Achados Padronizados (Modo Cirúrgico / Focado)

No modo cirúrgico (`--diff`), o subagente deve responder estritamente com a estrutura abaixo, sem relatórios visuais prolixos:

### 1. Síntese Executiva
Um parágrafo conciso resumindo o estado de eficiência técnica de máquina dos arquivos avaliados no diff.

### 2. Lista de Achados Padronizados
Cada ocorrência deve seguir o formato exato aceito pelo `code-review`:

> **[Severidade] [Performance & Recursos]** Título objetivo do achado
> - **Localização**: `caminho/do/arquivo:linha`
> - **Comportamento Atual**: Descrição factual do que foi implementado.
> - **Impacto / Risco Técnico**: Explicação técnica do impacto (alocações de memória, agregação em memória fora do SQL, ausência de índice SQLite, reflexão em runtime ou re-renderização excessiva no React).
> - **Ação Recomendada**: O que deve ser ajustado, acompanhado do trecho de código/diff sugerido.

### 3. Recomendação Formal de Auditoria Ampla
Se durante a análise do diff forem identificados gargalos sistêmicos ou problemas que extrapolam o escopo imediato das linhas alteradas, incluir recomendação formal:
> ⚠️ **Recomendação**: Problemas estruturais de performance identificados fora do escopo cirúrgico. Recomenda-se executar `/audit-performance [escopo]` para uma varredura completa da camada.

---

## Estrutura do Relatório de Auditoria Completa (Modo Amplo)

Ao concluir a auditoria no modo amplo, apresentar o relatório formatado:

### ⚡ Relatório de Auditoria de Performance

#### Resumo Executivo
- **Escopo Auditado**: [Caminho / Camadas]
- **Principais Oportunidades Identificadas**: [Resumo objetivo em 1-2 frases]

#### 1. Revisão Arquitetural de Hot Paths e Native AOT
- **Gargalos Estruturais**: Identificação de pontos com sobrecarga de CPU, reflexão ou alocações no heap.

#### 2. Varredura de Anti-Patterns (Deep Scan)
| Severidade | Categoria | Localização | Descrição do Anti-Pattern |
| :---: | :--- | :--- | :--- |
| [Severidade] | [Categoria Técnica] | `caminho/do/arquivo:linha` | [Descrição factual do anti-pattern identificado] |

#### 3. Auditoria de Queries SQL Dapper e Índices SQLite
- Análise de consultas SQL, agregações no banco, índices e uso de `Dapper.AOT`.

#### 4. Eficiência de Renderização e Rede no Frontend React
- Análise de waterfalls de requisição, cancelamento via `AbortSignal`, debounce e re-renders.

#### 5. Recomendações Acionáveis
> **[Severidade] [Performance & Recursos]** Título da Otimização
> - **Localização**: `caminho/do/arquivo:linha`
> - **Comportamento Atual**: Descrição factual do gargalo identificado.
> - **Impacto / Risco Técnico**: Explicação técnica do impacto (alocações, latência, full table scan).
> - **Ação Recomendada**: Ajuste conceitual otimizado em conformidade com as regras do CxManager.
