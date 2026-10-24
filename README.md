# CxManager

Plataforma de gestão de experiência do cliente (Customer Experience) voltada para cadastro de contatos, histórico de respostas e análise agregada de métricas de satisfação e relacionamento (**NPS** e **CSAT**).

O sistema adota Clean Architecture com separação rigorosa de responsabilidades, backend em **.NET 10** com **ASP.NET Core Minimal APIs**, validação desacoplada com **FluentValidation**, mapeamentos estáticos com **Riok.Mapperly**, persistência em **SQLite via Dapper** (SQL nativo parametrizado), frontend em **React 19** com **TypeScript 7** e **MUI Material**, e orquestração local via **.NET Aspire**.

---

## Sumário

1. [Estrutura do Repositório](#1-estrutura-do-repositório)
2. [Como Executar o Projeto Localmente](#2-como-executar-o-projeto-localmente)
3. [Como Executar os Testes e Validações](#3-como-executar-os-testes-e-validações)
4. [Deploy Automatizado no Microsoft Azure](#4-deploy-automatizado-no-microsoft-azure)
5. [Diferenciais Técnicos](#5-diferenciais-técnicos)
6. [O Que Faria Diferente com Mais Tempo](#6-o-que-faria-diferente-com-mais-tempo)
7. [Desenvolvimento Produtivo com IA](#7-desenvolvimento-produtivo-com-ia)

---

## 1. Estrutura do Repositório

O projeto é estruturado em monorepo, isolando as camadas de negócio, transporte HTTP e persistência no backend e mantendo a interface frontend desacoplada:

```text
cx-manager/
├── src/
│   ├── backend/
│   │   └── CxManager/
│   │       ├── CxManager.Domain/             # Entidades, regras de cálculo (NPS/CSAT) e interfaces de repositório
│   │       ├── CxManager.Contract/           # DTOs imutáveis de Request e Response da API REST
│   │       ├── CxManager.Application/        # Casos de uso, validadores (FluentValidation) e mappers (Riok.Mapperly)
│   │       ├── CxManager.Infrastructure/     # Conexão SQLite, schema DDL, carga inicial e queries Dapper
│   │       ├── CxManager.Api/                # Endpoints Minimal API, tratamento global de erros e contexto HTTP
│   │       ├── CxManager.AppHost/            # Orquestrador local .NET Aspire
│   │       ├── CxManager.ServiceDefaults/    # Configurações de telemetria OpenTelemetry e Health Checks
│   │       ├── CxManager.UnitTests/          # Testes unitários de domínio, validadores e formatação CSV
│   │       ├── CxManager.IntegrationTests/   # Testes de integração com SQLite real e validação de regras analíticas
│   │       ├── CxManager.ArchitectureTests/  # Testes automatizados de isolamento de camadas (NetArchTest)
│   │       └── CxManager.slnx                # Solução .NET
│   └── frontend/
│       ├── src/
│       │   ├── components/                   # Componentes de interface (tabelas, gráficos, modais e drawers)
│       │   ├── context/                      # Gerenciamento de estado da organização ativa
│       │   ├── pages/                        # Telas de Dashboard Analítico, Contatos e Pesquisas
│       │   ├── services/                     # Serviços de consumo da API HTTP com suporte a AbortSignal
│       │   ├── theme/                        # Tema centralizado Material UI
│       │   └── types/                        # Tipagens TypeScript correspondentes aos contratos da API
│       ├── package.json
│       └── vite.config.ts
├── .agents/                                  # Governança de regras arquiteturais e runbooks operacionais
├── .scratch/                                 # Rastreamento estruturado de demandas e issues técnicas
├── docs/                                     # Especificação técnica consolidada, glossário e ADRs
├── scripts/
│   └── check.sh                              # Script unificado de validação da esteira de qualidade
└── README.md
```

### Mapa de Dependências do Backend

```text
CxManager.Contract            ← Nenhuma dependência interna
CxManager.Domain              ← Nenhuma dependência interna
  ↑
CxManager.Application         ← Domain, Contract
  ↑
CxManager.Infrastructure      ← Domain, Application
  ↑
CxManager.Api                 ← Application, Infrastructure, Contract, ServiceDefaults
CxManager.AppHost             ← Api + Frontend (orquestração local via .NET Aspire)
```

---

## 2. Como Executar o Projeto Localmente

### Pré-requisitos

- **.NET 10 SDK** (versão `10.0.x` definida em [`global.json`](global.json))
- **Node.js** (versão `26.12.x` definida em [`.node-version`](.node-version) e [`src/frontend/package.json`](src/frontend/package.json))
- **npm** (versão `11.20.x`)

Verifique as ferramentas instaladas no terminal:

```bash
dotnet --version
node -v
npm -v
```

### Banco de Dados e Carga Inicial

A persistência utiliza **SQLite** sem necessidade de instalar ou configurar servidores externos:
- No startup da API (`InitializeDatabaseAsync`), o schema de tabelas, índices e triggers é inicializado automaticamente.
- Caso o banco esteja vazio, uma carga inicial com dados demonstrativos de organização, pesquisas de satisfação, contatos e histórico de avaliações é importada de forma atômica e idempotente.
- O arquivo SQLite (`cxmanager.db`) é gerado no diretório de execução da API. Para reiniciar a base de dados ao estado inicial, basta remover esse arquivo e iniciar a API novamente.

---

### Opção A: Executar com .NET Aspire (Orquestrador Único)

Esta é a opção recomendada para desenvolvimento local. O .NET Aspire sobe o backend e o frontend integrados em um único processo, disponibilizando painel de observabilidade com logs estruturados, métricas e traces OpenTelemetry:

```bash
# 1. Instalar as dependências do frontend (apenas na primeira vez)
npm --prefix src/frontend install

# 2. Iniciar o orquestrador AppHost na raiz do repositório
dotnet run --project src/backend/CxManager/CxManager.AppHost
```

Ao iniciar, os serviços estarão acessíveis em:
- **Frontend SPA**: `http://localhost:5173`
- **Backend API**: `http://localhost:5037`
- **Aspire Dashboard**: `https://localhost:17177` (ou `http://localhost:15032` — a URL com token de acesso é exibida no terminal)

---

### Opção B: Executar sem .NET Aspire (Terminais Separados)

Caso prefira executar os serviços de forma isolada sem o orquestrador Aspire, utilize dois terminais independentes:

#### Terminal 1 — Subir a API (.NET 10)

```bash
dotnet restore src/backend/CxManager/CxManager.slnx
dotnet run --project src/backend/CxManager/CxManager.Api
```

- **API REST**: `http://localhost:5037`
- **Documentação OpenAPI / Swagger**: `http://localhost:5037/swagger`

#### Terminal 2 — Subir o Frontend (Vite)

```bash
npm --prefix src/frontend install
npm --prefix src/frontend run dev
```

- **Frontend SPA**: `http://localhost:5173` (o Vite realiza proxy automático de `/api` para `http://localhost:5037`).

---

## 3. Como Executar os Testes e Validações

O repositório possui uma esteira unificada de integridade que executa todas as checagens estáticas e suítes de testes em sequência:

```bash
./scripts/check.sh
```

O script [`check.sh`](scripts/check.sh) executa de forma determinística:
1. **Formatação de código .NET**: `dotnet format --verify-no-changes` validando padrões do `.editorconfig`.
2. **Compilação do backend**: `dotnet build` da solução com `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
3. **Testes automatizados do backend**: `dotnet test` executando mais de 320 testes distribuídos em:
   - *UnitTests*: testes unitários de cálculo de domínio (NPS, CSAT, faixas e arredondamentos), validadores FluentValidation e formatação RFC 4180.
   - *IntegrationTests*: testes de integração com SQLite real cobrindo ciclo de vida de contatos, validações de e-mail ativo, soft delete, conferência analítica e consultas agregadas.
   - *ArchitectureTests*: testes automatizados com NetArchTest garantindo isolamento entre camadas.
4. **Linter do frontend**: `oxlint` para análise estática e boas práticas do código React/TS.
5. **Checagem de tipos**: `tsc -b` executando o compilador TypeScript 7 sob regras estritas de tipagem.
6. **Build de produção do frontend**: `vite build` validando a geração de bundles otimizados.
7. **Testes do frontend**: `vitest run` executando os testes unitários de componentes e clientes de serviço.

---

## 4. Deploy Automatizado no Microsoft Azure

O projeto conta com esteira de entrega contínua (CI/CD) para o **Microsoft Azure**, com empacotamento em containers Docker multi-stage e publicação gerenciada em **Azure Container Apps**:

- **Arquitetura Cloud-Native (Azure Container Apps)**:
  - **`cxmanager-api`**: container ASP.NET Core (.NET 10) publicado a partir do [`Dockerfile`](src/backend/CxManager/Dockerfile) do backend, hospedado como microserviço serverless com escalabilidade elástica.
  - **`cxmanager-web`**: container Nginx Alpine slim publicado a partir do [`Dockerfile`](src/frontend/Dockerfile) do frontend, servindo os assets estáticos de produção da SPA React 19.
- **Automação Contínua via GitHub Actions e OIDC**:
  - A cada push na branch `main`, os workflows de auto-deployment ([`cxmanager-api-AutoDeployTrigger`](.github/workflows/cxmanager-api-AutoDeployTrigger-3dbb9d76-e92e-4e3c-87be-e8e49fdcd722.yml) e [`cxmanager-web-AutoDeployTrigger`](.github/workflows/cxmanager-web-AutoDeployTrigger-4858a075-e2e5-4929-812f-b4729e7ac97e.yml)) compilam as imagens, realizam push no **Azure Container Registry** (`acrcxmanager.azurecr.io`) e atualizam as revisões dos Container Apps no resource group `rg-cxmanager`.
  - A autenticação é realizada via **GitHub OIDC (OpenID Connect)** com credenciais federadas, eliminando senhas e segredos estáticos de longa duração.
- **Suporte ao Azure Developer CLI (`azd`)**:
  - O arquivo [`azure.yaml`](azure.yaml) na raiz do repositório integra o orquestrador `.NET Aspire` (`CxManager.AppHost`) diretamente com a CLI oficial `azd`, permitindo provisionar e publicar toda a topologia de recursos em cloud com o comando `azd up`.

---

## 5. Diferenciais Técnicos

Abaixo estão os principais diferenciais técnicos e decisões de engenharia implementadas no projeto:

### 1. Agregações Analíticas e Séries Temporais Agrupadas em SQL Nativo
Todos os indicadores analíticos (resumo geral com cálculo de NPS e CSAT, evolução mensal temporal em UTC e distribuição de notas por segmento de contato) são calculados diretamente no banco de dados via Dapper (`COUNT`, `AVG`, `CASE`, `GROUP BY`, `HAVING`). Nenhuma tupla bruta é carregada para a memória da aplicação para agrupamentos posteriores, garantindo eficiência de processamento e alocação mínima de memória.

### 2. Filtros Combinados de Período e Pesquisa
Os endpoints analíticos (`/api/analytics/summary`, `/nps-by-month`, `/nps-by-segment`) e o endpoint de exportação aceitam parâmetros combináveis de data inicial (`from`), data final (`to`, com limite superior inclusivo até o fim do dia em UTC) e filtro de pesquisa (`surveyId`), refletidos dinamicamente nos gráficos e tabelas do painel.

### 3. Suporte Multi-Empresa com Resolução de Contexto
As entidades do domínio pertencem a uma organização (`company_id`). O contexto da requisição (`ICompanyContext`) resolve a empresa ativa a partir do cabeçalho `X-Company-Id` (ou parâmetro de consulta `?companyId=`), mantendo fallback transparente para a organização principal. O frontend disponibiliza seletor de organização no cabeçalho superior para inspeção e testes de isolamento de dados.

### 4. CRUD Completo de Todas as Entidades do Domínio
Além da gestão de contatos, a aplicação implementa endpoints REST e interfaces completas para criação, edição e exclusão lógica de organizações (`/api/companies`), pesquisas de satisfação (`/api/surveys`) com tipos `NPS` (escala 0–10) e `CSAT` (escala 1–5), e submissão/edição de avaliações individuais (`/api/responses`) vinculadas a canais e contatos.

### 5. Busca Textual com SQLite FTS5 (`trigram`) e Ordenação no Servidor
A busca textual de contatos utiliza tabela virtual SQLite FTS5 com o tokenizer `trigram`, sincronizada de forma atômica por triggers nativos (`AFTER INSERT`, `AFTER UPDATE`, `AFTER DELETE`). Essa abordagem permite buscas parciais eficientes por nome e e-mail sem distinção de maiúsculas ou acentos, evitando varreduras completas com `LIKE '%...%'`. O endpoint suporta ainda ordenação dinâmica por múltiplos campos (`sortBy` e `sortDir`).

### 6. Propagação de Soft Delete para Analytics (`EXISTS` vs `INNER JOIN`)
Para manter conformidade com princípios de privacidade (LGPD), quando um contato é excluído logicamente (`deleted_at IS NOT NULL`), suas avaliações deixam de compor os números analíticos. Em consultas onde nenhuma coluna de contato precisa ser exibida (`summary` e `nps-by-month`), utiliza-se semi-join de alta performance com `WHERE EXISTS (SELECT 1 FROM contacts c WHERE c.id = r.contact_id AND c.deleted_at IS NULL)`, realizando apenas busca pontual no índice primário sem custo de alocação de tuplas de junção.

### 7. Cache em Memória de Analytics com Invalidação Reativa
As consultas agregadas de analytics contam com camada de cache em memória escopada por organização e combinações de filtro. Qualquer operação de escrita (`POST`, `PUT` ou `DELETE`) em contatos, pesquisas ou respostas invalida reativamente o cache da organização correspondente, garantindo resposta imediata sem apresentar dados obsoletos.

### 8. Exportação de Respostas em CSV RFC 4180
O endpoint `GET /api/responses/export` gera relatórios em CSV em conformidade com o padrão RFC 4180, tratando corretamente o escape de vírgulas, quebras de linha e aspas duplas em comentários, com suporte aos filtros de período e pesquisa.

### 9. Compatibilidade com Native AOT e Serialização Estática
A API utiliza serialização JSON estática via `JsonSerializerContext` (`AppJsonSerializerContext`), mapeamentos gerados em tempo de compilação com `Riok.Mapperly` e consultas Dapper preparadas para AOT (`Dapper.AOT`), eliminando reflexão em runtime e reduzindo a sobrecarga de inicialização e memória.

### 10. Validação Declarativa Desacoplada com FluentValidation e Endpoint Filters
Todas as requisições que chegam à API são validadas por regras fortemente tipadas na camada `Application` via `FluentValidation`. A execução é interceptada declarativamente nos endpoints Minimal API através de filtros de rota (`.WithValidation<TRequest>()`). Essa abordagem mantém os DTOs do `Contract` livres de anotações como DataAnnotations, centraliza regras de negócio de entrada e garante respostas imediatas em caso de inconsistência com o envelope canônico `{ "error": "mensagem legível em pt-BR" }` antes da execução dos casos de uso.

### 11. Testes de Arquitetura Contínuos (`NetArchTest`)
O projeto `CxManager.ArchitectureTests` valida em tempo de execução de testes as regras fundamentais de Clean Architecture (como invariantes que impedem que `Domain` e `Contract` conheçam camadas externas ou que `Infrastructure` acesse contratos de apresentação da API).

---

## 6. O Que Faria Diferente com Mais Tempo

1. **Testes Ponta a Ponta (E2E) no Navegador com Playwright**:
   Estruturar uma suíte de testes E2E cobrindo a jornada completa do usuário no navegador real (criação de contato, aplicação de filtros com debounce, visualização dos gráficos e exportação de CSV).
2. **Ajustes Finos de Design, Responsividade e Navegação**:
   Aprimorar animações de transição de rota, microinterações em ações assíncronas e refinamento de layout em resoluções ultra-wide e telas móveis menores.
3. **PWA (Progressive Web App)**:
   Configurar manifest, service worker e estratégia de cache local para possibilitar recursos de aplicativo nativo na web, com suporte a acesso rápido e funcionamento otimizado em dispositivos móveis.
4. **Autenticação e Extração Segura de Tenant**:
   Evoluir a resolução de empresa/organização para que o contexto de tenant não seja trafegado abertamente no request, mas extraído de forma segura e autenticada a partir das claims de um token JWT/OAuth2.
5. **Painel Administrativo para Reativação de Itens Desativados**:
   Implementar visão gerencial com filtros dedicados a registros marcados com soft delete (`deleted_at IS NOT NULL`), permitindo auditoria e reativação pontual de contatos, pesquisas ou avaliações quando necessário.

---

## 7. Desenvolvimento Produtivo com IA

O ciclo de desenvolvimento do CxManager foi estruturado com foco em governança técnica e colaboração homem-máquina, utilizando práticas rigorosas de contexto e rastreabilidade:

- **Governança em `.agents/`**:
  O diretório centraliza a identidade, as diretrizes arquiteturais e os runbooks operacionais do projeto. As regras residem em [.agents/rules/](.agents/rules/) (convenções C#, regras de frontend, integridade de dados e governança Git), enquanto runbooks operacionais residem em [.agents/skills/](.agents/skills/) (auditorias especializadas por camada, propagação de features de ponta a ponta e automação de commits semânticos).
- **Rastreabilidade e Backlog em `.scratch/`**:
  O diretório [.scratch/](.scratch/) atua como rastreador estruturado de demandas e especificações técnicas vivas ([`.scratch/mini-cx/issues/`](.scratch/mini-cx/issues/)), mapeando etapas de implementação, critérios de aceite e acompanhamento de tarefas.
- **Contexto e Decisões em `docs/`**:
  Decisões arquiteturais fundamentais são formalizadas através de registros de decisão em [docs/adr/](docs/adr/) (ADR-0001 a ADR-0005), acompanhadas pelo glossário de domínio unificado ([`GLOSSARY.md`](GLOSSARY.md)) e pela especificação técnica consolidada ([`docs/SPECIFICATION.md`](docs/SPECIFICATION.md)).
