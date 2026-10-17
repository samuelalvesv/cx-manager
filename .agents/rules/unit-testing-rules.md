---
name: unit-testing-rules
description: Rules for Unit Tests, pure memory business logic validation, NPS/CSAT calculation precision, Fake test doubles, and TDD cycle (Red-Green-Refactor).
applyTo: "src/backend/CxManager/CxManager.UnitTests/**/*.cs"
---

# Regras de Testes de Unidade (CxManager.UnitTests)

## Diretrizes e Responsabilidades
- **Foco**: Testes unitários de regras de negócio (`CxManager.Domain`), cálculos e arredondamentos analíticos, validadores e casos de uso (`CxManager.Application`).
- **Isolamento Total**: Proibido acessar banco de dados físico, realizar chamadas de rede (HTTP) ou ler arquivos externos do disco. Tudo deve rodar estritamente na memória.
- **Convenção de Nomenclatura e Inicialização de Dublês**:
  - Dublês de teste manuais em memória devem ser implementados como classes concretas seladas utilizando obrigatoriamente o sufixo `Fake`.
  - Dublês manuais devem ser inicializados em estado funcional e válido por padrão, assegurando que o contexto represente uma execução legítima e prevenindo código defensivo artificial no código de produção.
- **TDD (Test-Driven Development)**: Aplicar TDD rigoroso para novas regras de domínio, fórmulas de arredondamento e fluxos de aplicação.
  - Ciclo: Escrever o teste que falha (Red) ➜ Escrever o código mínimo para passar (Green) ➜ Refatorar o código (Refactor).
- **Propagação de Cancelamento em Testes Assíncronos**: Operações assíncronas executadas dentro de métodos de teste devem propagar `TestContext.Current.CancellationToken` do xUnit v3.
- **Princípio YAGNI e Eficiência de Testes**: É estritamente proibido criar testes unitários para validar comportamentos, restrições ou garantias nativas da linguagem C#, do compilador, do sistema de tipos ou do contêiner de injeção de dependência. Todo teste deve validar exclusivamente comportamento observável de negócio, invariantes de domínio, precisão matemática de arredondamento, orquestração de casos de uso e cenários de borda com relevância funcional.
- **Escopo Exclusivo em Memória**: Toda validação unitária de casos de uso, lógica de domínio, validadores e mapeadores deve residir obrigatoriamente neste projeto.
- **Performance**: Todos os testes devem ser extremamente rápidos (rodando em poucos milissegundos).

## Ferramentas Globais

- **Plugin `dotnet-experimental`**: skills `exp-mock-usage-analysis`, `exp-test-maintainability`
- **Plugin `ponytail`**: skill `ponytail`
