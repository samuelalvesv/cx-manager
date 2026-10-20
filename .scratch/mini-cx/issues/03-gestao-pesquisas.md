# 03: Gestão Completa de Pesquisas (Surveys)

**O que construir:**
Operações de CRUD de pesquisas no backend (`POST /api/surveys`, `GET /api/surveys`, `GET /api/surveys/{id}`, `PUT`, `DELETE`) isoladas pela empresa ativa, validando tipos de métrica (`NPS` escala 0–10 ou `CSAT` escala 1–5), com visão dedicada de Gestão de Pesquisas no frontend.

**Bloqueado por:** 01: Fundação Multi-Empresa e Carga Inicial de Dados

**Status:** resolved

**Ferramentas recomendadas:**
- Skills: `propagate-feature`, `csharp-expert`
- MCPs: `mui-mcp`

**Critérios de aceitação:**
- [x] `POST /api/surveys` valida campos obrigatórios, restringe `type` a `NPS` ou `CSAT`, associa ao `company_id` ativo e retorna `201 Created` com header `Location`.
- [x] `GET /api/surveys` lista pesquisas ativas da empresa (`id`, `name`, `type`, `createdAt`).
- [x] `GET /api/surveys/{id}` detalha pesquisa ativa da empresa ou retorna `404`.
- [x] `PUT /api/surveys/{id}` atualiza o nome da pesquisa.
- [x] `DELETE /api/surveys/{id}` executa soft delete da pesquisa (`deleted_at = UTC now`).
- [x] Tela de Gestão de Pesquisas no React lista os questionários da empresa com identificação de tipo/escala e formulário modal para criar nova pesquisa.

## Resposta

Implementação concluída com sucesso e verificada por testes automatizados ponta a ponta:

1. **Domínio e Modelagem (`CxManager.Domain`)**:
   - Atualizada a entidade `Survey` para validação de tipo de métrica (`NPS` ou `CSAT`) e invariantes de nome.
   - Definida a interface `ISurveyRepository` com isolamento estrito por `companyId` e suporte a soft delete.
2. **Contratos REST Imutáveis (`CxManager.Contract`)**:
   - Criados DTOs `CreateSurveyRequest`, `UpdateSurveyRequest` e `SurveyResponse` como `sealed record` com propriedades `required` e compatibilidade total com Native AOT.
3. **Persistência SQL com Dapper.AOT (`CxManager.Infrastructure`)**:
   - Implementado `SurveyRepository` com SQL parametrizado nativo para SQLite, respeitando o predicado `deleted_at IS NULL` e índice `ix_surveys_company_active(company_id, deleted_at)`.
   - Registrado `ISurveyRepository` no contêiner de injeção de dependência.
4. **Mapeamento e Casos de Uso (`CxManager.Application`)**:
   - Criado `SurveyMapper` com Riok.Mapperly para mapeamento em tempo de compilação sem reflexão.
   - Criados validadores `CreateSurveyRequestValidator` e `UpdateSurveyRequestValidator` com FluentValidation.
   - Implementados os casos de uso `CreateSurveyUseCase`, `UpdateSurveyUseCase`, `DeleteSurveyUseCase`, `GetSurveysUseCase` e `GetSurveyByIdUseCase` com injeção de `ICompanyContext`.
5. **Endpoints Minimal API e Serialização AOT (`CxManager.Api`)**:
   - Mapeados endpoints sob `/api/surveys` com proteção por `RequireCompanyContextFilter`: `POST` (201 Created com header Location), `GET /` (200 OK), `GET /{id}` (200 OK ou 404), `PUT /{id}` (200 OK) e `DELETE /{id}` (204 NoContent).
   - Registrados DTOs de request e response em `AppJsonSerializerContext`.
6. **Interface Frontend React 19 + TypeScript 7 + MUI Material v9 (`src/frontend`)**:
   - Definidos tipos estritos em `src/types/survey.ts` e serviços HTTP com `AbortSignal` em `src/services/surveyService.ts`.
   - Criados componentes `SurveyDialog.tsx` (criação e edição com seletor de métrica NPS/CSAT e escalas explicadas) e `DeleteSurveyDialog.tsx` (confirmação modal de soft delete).
   - Criada página `SurveysPage.tsx` com navegação no `AppBar`, tratamento dos estados `loading` (Skeletons), `error` (Alert com retry) e `empty` (Call to Action amigável).
7. **Validação da Suíte de Testes e Compilação**:
   - 92 testes unitários (`CxManager.UnitTests`) passando.
   - 39 testes de integração (`CxManager.IntegrationTests`) passando.
   - 6 testes de arquitetura (`CxManager.ArchitectureTests`) passando.
   - Compilação backend com 0 warnings (`TreatWarningsAsErrors=true`).
   - Lint (`oxlint`), typecheck (`tsc -b`) e build (`vite build`) do frontend com 0 erros e 0 warnings.
