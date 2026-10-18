# 0005. CRUD Completo de Todas as Entidades do Domínio

Para transformar a aplicação em uma plataforma de gestão de experiência do cliente completa e extensível, decidimos expandir a API além do escopo mínimo do desafio técnico (que previa CRUD apenas para contatos), implementando operações completas de criação, leitura, atualização e exclusão lógica para todas as entidades fundamentais do domínio:

1. **Empresas (`companies`)**:
   - Criação (`POST /api/companies`), consulta (`GET /api/companies`, `GET /api/companies/{id}`), edição (`PUT /api/companies/{id}`) e desativação/exclusão lógica (`DELETE /api/companies/{id}`).
2. **Pesquisas (`surveys`)**:
   - Criação de novas pesquisas (`POST /api/surveys`), com validação rigorosa de tipo (`NPS` na escala 0–10 ou `CSAT` na escala 1–5), consulta detalhada (`GET /api/surveys/{id}`) e desativação lógica (`DELETE /api/surveys/{id}`).
3. **Contatos (`contacts`)**:
   - CRUD completo já previsto com busca FTS5 trigram, paginação e controle de e-mail ativo.
4. **Respostas (`responses`)**:
   - Registro de novas respostas (`POST /api/responses`) validando notas na escala da respectiva pesquisa (0–10 para NPS, 1–5 para CSAT) e canais suportados, consulta unitária (`GET /api/responses/{id}`), edição de comentários (`PUT /api/responses/{id}`) e soft delete de resposta (`DELETE /api/responses/{id}`).

Qualquer mutação nas entidades que alimentam os cálculos de satisfação (`contacts`, `responses`, `surveys`) invalida reativamente o cache analítico da respectiva empresa.
