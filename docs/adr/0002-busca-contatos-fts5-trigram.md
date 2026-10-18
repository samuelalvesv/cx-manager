# 0002. Busca de Contatos com SQLite FTS5 e Tokenizer Trigram

Para atender ao requisito de busca parcial por nome e e-mail sem distinção de maiúsculas e acentos, evitando varreduras completas de tabela com `LIKE '%termo%'` (que no SQLite nativo não tratam caracteres Unicode acentuados sem extensões externas), decidimos adotar uma tabela virtual SQLite FTS5 (`contacts_fts`) configurada com o tokenizer `trigram`.

A sincronização de inserções, atualizações e exclusões entre `contacts` e `contacts_fts` ocorre de forma transparente e atômica diretamente no motor SQLite através de *triggers* (`AFTER INSERT`, `AFTER UPDATE`, `AFTER DELETE`). O filtro de exclusão lógica é garantido pela junção da tabela virtual com a tabela física: `INNER JOIN contacts c ON c.id = fts.rowid WHERE c.deleted_at IS NULL`.
