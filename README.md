# CxManager

Plataforma de gestão de experiência do cliente (Customer Experience — **NPS** e **CSAT**), gestão de contatos e análise de pesquisas de satisfação, construída com **Clean Architecture**, **DDD pragmático**, **.NET 10 (Minimal APIs REST + Native AOT)**, **Dapper.AOT + SQLite** no backend e **React 19 + TypeScript 7 + MUI Material v9** no frontend, orquestrada com **.NET Aspire**.

---

## 🚀 Stack Tecnológica

### Backend (`src/backend/CxManager`)
- **Runtime & Linguagem**: .NET 10 (`10.0.402+`) e C# moderno com `<Nullable>enable</Nullable>` e `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- **API & Serialização AOT**: ASP.NET Core Minimal APIs (`TypedResults`) + `System.Text.Json` Source Generation (`JsonSerializerContext`)
- **Persistência & Consultas**: SQLite (`Microsoft.Data.Sqlite`) + **Dapper** e **Dapper.AOT** (SQL nativo parametrizado escrito à mão, sem ORM pesado)
- **Mapeamento & Validação**: `Riok.Mapperly` (geração estática em tempo de compilação, zero reflexão) e `FluentValidation`
- **Orquestração & Observabilidade**: .NET Aspire (`CxManager.AppHost`), OpenTelemetry, Health Checks e Resiliência HTTP (`CxManager.ServiceDefaults`)
- **Gerenciamento de Pacotes**: NuGet Central Package Management (`Directory.Packages.props`) e políticas unificadas de build (`Directory.Build.props`)
- **Testes Automatizados**: `xUnit v3` sobre `Microsoft.Testing.Platform`, `NetArchTest.eNhancedEdition` e cobertura de código integrada

### Frontend (`src/frontend`)
- **UI & Componentes**: **React 19.3** + **MUI Material v9** (`@mui/material`, `@mui/icons-material`, `@emotion/react`, `@emotion/styled`)
- **Tipagem & Contratos**: **TypeScript 7** (`tsc -b`) configurado em modo de rigor máximo (`strict`, `noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`, `verbatimModuleSyntax`, `erasableSyntaxOnly`)
- **Linter Type-Aware**: **Oxlint** + `oxlint-tsgolint` (`typeAware: true`, `typeCheck: true`, `denyWarnings: true`)
- **Bundler & Dev Server**: **Vite 8**

---

## 🏛️ Arquitetura e Mapa de Dependências

O projeto aplica isolamento estrito de camadas (verificado automaticamente por `CxManager.ArchitectureTests`), garantindo que regras de negócio, contratos de transporte HTTP e persistência SQL permaneçam desacoplados.

### Mapa de Dependências

```text
CxManager.Contract            ← Nenhuma dependência (DTOs imutáveis de Request/Response REST)
CxManager.Domain              ← Nenhuma dependência (entidades, invariantes e interfaces de repositório)
  ↑
CxManager.Application         ← Domain, Contract (Use Cases, FluentValidation, Riok.Mapperly)
  ↑
CxManager.Infrastructure      ← Domain, Application (Dapper + Dapper.AOT + SQLite; não conhece Contract nem Api)
  ↑
CxManager.Api                 ← ServiceDefaults, Application, Infrastructure, Contract
CxManager.ServiceDefaults     ← Nenhuma (OpenTelemetry, Health Checks, Service Discovery e Resiliência)
CxManager.AppHost             ← Api + Frontend (orquestrador .NET Aspire)

CxManager.UnitTests           ← Domain, Application, Contract, Api
CxManager.IntegrationTests    ← Infrastructure, Api
CxManager.ArchitectureTests   ← Domain, Application, Contract, Infrastructure, Api

src/frontend (React 19 SPA)   ← Consome a API REST (/api) via contratos imutáveis espelhados 1:1 com CxManager.Contract
```

### Regra de Ouro (Golden Rule)
> **A dependência no backend SEMPRE aponta para dentro (em direção ao `Domain` e `Contract`).**  
> `CxManager.Domain` e `CxManager.Contract` não referenciam nenhum outro projeto da solução, e `CxManager.Infrastructure` nunca referencia `CxManager.Contract` ou `CxManager.Api`.

