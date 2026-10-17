---
name: domain-rules
description: Rules for Domain layer entities, value objects, domain enums, repository interfaces, and polymorphic domain exceptions.
applyTo: "src/backend/CxManager/CxManager.Domain/**/*.cs"
---

# Regras do Domínio (CxManager.Domain)

## Estrutura de Pastas
- `Entities/`: Entidades com identidade e invariantes de negócio.
- `ValueObjects/`: Objetos de valor imutáveis sem identidade.
- `Enums/`: Enumerações de domínio.
- `Interfaces/`: Contratos de repositórios e unidades de persistência (`IXxxRepository`).
- `Exceptions/`: Hierarquia polimórfica de exceções de negócio derivadas de `DomainException`.

## Diretrizes e Restrições
- **Isolamento Total de Infraestrutura**: Zero pacotes NuGet externos e zero referências a outros projetos da Solution.
- **Modelos Ricos e Encapsulamento (DDD)**:
  - As propriedades de entidades mutáveis devem possuir setters privados (`private set`), expondo mutações (como atualização cadastral ou exclusão lógica) exclusivamente por métodos de domínio expressivos que validem as invariantes.
  - Em classes seladas de entidade, os construtores devem ser privados (`private`), expondo métodos estáticos de fábrica expressivos (`Create`, `Reconstitute`) para garantir que instâncias inválidas nunca sejam criadas.
- **Tipos Temporais no Domínio**:
  - Instantes de tempo no domínio devem ser representados obrigatoriamente por `DateTimeOffset` em UTC (`DateTimeOffset.UtcNow`).
- **Categorização Polimórfica de Exceções**:
  - A taxonomia de erros e violações de regras de negócio reside exclusivamente na hierarquia de tipos derivados de `DomainException` (ex: recurso não encontrado, conflito de unicidade, violação de validação de domínio).
  - É proibido o uso de códigos de erro em string, enumerações ou propriedades auxiliares de classificação HTTP dentro do domínio, assegurando que o mapeamento para status codes ocorra na camada de apresentação via polimorfismo puro (*pattern matching*).
- **Integridade de Dados & Soft Delete**: Seguir integralmente [.agents/rules/data-integrity-rules.md](data-integrity-rules.md).

## Ferramentas Globais

- **Plugin `dotnet-diag`**: subagente `optimizing-dotnet-performance`, skill `analyzing-dotnet-performance`
- **Plugin `dotnet`**: skills `csharp-expert`, `csharp-refactoring`
- **Plugin `ponytail`**: skills `ponytail`, `ponytail-review`
