---
name: api-rules
description: Minimal APIs REST endpoints under /api, TypedResults, System.Text.Json Source Generation (AOT), global IExceptionHandler mapping, and DI composition.
applyTo: "src/backend/CxManager/CxManager.Api/**/*.cs"
---

# Regras do Ponto de Entrada API (CxManager.Api)

## Estrutura de Pastas
- `Endpoints/`: Mapeamento de grupos de rotas Minimal API organizados por recurso.
- `Serialization/`: Contexto de serialização AOT (`AppJsonSerializerContext : JsonSerializerContext`).
- `ExceptionHandling/`: Implementação de `IExceptionHandler` global para tradução de exceções em respostas padronizadas.
- `Extensions/`: Métodos de extensão para configuração do contêiner de injeção de dependência, JSON AOT, CORS e OpenAPI.
- `Program.cs`: Composição raiz e pipeline HTTP do projeto.

## Diretrizes de Minimal APIs REST e Native AOT
- **Endpoints Finos e Sem Lógica de Negócios**:
  - Os endpoints devem ser agrupados sob o prefixo `/api` utilizando `MapGroup("/api/...")`.
  - Recebem parâmetros de rota, query ou corpo, acionam diretamente o caso de uso da camada `CxManager.Application` via injeção de dependência (propagando o `CancellationToken` da requisição HTTP) e retornam o resultado usando `TypedResults`.
  - É proibido conter regras de negócio, validações manuais ad-hoc ou queries SQL dentro dos endpoints.
- **Fidelidade Estrita ao Contrato REST**:
  - Respeitar rigorosamente as rotas, verbos HTTP, query params, status codes (`200 OK`, `201 Created` com header `Location`, `204 NoContent`, `400 BadRequest`, `404 NotFound`, `409 Conflict`) e formatos de payload JSON especificados no contrato da API.
  - No endpoint de exportação CSV, retornar `text/csv; charset=utf-8` com escaping RFC 4180 para campos contendo vírgulas, quebras de linha ou aspas duplas.
- **Vinculação de Parâmetros de Rota e Consulta**: Parâmetros de rota (`/{id:long}`) e de query string (`?page=&pageSize=&search=`) devem ser vinculados diretamente ao DTO de Request da operação utilizando o atributo nativo `[AsParameters]`.
- **Validação Fail-Fast na Borda HTTP**: A validação sintática e de limites da requisição ocorre obrigatoriamente no pipeline de entrada das Minimal APIs via filtro de endpoint (`IEndpointFilter`), interrompendo requisições inválidas com status `400 BadRequest` antes da execução do caso de uso.
- **Compatibilidade Integral com Native AOT**:
  - Configurar `ConfigureHttpJsonOptions` registrando `AppJsonSerializerContext.Default` no `TypeInfoResolverChain`, com `PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase`.
  - Todos os DTOs de Request e Response expostos nos endpoints devem estar declarados com `[JsonSerializable(typeof(...))]` em `AppJsonSerializerContext`.
  - Utilizar assinaturas de retorno fortemente tipadas (`Task<Results<...>>`) em vez de `IResult` genérico, garantindo geração estática de metadados OpenAPI em tempo de compilação.
- **Tratamento Global de Exceções (`IExceptionHandler`)**:
  - Mapear exceções da camada de domínio e aplicação para códigos de status HTTP (`400`, `404`, `409`) e envelope `ErrorResponse` em português (`pt-BR`) exclusivamente através de correspondência de padrões (*pattern matching*) por tipo de exceção.
  - Exceções decorrentes de falhas na conversão ou desserialização de parâmetros de rota, cabeçalhos ou query strings fornecidos pelo cliente devem ser interceptadas pelo manipulador global de erros e traduzidas invariavelmente para status HTTP 400 (Bad Request). Em nenhuma hipótese erros de sintaxe ou inconsistências de formatação originados na entrada do cliente podem ser convertidos no status padrão HTTP 500 (Internal Server Error).
- **Injeção de Dependências**: Os métodos de extensão em `Extensions/` acionados pelo `Program.cs` compõem o contêiner de DI de forma explícita e livre de escaneamento reflexivo de assemblies.

## Ferramentas Globais

- **Plugin `dotnet-aspnetcore`**: skill `dotnet-webapi`
- **Plugin `dotnet-diag`**: subagente `optimizing-dotnet-performance`, skill `analyzing-dotnet-performance`
- **Plugin `dotnet-nuget`**: skill `convert-to-cpm`
- **Plugin `ponytail`**: skill `ponytail`
