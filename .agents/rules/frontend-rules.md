---
name: frontend-rules
description: React 19, TypeScript 7 strict contracts, MUI Material v9 theming, Oxlint type-aware analysis, API service layer isolation, AbortSignal propagation, and UX states.
applyTo: "src/frontend/**/*.{ts,tsx,css,html,json}"
---

# Regras de Frontend (React 19 + TypeScript 7 + MUI Material v9 — `src/frontend`)

## Estrutura de Pastas (`src/frontend/src/`)
- `types/`: Interfaces e tipos imutáveis (`readonly`) espelhando 1:1 os contratos REST de `CxManager.Contract`.
- `services/`: Camada isolada de comunicação HTTP com a API (`/api`), validação de resposta, tratamento padronizado do envelope de erro e suporte a cancelamento via `AbortSignal`.
- `hooks/`: Custom hooks para gerenciamento de estado de servidor, paginação, filtros e debounce de busca.
- `components/`: Componentes visuais reutilizáveis baseados em MUI Material (`@mui/material` e `@mui/icons-material`), diálogos de confirmação, estados de esqueleto/vazio/erro e elementos de formulário.
- `pages/`: Visões principais da aplicação.
- `theme/`: Tema centralizado do MUI (`createTheme`, `ThemeProvider`, `CssBaseline`) e tokens semânticos de design.

## Diretrizes de Código, Contratos Rígidos e Tipagem Estrita
- **Rigidez de Compilação (TypeScript 7)**:
  - O compilador (`tsconfig.app.json` e `tsconfig.node.json`) deve operar com rigor máximo: `strict`, `exactOptionalPropertyTypes`, `noUncheckedIndexedAccess`, `noImplicitReturns`, `noFallthroughCasesInSwitch`, `noImplicitOverride`, `noPropertyAccessFromIndexSignature`, `noUnusedLocals`, `noUnusedParameters`, `verbatimModuleSyntax`, `isolatedModules` e `erasableSyntaxOnly`.
- **Contratos Rígidos e Imutabilidade (Zero `any`)**:
  - É expressamente proibido o uso de `any`, `@ts-ignore`, `@ts-nocheck`, asserções não-nulas (`!`) ou asserções de tipo inseguras (`as unknown as T`).
  - Todas as propriedades de contratos recebidos ou enviados para a API devem ser declaradas como `readonly` e refletir com exatidão os tipos, nulabilidades explícitas (`T | null`) e nomenclatura `camelCase` definidos em `CxManager.Contract`.
- **Rigidez de Lint (`oxlint` + `oxlint-tsgolint`)**:
  - O linter (`.oxlintrc.json`) deve executar obrigatoriamente em modo *type-aware* (`typeAware: true` e `typeCheck: true`) e tolerância zero a avisos (`denyWarnings: true`, `reportUnusedDisableDirectives: "error"`).
  - Regras estritas de promessas não tratadas (`no-floating-promises`, `no-misused-promises`), atribuições/acessos inseguros (`no-unsafe-*`), exaustividade de `switch` (`switch-exhaustiveness-check`), importações de tipo explícitas (`consistent-type-imports`) e regras de hooks do React são obrigatórias.
- **Isolamento da Camada de API e Cancelamento (`AbortSignal`)**:
  - Componentes React **nunca** devem invocar `fetch` diretamente nem construir URLs de endpoint manualmente. Toda requisição deve passar pelas funções tipadas em `src/services/`.
  - Funções de leitura e busca na camada de serviços devem aceitar um `AbortSignal` opcional para cancelar requisições em voo durante digitação em campos de filtro, troca de página ou desmontagem de componente (equivalente direto ao `CancellationToken` do backend .NET).
  - Erros HTTP retornados pela API (`400`, `404`, `409`, `500`) devem extrair a mensagem legível do envelope padronizado da API e propagá-la de forma tipada para exibição clara na interface.
- **Proibição de Regra de Negócio Analítica no Cliente**:
  - O frontend **nunca** deve calcular ou recalcular métricas analíticas, percentuais ou médias sobre listas de registros. Ele exibe fielmente os dados já agregados e arredondados pelo backend.

## Diretrizes de Experiência de Uso (UX), Estados e Design System (MUI)
- **Tratamento Obrigatório de Estados Intermediários (`Loading`, `Error`, `Empty`)**:
  - A aplicação jamais pode apresentar tela em branco ou sofrer crash na árvore de componentes quando a API demora ou falha.
  - Toda listagem, painel analítico ou visualização de histórico deve implementar explicitamente:
    1. **Loading State**: Componentes `Skeleton` do MUI preservando a geometria do layout sem *layout shift* brusco.
    2. **Error State**: Alerta amigável (`Alert` do MUI) em português (`pt-BR`) exibindo a mensagem de erro e botão de nova tentativa.
    3. **Empty State**: Estado vazio claro com orientação contextual ou chamada para ação quando não houver registros para o filtro aplicado.
- **Fluidez em Busca, Paginação e Ações de Mutação**:
  - Aplicar *debounce* na busca textual e resetar para a primeira página ao alterar o filtro buscado.
  - Exigir confirmação explícita do usuário em diálogo modal (`Dialog` do MUI) antes de executar operações destrutivas (`DELETE`).
  - Refletir criações, edições e exclusões imediatamente no estado da interface sem recarregar a página do navegador (`window.location.reload()` é proibido).
- **Padronização Visual com MUI Material v9**:
  - Utilizar exclusivamente os componentes e o sistema de tema do `@mui/material` e `@mui/icons-material` com paleta semântica centralizada em `src/theme/`.
  - É proibido espalhar estilos inline (`style={{ ... }}`) ou cores hexadecimais hardcoded nos componentes; toda customização de layout deve utilizar o tema centralizado via prop tipada `sx` do MUI.
- **Validação Estática e Build**:
  - Todo código TypeScript/React deve passar sem erros ou avisos em `rtk npm --prefix src/frontend run lint`, `rtk npm --prefix src/frontend run typecheck` e `rtk npm --prefix src/frontend run build`.

## Ferramentas Globais

- **Skills de React e Arquitetura UI**: `vercel-react-best-practices`, `vercel-composition-patterns`, `vercel-react-view-transitions`, `web-design-guidelines`
- **Servidores MCP**: `mui-mcp`, `playwright`, `chrome-devtools`, `context7`
- **Plugin `ponytail`**: skills `ponytail`, `ponytail-review`, `ponytail-audit`
