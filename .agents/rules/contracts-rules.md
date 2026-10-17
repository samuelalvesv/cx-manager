---
name: contracts-rules
description: Data transfer objects (DTOs), REST JSON request/response contracts, pagination shapes, error envelopes, and AOT serialization compatibility.
applyTo: "src/backend/CxManager/CxManager.Contract/**/*.cs"
---

# Regras de Contratos (CxManager.Contract)

## Estrutura de Pastas
- `Requests/`:
  - `[Feature]/`: DTOs de requisição e parâmetros de consulta fortemente tipados por funcionalidade.
- `Responses/`:
  - `[Feature]/`: DTOs de resposta fortemente tipados por funcionalidade.
  - `PagedResponse.cs`: Contrato genérico paginado (`items`, `total`, `page`, `pageSize`).
  - `ErrorResponse.cs`: Envelope padronizado de erro (`{ "error": "mensagem legível" }`).
- `ContractDefaults.cs`: Constantes de paginação (`DefaultPage = 1`, `DefaultPageSize = 20`, `MaxPageSize = 100`) e limites de transporte.

## Diretrizes e Restrições
- **Isolamento Total**: Zero dependências de projetos internos (`Domain`, `Application`, `Infrastructure`, `Api`) e zero dependências de banco de dados ou frameworks web.
- **DTOs Imutáveis com `required`**: Todos os DTOs concretos de Request e Response devem ser declarados como `sealed record` com propriedades `{ get; init; }` marcadas obrigatoriamente como `required`.
- **Sem Construtores Customizados**: É proibido o uso de construtores parametrizados em DTOs de contratos. Todas as instanciações devem utilizar inicializadores de objeto nomeados (`{ Prop = valor }`), forçando a declaração explícita de todos os campos (mesmo quando o valor for explicitamente `null`) e garantindo desserialização direta via `System.Text.Json` Source Generation.
- **Coleções em Respostas**: Coleções em DTOs devem utilizar obrigatoriamente `IReadOnlyList<T>` em vez de `IEnumerable<T>` ou `List<T>`, garantindo imutabilidade, contagem indexada $O(1)$ e serialização direta sem múltiplas enumerações.
- **Contrato Oficial de Paginação e Erros**:
  - Respostas paginadas devem expor exatamente as propriedades `Items` (`IReadOnlyList<T>`), `Total` (`int`), `Page` (`int`) e `PageSize` (`int`), serializadas em `camelCase`.
  - Respostas de falha de validação ou regra de negócio (`400`, `404`, `409`) devem utilizar o contrato único `ErrorResponse` contendo a propriedade `Error` (`string`).
- **Tipagem Forte e Tipos Temporais Determinísticos**:
  - É proibido expor propriedades de tipos genéricos (`object`, `dynamic`) ou dicionários não tipados.
  - Instantes temporais devem utilizar obrigatoriamente `DateTimeOffset` em UTC para serialização estrita em ISO 8601 (`YYYY-MM-DDTHH:mm:ssZ`).
  - Filtros de data sem componente de hora devem utilizar obrigatoriamente `DateOnly` no formato ISO 8601 (`YYYY-MM-DD`).
- **Sincronia Estrita com o Frontend**: Qualquer adição ou alteração nos contratos de `CxManager.Contract` deve ser refletida simetricamente nos tipos TypeScript do frontend (`src/frontend/src/types/`) e registrada no `JsonSerializerContext` da camada `CxManager.Api`.

## Ferramentas Globais

- **Plugin `dotnet-diag`**: subagente `optimizing-dotnet-performance`, skill `analyzing-dotnet-performance`
- **Plugin `dotnet`**: skills `csharp-expert`, `csharp-refactoring`
- **Plugin `ponytail`**: skill `ponytail`