### Fluxo de uma Requisição

```text
[React 19 + MUI v9 SPA (src/frontend)]
        │ (HTTP/JSON sob /api com AbortSignal)
        ▼
[CxManager.Api] (Minimal API + TypedResults + JsonSerializerContext AOT)
        │
        ▼
[CxManager.Application] (UseCases + FluentValidation + Riok.Mapperly)
        │
        ├───────────────► [CxManager.Domain] (Entidades, Escalas NPS/CSAT e Invariantes)
        │
        ▼
[CxManager.Infrastructure] (Dapper.AOT + SQL parametrizado + Soft Delete deleted_at IS NULL)
        │
        ▼
[SQLite Database]
```

---

## 📁 Estrutura do Repositório

```text
cx-manager/
├── src/
│   ├── backend/
│   │   └── CxManager/
│   │       ├── CxManager.Domain/             # Entidades, Value Objects, Enums, Interfaces e Exceções de Domínio
│   │       ├── CxManager.Contract/           # DTOs imutáveis (sealed record) de Request e Response REST
│   │       ├── CxManager.Application/        # Casos de uso (Commands/Queries), Validadores e Mappers (Mapperly)
│   │       ├── CxManager.Infrastructure/     # Conexão SQLite, Schema, Seed Importer e Repositórios Dapper.AOT
│   │       ├── CxManager.Api/                # Endpoints Minimal API, JsonSerializerContext AOT e Error Handling
│   │       ├── CxManager.AppHost/            # Orquestrador .NET Aspire
│   │       ├── CxManager.ServiceDefaults/    # Telemetria OpenTelemetry, Health Checks e Resiliência
│   │       ├── CxManager.UnitTests/          # Testes unitários em memória
│   │       ├── CxManager.IntegrationTests/   # Testes de integração com SQLite real e validação do seed
│   │       ├── CxManager.ArchitectureTests/  # Testes de fronteira e isolamento arquitetural (NetArchTest)
│   │       ├── CxManager.slnx                # Solução .NET
│   │       ├── Directory.Build.props         # Propriedades globais MSBuild e runner Microsoft.Testing.Platform
│   │       └── Directory.Packages.props      # Central Package Management (CPM)
│   └── frontend/                             # Single Page Application (React 19 + TS 7 + MUI v9 + Vite 8)
│       ├── src/
│       │   ├── components/                   # Componentes visuais reutilizáveis baseados em MUI Material
│       │   ├── hooks/                        # Custom hooks de estado, paginação e debounce
│       │   ├── pages/                        # Visões de Gestão de Contatos e Dashboard Analítico (NPS/CSAT)
│       │   ├── services/                     # Camada isolada de cliente HTTP tipado com suporte a AbortSignal
│       │   ├── theme/                        # Tema centralizado do MUI e tokens semânticos
│       │   └── types/                        # Contratos TypeScript readonly espelhados com CxManager.Contract
│       ├── .oxlintrc.json                    # Configuração estrita do Oxlint Type-Aware
│       ├── package.json                      # Scripts e dependências do frontend
│       ├── tsconfig.app.json                 # Configuração estrita do compilador TypeScript 7
│       └── vite.config.ts                    # Configuração do Vite 8
├── .agents/                                  # Governança, regras arquiteturais e skills operacionais de IA
│   ├── AGENTS.md                             # Identidade do agente, convenções e catálogo de ferramentas
│   ├── rules/                                # Regras modulares por camada (arquitetura, C#, SQL, frontend, testes)
│   └── skills/                               # Runbooks operacionais (/code-review, /audit-*, /propagate-feature, /commit)
├── .editorconfig                             # Regras de estilo e severidade de analisadores Roslyn
├── global.json                               # Pinagem do .NET 10 SDK e runner Microsoft.Testing.Platform
└── README.md
```

---

## 🛠️ Tutorial Passo a Passo: Como Rodar o Projeto

### 1. Pré-requisitos

