# Rastreador de Issues: Markdown Local

Demandas (issues) e especificações (specs) para este repositório residem como arquivos markdown em `.scratch/`.

## Convenções

- Uma funcionalidade/esforço por diretório: `.scratch/<feature-slug>/`
- A especificação fica em `.scratch/<feature-slug>/spec.md`
- As demandas de implementação residem em um arquivo por ticket em `.scratch/<feature-slug>/issues/<NN>-<slug>.md`, numeradas a partir de `01`, nunca em um único arquivo consolidado
- O estado de triagem é registrado em uma linha `Status:` no topo de cada arquivo de demanda (consulte `triage-labels.md` para os nomes dos estados)
- Comentários e histórico de conversas são anexados ao final do arquivo sob o cabeçalho `## Comentários` (ou `## Comments`)

## Quando uma skill indicar "publicar no rastreador de issues"

Crie um novo arquivo sob `.scratch/<feature-slug>/` (criando o diretório caso não exista).

## Quando uma skill indicar "obter o ticket relevante"

Leia o arquivo no caminho referenciado. O usuário normalmente informará o caminho ou o número da demanda diretamente.

## Operações do Wayfinder

Utilizado por `/wayfinder`. O **mapa** é um arquivo principal com um arquivo **filho** por demanda/ticket.

- **Mapa**: `.scratch/<effort>/map.md` (contém as seções de Notas / Decisões tomadas / Áreas de incerteza).
- **Ticket filho**: `.scratch/<effort>/issues/NN-<slug>.md`, numerado a partir de `01`, com a pergunta/tarefa no corpo. Uma linha `Type:` registra o tipo do ticket (`research`/`prototype`/`grilling`/`task`); uma linha `Status:` registra o estado (`claimed`/`resolved`).
- **Bloqueios**: uma linha `Blocked by: NN, NN` no topo do arquivo. Uma demanda é considerada desbloqueada quando todos os arquivos listados estiverem como `resolved`.
- **Fronteira (Frontier)**: analisa `.scratch/<effort>/issues/` buscando arquivos abertos, desbloqueados e ainda não reivindicados; a ordem numérica define a prioridade.
- **Reivindicar (Claim)**: definir `Status: claimed` e salvar antes de iniciar qualquer trabalho.
- **Resolver (Resolve)**: anexar a resposta sob o cabeçalho `## Resposta` (ou `## Answer`), definir `Status: resolved`, e adicionar um apontamento com contexto (resumo + link) na seção de Decisões do `map.md`.
