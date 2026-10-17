---
name: architecture
description: Clean Architecture layers mapping, project references guidelines, React SPA integration, and Golden Rule of dependency boundaries. Use this rule when creating new projects, reference assemblies, or checking structural boundary constraints.
---

# Arquitetura — CxManager

## Visão Geral

Sistema construído com Clean Architecture + DDD pragmático no backend (.NET 10, Minimal APIs REST, Dapper + SQLite, Native AOT) e Single Page Application no frontend (React 19 + TypeScript 7 + MUI Material v9 + Vite 8), orquestrado pelo .NET Aspire.

## Mapa de Dependências

```text
CxManager.Contract            ← Nenhuma dependência (DTOs imutáveis de Request/Response REST)
CxManager.Domain              ← Nenhuma dependência (camada mais interna e pura)
  ↑
CxManager.Application         ← Domain, Contract
  ↑
CxManager.Infrastructure      ← Domain, Application (Dapper + SQLite + Seed Importer; nunca conhece Contract nem Api)
  ↑
CxManager.Api                 ← Domain, Application, Infrastructure, ServiceDefaults, Contract
CxManager.ServiceDefaults     ← Nenhuma (biblioteca compartilhada de telemetria e resiliência)
CxManager.AppHost             ← Api + Frontend Node/Vite (orquestrador .NET Aspire)
CxManager.UnitTests           ← Domain, Application, Api, Contract
CxManager.IntegrationTests    ← Infrastructure, Api
CxManager.ArchitectureTests   ← Domain, Application, Infrastructure, Api, Contract

src/frontend (React 19 SPA)   ← Consome a API REST (/api) via contratos TypeScript espelhados 1:1 com CxManager.Contract
```

## Camadas e Responsabilidades

| Camada | Projeto / Diretório | O que vai aqui | O que NÃO vai aqui |
| --- | --- | --- | --- |
| **Contract** | `src/backend/CxManager/CxManager.Contract` | DTOs imutáveis (`sealed record`) de Request e Response da API REST, contratos de paginação e envelope padronizado de erro. | Lógica de negócios, casos de uso, dependências de banco de dados ou pacotes de infraestrutura/web. |
| **Domain** | `src/backend/CxManager/CxManager.Domain` | Entidades ricas, Value Objects, Enums de domínio, interfaces de repositório (`IXxxRepository`), hierarquia de `DomainException`. | DTOs de transporte, frameworks, atributos de serialização JSON ou dependências de banco. |
| **Application** | `src/backend/CxManager/CxManager.Application` | Casos de uso (Commands, Queries), mapeadores estáticos (`Riok.Mapperly`), validadores (`FluentValidation`), interfaces de serviços de aplicação. | Acesso direto a banco, conexões SQLite, queries SQL, dependências de ASP.NET Core. |
| **Infrastructure** | `src/backend/CxManager/CxManager.Infrastructure` | Fábrica de conexões SQLite, scripts/inicialização de schema, importador reproduzível de seed, repositórios concretos com SQL parametrizado via Dapper + Dapper.AOT. | Regras de negócio de domínio, endpoints HTTP. |
| **Presentation (API)** | `src/backend/CxManager/CxManager.Api` | Endpoints Minimal API sob `/api`, `JsonSerializerContext` para Source Generation AOT, `IExceptionHandler` global, composição de injeção de dependência. | Lógica de negócio, queries SQL ou acesso direto a conexões de banco. |
| **Orchestration** | `src/backend/CxManager/CxManager.AppHost` | Registro e orquestração da API .NET e do servidor de desenvolvimento Vite/React no .NET Aspire. | Lógica de negócio. |
| **Shared** | `src/backend/CxManager/CxManager.ServiceDefaults` | OpenTelemetry, Health Checks, Service Discovery e Resiliência HTTP. | Lógica de negócio. |
| **Tests** | `src/backend/CxManager/CxManager.*Tests` | Testes unitários em memória (`UnitTests`), testes de integração em banco SQLite real validando valores de conferência (`IntegrationTests`) e testes de arquitetura (`ArchitectureTests` com `NetArchTest`). | Código de produção. |
| **Frontend SPA** | `src/frontend` | Páginas, componentes React reutilizáveis, hooks de estado/busca, camada isolada de cliente HTTP tipado (`services/`), tipos TypeScript espelhados (`types/`) e tokens CSS. | Chamadas `fetch` espalhadas em componentes visuais, tipos `any`, regras de agregação que pertencem ao SQL. |

## Fluxo de uma Requisição

```text
[React SPA (src/frontend)]
   ↓ (HTTP/JSON sob /api com AbortSignal)
[CxManager.Api (Minimal API + JsonSerializerContext AOT)]
   ↓ (Injeção direta de caso de uso)
[CxManager.Application (UseCase + FluentValidation + Mapperly)]
   ↓
[CxManager.Domain (Entidades e Invariantes)] + [CxManager.Infrastructure (Dapper.AOT + SQL parametrizado no SQLite)]
```

## Estratégia de Contratos Rígidos & Compilação AOT

1. **Minimal APIs + System.Text.Json Source Generation**: Os endpoints REST utilizam `TypedResults` e serialização gerada em tempo de compilação (`JsonSerializerContext`), eliminando reflexão dinâmica e garantindo compatibilidade com Native AOT.
2. **Dapper + SQL Escrito à Mão + Dapper.AOT**: Toda persistência e agregação analítica é executada em SQL nativo parametrizado sobre SQLite com geração de código ADO.NET em tempo de compilação (`[DapperAot]`), proibindo ORMs completos (EF Core) ou agregações em memória.
3. **Sincronia Estrita Backend ↔ Frontend**: Os contratos definidos em `CxManager.Contract` possuem correspondência 1:1 com as interfaces imutáveis TypeScript em `src/frontend/src/types/`, garantindo segurança de tipos de ponta a ponta.

## Regra de Ouro
>
> A dependência no backend SEMPRE aponta para dentro (em direção ao `Domain` e `Contract`).
> O `Domain` e o `Contract` nunca referenciam nenhum outro projeto da solução.

## Ferramentas Globais

- **Plugin `dotnet-msbuild`**: subagentes `msbuild-code-review`, `build-perf`, `msbuild`, skills `directory-build-organization`, `msbuild-antipatterns`, `including-generated-files`, `incremental-build`, `check-bin-obj-clash`, `copy-to-output-directory`, `binlog-failure-analysis`, `binlog-generation`
- **Plugin `dotnet-nuget`**: skill `convert-to-cpm`
- **Plugin `dotnet-template-engine`**: subagente `template-engine`, skills `template-instantiation`, `template-discovery`
- **Plugin `dotnet`**: skills `csharp-expert`, `csharp-refactoring`, `setup-local-sdk`
- **Plugin `ponytail`**: skills `ponytail`, `ponytail-review`, `ponytail-audit`
