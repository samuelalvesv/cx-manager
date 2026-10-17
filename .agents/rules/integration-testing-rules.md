---
name: integration-testing-rules
description: Rules for Integration Tests, real SQLite persistence checks, seed verification values, soft delete invariants, and API endpoint contract validation.
applyTo: "src/backend/CxManager/CxManager.IntegrationTests/**/*.cs"
---

# Regras de Testes de Integração (CxManager.IntegrationTests)

## Diretrizes e Responsabilidades
- **Foco**: Testar a persistência e consultas SQL reais no SQLite (schema, índices, importação do `seed.json`, repositórios Dapper concretos, invariante `deleted_at IS NULL`, agregações analíticas e endpoints Minimal API REST).
- **Acesso Real ao Banco SQLite**: Os testes devem interagir com uma instância real do motor SQLite utilizando arquivo de banco temporário isolado por fixture (removido automaticamente no `DisposeAsync`), executando exatamente o mesmo DDL, importador de seed e queries SQL da aplicação.
- **Sem Mocks para Banco**: Proibido usar dublês ou substituir conexões e classes de repositório nos testes de integração. O objetivo é validar a execução real do SQL escrito à mão e dos endpoints HTTP.
- **Validação Obrigatória da Tabela de Conferência (Seed)**:
  - A suíte de integração deve conter testes explícitos validando os resultados das queries de resumo geral, série mensal, agrupamento por segmento e filtros de pesquisa contra os valores oficiais de conferência do `seed.json`, garantindo 100% de precisão na filtragem de `deleted_at IS NULL`, separação de escalas (`NPS` vs `CSAT`) e arredondamento.
- **Validação Completa do Ciclo de Soft Delete**:
  - Validar que um contato excluído logicamente desaparece imediatamente das listagens e buscas, retorna não encontrado nas consultas por identificador e libera o seu endereço de e-mail para um novo cadastro sem conflito de unicidade.
- **Isolamento de Estado**: Cada fixture deve inicializar seu próprio arquivo de banco SQLite temporário isolado para que mutações de escrita não interfiram nas asserções analíticas sobre o seed.
- **Convenção de Nomenclatura para Recursos Compartilhados**: Classes responsáveis pela inicialização do banco SQLite, carga do seed e `WebApplicationFactory` devem utilizar obrigatoriamente o sufixo `Fixture`.
- **Princípio YAGNI e Escopo Real**: Focar estritamente em integrações reais (queries SQL no SQLite, importação de dados, serialização JSON AOT e contratos HTTP ponta a ponta).

## Ferramentas Globais

- **Plugin `dotnet-experimental`**: skill `exp-test-maintainability`
- **Plugin `dotnet-aspnetcore`**: skill `dotnet-webapi`
- **Plugin `ponytail`**: skill `ponytail`
