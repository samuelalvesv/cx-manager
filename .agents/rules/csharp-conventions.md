---
name: csharp-conventions
description: Apply C# coding conventions, async/await guidelines, cancellation token propagation, memory optimizations, Native AOT compatibility, and static analysis.
applyTo: "src/backend/**/*.cs"
---

# Convenções de Código C#

> O arquivo `.editorconfig` na raiz do repositório é a fonte única de verdade para estilo de escrita de código, formatação, regras de chaves, espaçamento, nomenclatura e analisadores do Roslyn. Este documento especifica exclusivamente diretrizes arquiteturais, semânticas de design e decisões que o `.editorconfig` não é capaz de validar nativamente.

## Convenções Gerais

- Usar `sealed record` para DTOs e Value Objects imutáveis.
- Usar `sealed class` por padrão; remover `sealed` apenas quando a classe for explicitamente projetada como classe base abstrata (`abstract class`).
- Nunca instanciar dependências de serviços ou infraestrutura com `new`; sempre usar injeção de dependência.
- **Programação Assíncrona**: Usar obrigatoriamente `async`/`await` de ponta a ponta com `Task` e `Task<T>` para operações assíncronas que executam processamento local ou manipulação de recursos (`ValueTask<T>` é restrito à implementação de interfaces do próprio runtime que exijam essa assinatura). Métodos que realizam estritamente o repasse direto de chamadas (sem blocos de controle locais, capturas de exceção ou descarte de recursos no escopo) devem retornar a `Task` diretamente sem a máquina de estados `async`.
- **Propagação de CancellationToken**: Métodos assíncronos devem configurar o `CancellationToken` como último parâmetro opcional com valor padrão (`default`). É estritamente proibida a invocação manual de `ThrowIfCancellationRequested()` em camadas de orquestração (casos de uso, serviços e repositórios), delegando a interrupção exclusivamente às operações nativas da BCL e do driver de banco de dados através da propagação direta do token.
- **Tratamento de Exceções Nativo**: O handler de exceções globais na camada de API deve ser implementado utilizando a interface nativa `IExceptionHandler`.
- **Tipos Temporais Determinísticos (UTC)**: É obrigatório utilizar `DateTimeOffset` em formato UTC (`DateTimeOffset.UtcNow`) para representar instantes de data/hora e `DateOnly` para representar datas sem componente de hora. É proibido o uso de `DateTime` no código de domínio, aplicação e contratos.
- **Ferramentas e Bibliotecas**: Adotar exclusivamente as bibliotecas consolidadas e declaradas no `Directory.Packages.props`.
- **Desacoplamento e Reuso**: Centralizar o tratamento de erros de negócio e validação no `IExceptionHandler` global, proibindo blocos `try-catch` repetitivos nos endpoints.
- **Validação Estática em Tempo de Compilação**: Detectar erros em tempo de compilação utilizando tipos fortes, atributos de análise estática (`[NotNullWhen]`, `[MemberNotNull]`, `[StringSyntax]`), `const` e `readonly` para imutabilidade verificável pelo compilador, `<Nullable>enable</Nullable>` e `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- **Injeção de Dependências Mandatórias e Ausência de Opcionalidade**: Classes de serviço, persistência e infraestrutura gerenciadas pelo contêiner devem declarar suas dependências operacionais como estritamente obrigatórias no construtor primário. É proibida a utilização de tipos anuláveis ou valores padrão substitutivos para dependências essenciais, assegurando resolução integral pelo contêiner e *fail-fast* imediato.
- **Primary Constructors e Injeção de Dependência**: Utilizar os parâmetros de *primary constructor* diretamente no corpo dos métodos da classe, sem criar campos privados extras nem verificações defensivas de nulidade para dependências resolvidas pelo contêiner de injeção de dependência.
- **Sem Validações Defensivas Redundantes (Anti-Overengineering / YAGNI)**: Proibido adicionar verificações defensivas de nulidade quando essa garantia já for fornecida pelo pipeline do framework, pelo sistema de tipos ou por camadas precedentes. A validação de dados de negócio deve residir de forma única e centralizada nas camadas `Application` e `Domain`.
- **Argumentos Nomeados Obrigatórios**: É estritamente obrigatório utilizar argumentos nomeados em todas as invocações de construtores, métodos, funções e instanciações sempre que a sintaxe da linguagem permitir. É proibido o uso de argumentos posicionais quando houver a possibilidade de passá-los de forma nomeada, maximizando a clareza semântica e prevenindo erros de inversão de parâmetros.
- **Caminho Único, Ausência de Fallbacks e Falha Rápida (Fail-Fast)**: É expressamente proibido criar fallbacks condicionais silenciosos, valores padrão mascaradores ou múltiplos caminhos concorrentes de resolução para configurações e contratos do sistema. O fluxo de execução e resolução de dependências deve ser estritamente único, canônico e mandatório, falhando imediatamente de forma explícita (*fail-fast* com exceção clara) caso o recurso ou parâmetro obrigatório não seja fornecido.

## Diretrizes de Otimização, Source Generators e Native AOT

- **Source Generators (Geração de Código no Build)**: É proibido o uso de reflexão dinâmica em tempo de execução (`System.Reflection` e `System.Reflection.Emit`).
  - **Transporte e Serialização JSON**: Toda serialização e desserialização HTTP/JSON na API e na leitura de arquivos de carga deve utilizar obrigatoriamente **System.Text.Json Source Generation**, configurando classes parciais que estendem `JsonSerializerContext` com os atributos `[JsonSourceGenerationOptions]` e `[JsonSerializable]`.
  - **Acesso a Dados com Dapper AOT**: O mapeamento relacional com Dapper deve habilitar o Source Generator do **Dapper.AOT** (`[module: DapperAot]`), gerando comandos e leitores ADO.NET em tempo de compilação sem emissão dinâmica de IL em runtime.
  - **Mapeamento de Objetos**: A biblioteca oficial de mapeamento entre contratos e modelos de domínio/aplicação é o **Riok.Mapperly**, operando estritamente via Source Generator em tempo de compilação (`[Mapper]` em classe `public static partial`).
  - **Logging**: Emissão de logs estruturados deve utilizar o Source Generator nativo `[LoggerMessage]` em métodos parciais estáticos com `Microsoft.Extensions.Logging`, garantindo zero alocações na Heap e conformidade estrita com Native AOT.
  - **Expressões Regulares**: Para expressões regulares estáticas, utilizar obrigatoriamente o atributo `[GeneratedRegex]` em métodos parciais.
- **Estruturas de Dados e Passagem por Referência**:
  - Utilizar `readonly struct` para criar tipos de valor imutáveis que não necessitam de alocações na Heap.
  - Ao passar structs grandes como argumentos de método para evitar a cópia de seus dados na Stack, utilizar o modificador de parâmetro `in` (passagem por referência somente leitura).

## Ferramentas Globais

- **Plugin `dotnet-diag`**: subagente `optimizing-dotnet-performance`, skills `analyzing-dotnet-performance`, `microbenchmarking`, `dotnet-trace-collect`, `dump-collect`
- **Plugin `dotnet`**: skills `csharp-expert`, `csharp-refactoring`
- **Plugin `dotnet-msbuild`**: skill `including-generated-files`
- **Plugin `ponytail`**: skills `ponytail`, `ponytail-review`, `ponytail-audit`, `ponytail-debt`