Certifique-se de ter instalado no seu ambiente:
- **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** (versão `10.0.402` definida em [global.json](global.json))
- **[Node.js](https://nodejs.org/)** (versão `26.12+`) e **npm** (`11.20+`)

Verifique as versões instaladas no terminal:
```bash
dotnet --version
node -v
npm -v
```

---

### 2. Restaurar Dependências

Na raiz do repositório, restaure os pacotes NuGet da solução backend e instale as dependências npm do frontend:

```bash
# 1. Restaurar pacotes NuGet da solução backend
dotnet restore src/backend/CxManager/CxManager.slnx

# 2. Instalar pacotes npm do frontend
npm --prefix src/frontend install
```

---

### 3. Compilar e Validar (Backend & Frontend)

Valide a compilação completa e o linter estático nas duas pontas:

```bash
# Compilar toda a solução .NET (11 projetos, zero warnings)
dotnet build src/backend/CxManager/CxManager.slnx

# Executar linter type-aware (Oxlint + TypeScript 7) no frontend
npm --prefix src/frontend run lint

# Executar checagem de tipos e build de produção do frontend
npm --prefix src/frontend run build
```

---

### 4. Executar a Aplicação

Inicie o orquestrador **.NET Aspire AppHost** na raiz do repositório:

```bash
dotnet run --project src/backend/CxManager/CxManager.AppHost
```
- **Aspire Dashboard**: `https://localhost:17177` — centraliza logs estruturados, traces OpenTelemetry, métricas, health checks (`/health` e `/alive`) e endpoints dos serviços.

Para subir o servidor de desenvolvimento do frontend com *Hot Module Replacement (HMR)*:
```bash
npm --prefix src/frontend run dev
```
- **Frontend SPA (Vite 8)**: `http://localhost:5173`
- **Backend API (.NET 10)**: `http://localhost:5037` (também executável isoladamente via `dotnet run --project src/backend/CxManager/CxManager.Api`)

---

### 5. Executar os Testes Automatizados

A suíte utiliza o **Microsoft.Testing.Platform** com **xUnit v3**, permitindo execução paralela ultrarrápida diretamente pela solução:

```bash
# Executar todos os testes da solução (Unitários, Integração e Arquitetura)
dotnet test --solution src/backend/CxManager/CxManager.slnx

# Executar apenas os Testes de Unidade
dotnet test --project src/backend/CxManager/CxManager.UnitTests

# Executar apenas os Testes de Integração (SQLite real + Seed)
dotnet test --project src/backend/CxManager/CxManager.IntegrationTests

# Executar apenas os Testes de Arquitetura (NetArchTest)
dotnet test --project src/backend/CxManager/CxManager.ArchitectureTests
```

---

## ⚡ Referência Rápida de Comandos

| Objetivo | Comando |
| :--- | :--- |
| **Restaurar Backend** | `dotnet restore src/backend/CxManager/CxManager.slnx` |
| **Compilar Backend** | `dotnet build src/backend/CxManager/CxManager.slnx` |
| **Rodar Todos os Testes (.NET)** | `dotnet test --solution src/backend/CxManager/CxManager.slnx` |
| **Subir API Backend** | `dotnet run --project src/backend/CxManager/CxManager.Api` |
| **Subir Aspire AppHost** | `dotnet run --project src/backend/CxManager/CxManager.AppHost` |
| **Instalar Dependências Frontend** | `npm --prefix src/frontend install` |
| **Subir Frontend (Dev / HMR)** | `npm --prefix src/frontend run dev` |
| **Lint Type-Aware Frontend** | `npm --prefix src/frontend run lint` |
| **Checagem de Tipos Frontend** | `npm --prefix src/frontend run typecheck` |
| **Build de Produção Frontend** | `npm --prefix src/frontend run build` |

---

## 📚 Documentação e Governança Técnica

As diretrizes arquiteturais, invariantes de integridade de dados e padrões por camada estão documentados em [.agents/](.agents/):

- **[Identidade e Governança do Agente](.agents/AGENTS.md)**
- **[Arquitetura e Fronteiras de Dependência](.agents/rules/architecture.md)**
- **[Integridade de Dados, Soft Delete e Escalas NPS/CSAT](.agents/rules/data-integrity-rules.md)**
- **[Convenções C# e Native AOT](.agents/rules/csharp-conventions.md)**
- **[Regras de Frontend (React 19 + TS 7 + MUI v9)](.agents/rules/frontend-rules.md)**
- **[Governança Git](.agents/rules/git.md)**
