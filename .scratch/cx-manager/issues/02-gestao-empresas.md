# 02: Gestão Completa de Empresas (Companies)

**O que construir:**
Operações completas de CRUD para a entidade Empresa no backend (`POST /api/companies`, `GET /api/companies/{id}`, `PUT /api/companies/{id}` e `DELETE /api/companies/{id}` com soft delete) e diálogo de criação/edição de empresas no frontend acessível a partir do seletor superior.

**Bloqueado por:** 01: Fundação Multi-Empresa e Carga Inicial de Dados

**Status:** resolved

**Ferramentas recomendadas:**
- Skills: `propagate-feature`, `audit-api`
- MCPs: `mui-mcp`

**Critérios de aceitação:**
- [x] `POST /api/companies` cria nova empresa com validação de nome obrigatório e retorna `201 Created` com header `Location`.
- [x] `GET /api/companies/{id}` retorna detalhes da empresa ativa ou `404` caso não exista ou esteja excluída logicamente.
- [x] `PUT /api/companies/{id}` atualiza nome e descrição da empresa.
- [x] `DELETE /api/companies/{id}` realiza soft delete preenchendo `deleted_at` com timestamp UTC e retorna `204 NoContent`.
- [x] Interface no React permite abrir modal para cadastrar nova empresa e atualizar os dados da empresa ativa.

## Resposta

Implementação concluída com sucesso e verificada por testes automatizados ponta a ponta:

1. **Contratos REST Imutáveis (`CxManager.Contract`)**:
   - Criados DTOs `CreateCompanyRequest` e `UpdateCompanyRequest` como `sealed record` com propriedades `required`, sem construtores customizados e compatíveis com Native AOT.
2. **Repositório e SQL Parametrizado com Dapper.AOT (`CxManager.Infrastructure`)**:
   - Adicionados métodos `UpdateAsync` e `DeleteAsync` (soft delete com `deleted_at = @DeletedAt`) no `CompanyRepository`, preservando o predicado `deleted_at IS NULL` em todas as consultas ativas.
3. **Validação de Entrada e Casos de Uso (`CxManager.Application`)**:
   - Criados validadores `CreateCompanyRequestValidator` e `UpdateCompanyRequestValidator` com regras de tamanho e obrigatoriedade via FluentValidation.
   - Criados casos de uso isolados `CreateCompanyUseCase`, `GetCompanyByIdUseCase`, `UpdateCompanyUseCase` e `DeleteCompanyUseCase`, registrando-os explicitamente no contêiner de DI.
   - Atualizado `CompanyMapper` para expor o método estático `ToResponse` gerado pelo Riok.Mapperly.
4. **Endpoints Minimal API e Serialização AOT (`CxManager.Api`)**:
   - Mapeados endpoints `POST /api/companies` (201 Created com header Location), `GET /api/companies/{id}` (200 OK ou 404 NotFound), `PUT /api/companies/{id}` (200 OK ou 404/400) e `DELETE /api/companies/{id}` (204 NoContent).
   - Registrados novos DTOs em `AppJsonSerializerContext`.
5. **Frontend React 19 + TypeScript 7 + MUI Material v9 (`src/frontend`)**:
   - Espelhados contratos de criação e atualização em `types/company.ts`.
   - Adicionadas funções `createCompany`, `updateCompany`, `deleteCompany` e `getCompanyById` em `services/companyService.ts`.
   - Criado componente `CompanyDialog.tsx` com formulário tipado, tratamento de estados e zero renders em efeito (`key`-based mounting).
   - Integrados botões de ação com Tooltip no `AppBar` e no cartão de organização ativa do dashboard para cadastro de nova empresa e edição dos dados da empresa ativa.
6. **Suíte de Testes Automatizados**:
   - 60 testes unitários em memória (`CxManager.UnitTests`).
   - 6 testes de arquitetura com NetArchTest (`CxManager.ArchitectureTests`).
   - 20 testes de integração contra SQLite real e Kestrel HTTP (`CxManager.IntegrationTests`).
   - 100% de aprovação sem avisos em `oxlint`, `tsc -b`, `vite build` e `dotnet build`.
