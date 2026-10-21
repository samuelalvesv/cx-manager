---
name: application-rules
description: Use Cases, Commands/Queries, request validators with FluentValidation, static mappers with Riok.Mapperly, and application orchestrations.
applyTo: "src/backend/CxManager/CxManager.Application/**/*.cs"
---

# Regras de Aplicação (CxManager.Application)

## Estrutura de Pastas
- `UseCases/[Feature]/`:
  - `Commands/`: Operações de escrita (criação, edição, exclusão lógica).
  - `Queries/`: Operações de leitura, listagem paginada, histórico e resumos analíticos.
- `Mappers/`: Mapeadores estáticos gerados em tempo de compilação com `Riok.Mapperly`.
- `Validators/[Feature]/`: Validações de entrada de dados com `FluentValidation`.

## Diretrizes e Restrições
- **Isolamento de Infraestrutura**: Referencia apenas `CxManager.Domain` e `CxManager.Contract`. Proibido importar namespaces de acesso a banco (`System.Data`, `Microsoft.Data.Sqlite`, `Dapper`, etc.) ou de servidor web (`Microsoft.AspNetCore.*`).
- **Orquestração de Casos de Uso e Invocação Direta (Native AOT)**:
  - A camada de apresentação (`CxManager.Api`) consome casos de uso através de injeção direta de dependência (`IXxxUseCase`), dispensando mediadores dinâmicos (como MediatR) e reflexão em tempo de execução, garantindo total compatibilidade com Native AOT.
  - Cada caso de uso deve ser uma classe isolada (`sealed`) expondo um único método público assíncrono (`ExecuteAsync`).
- **Espelhamento de Invariantes em Validadores**: As classes em `Validators/[Feature]/` utilizam `FluentValidation` e replicam rigorosamente os limites e formatos exigidos pelas validações de domínio (`DomainValidation`), com registro explícito e tipado no contêiner de injeção de dependência sem reflexão em runtime.
- **Casos de Uso Limpos de Validação de Entrada**: Os casos de uso operam sobre a garantia prévia de contratos válidos assegurada pela borda HTTP, sendo proibida a injeção de validadores ou a implementação de verificações defensivas de formato de requisição no método `ExecuteAsync`.
- **Mapeamento em Tempo de Compilação**:
  - Conversões entre entidades/modelos de domínio e DTOs de `CxManager.Contract` devem utilizar classes `public static partial` decoradas com `[Mapper]` do `Riok.Mapperly`.
- **Validação Preventiva de Conflitos e Existência**:
  - Operações de escrita devem verificar preventivamente a existência do registro ativo e a unicidade de e-mail entre contatos não excluídos antes de efetivar a persistência, lançando as respectivas exceções polimórficas derivadas de `DomainException` para mapeamento exato aos códigos HTTP (`404` e `409`).
- **Integridade de Dados & Soft Delete**: Seguir integralmente [.agents/rules/data-integrity-rules.md](data-integrity-rules.md).

## Ferramentas Globais

- **Plugin `dotnet-diag`**: subagente `optimizing-dotnet-performance`, skill `analyzing-dotnet-performance`
- **Plugin `dotnet-experimental`**: skill `exp-mock-usage-analysis`
- **Plugin `dotnet`**: skills `csharp-expert`, `csharp-refactoring`
- **Plugin `ponytail`**: skills `ponytail`, `ponytail-review`
