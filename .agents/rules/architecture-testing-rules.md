---
name: architecture-testing-rules
description: Rules for Architecture Tests, NetArchTest constraints, Clean Architecture dependency boundaries verification, AOT compliance, and naming conventions.
applyTo: "src/backend/CxManager/CxManager.ArchitectureTests/**/*.cs"
---

# Regras de Testes de Arquitetura (CxManager.ArchitectureTests)

## Diretrizes e Responsabilidades
- **Foco**: Testes de arquitetura automatizados para validar programaticamente o mapa de dependências e regras de design da Clean Architecture e conformidade AOT.
- **Validação de Dependências**:
  - As camadas `CxManager.Domain` e `CxManager.Contract` não podem referenciar nenhum outro projeto da Solution nem bibliotecas de infraestrutura ou banco de dados.
  - A camada `CxManager.Application` deve referenciar apenas `Domain` e `Contract` (proibido referenciar `Infrastructure`, `Api`, `Dapper` ou `Microsoft.Data.Sqlite`).
  - A camada `CxManager.Infrastructure` deve referenciar apenas `Domain` e `Application` (proibido referenciar ou depender de tipos de `CxManager.Contract` ou `CxManager.Api`).
- **Padrões de Nomenclatura e Design**:
  - Garantir que todas as interfaces comecem com `I` (ex: `IXxxRepository`, `IXxxUseCase`).
  - Garantir que classes concretas dentro de `Domain`, `Application`, `Contract` e `Infrastructure` sejam marcadas como `sealed` (exceto classes estáticas de extensão/mapeamento `static` e a classe base abstrata `DomainException`).
  - Garantir que todos os DTOs concretos em `CxManager.Contract` sejam imutáveis (`sealed record`).
- **Validação de Exaustividade de Mapeamento de Exceções**:
  - Testes de arquitetura devem inspecionar os componentes de tratamento de erros (`IExceptionHandler`) da camada `CxManager.Api` para assegurar que todas as subclasses concretas de `DomainException` possuam tratamento explícito.
- **Tecnologia**: Usar a biblioteca **`NetArchTest.eNhancedEdition`** para definir as asserções de arquitetura de forma fluida.

## Ferramentas Globais

- **Plugin `dotnet-msbuild`**: subagente `msbuild-code-review`, skills `directory-build-organization`, `msbuild-antipatterns`
- **Plugin `ponytail`**: skill `ponytail`
