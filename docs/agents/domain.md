# Documentação de Domínio

Como as skills de engenharia devem consultar e consumir a documentação de domínio deste repositório ao explorar a base de código.

## Antes de explorar, consulte

- **`GLOSSARY.md`** na raiz do repositório, ou
- **`GLOSSARY-MAP.md`** na raiz caso exista: aponta para um `GLOSSARY.md` por contexto. Leia cada glossário relevante ao escopo da tarefa.
- **`docs/adr/`**: consulte os Registros de Decisões Arquiteturais (ADRs) relacionados à área em que você irá atuar. Em repositórios multi-contexto, consulte também `src/<context>/docs/adr/` para decisões com escopo delimitado.

Se nenhum desses arquivos existir, **prossiga silenciosamente**. Não aponte sua ausência nem sugira criá-los antecipadamente. A skill `/domain-modeling` (acionada via `/grill-with-docs` e `/improve-codebase-architecture`) os cria sob demanda quando termos ou decisões forem efetivamente acordados.

## Estrutura de arquivos

Repositório de contexto único / single-context (padrão adotado):

```
/
├── GLOSSARY.md
├── docs/adr/
│   ├── 0001-event-sourced-orders.md
│   └── 0002-postgres-for-write-model.md
└── src/
```

Repositório multi-contexto (indicado pela presença de `GLOSSARY-MAP.md` na raiz):

```
/
├── GLOSSARY-MAP.md
├── docs/adr/                          ← decisões corporativas e sistêmicas
└── src/
    ├── ordering/
    │   ├── GLOSSARY.md
    │   └── docs/adr/                  ← decisões específicas do contexto
    └── billing/
        ├── GLOSSARY.md
        └── docs/adr/
```

## Utilize o vocabulário do glossário

Quando a sua resposta nomear um conceito de domínio (no título de uma issue, proposta de refatoração, hipótese ou nome de teste), utilize o termo conforme definido no `GLOSSARY.md`. Evite sinônimos que o glossário explicitamente desencorajar.

Se o conceito necessário ainda não constar no glossário, isso é um sinal: você pode estar introduzindo jargão não padronizado no projeto (reavalie) ou há uma lacuna real de documentação (registre para a skill `/domain-modeling`).

## Sinalize conflitos com ADRs

Se a sua proposta ou implementação entrar em conflito com uma ADR existente, exponha isso de forma explícita em vez de ignorar silenciosamente:

> _Entra em conflito com a ADR-0007 (pedidos com event-sourcing), porém justificável reabrir porque…_
