# 05: Histórico e Submissão de Respostas (Responses)

**O que construir:**
Submissão de avaliações de satisfação (`POST /api/responses` com validação de escala `0–10` para NPS e `1–5` para CSAT e canal suportado), consulta de histórico (`GET /api/contacts/{id}/responses` com JOIN de pesquisa, `GET /api/responses/{id}`), edição e exclusão de respostas (`PUT`, `DELETE`), e Drawer lateral de histórico de avaliações no frontend ao clicar em um contato.

**Bloqueado por:** 03: Gestão Completa de Pesquisas (Surveys), 04: Gestão de Contatos com Busca FTS5 Trigram e Soft Delete

**Status:** ready-for-agent

**Ferramentas recomendadas:**
- Skills: `propagate-feature`, `audit-api`
- MCPs: `mui-mcp`

**Critérios de aceitação:**
- [ ] `POST /api/responses` valida notas na escala exata da pesquisa (0–10 para NPS, 1–5 para CSAT; 400 se fora da escala) e canal (`whatsapp`, `email`, `link`).
- [ ] `GET /api/contacts/{id}/responses` retorna respostas ativas do contato ordenadas por `responded_at DESC`, trazendo `surveyName` e `surveyType` via JOIN.
- [ ] `GET /api/responses/{id}` detalha resposta individual ativa.
- [ ] `PUT /api/responses/{id}` e `DELETE /api/responses/{id}` permitem editar comentário/nota ou excluir logicamente a resposta.
- [ ] Frontend exibe gaveta lateral (Drawer) com a linha do tempo de respostas do contato selecionado, exibindo nota com badge colorida, canal, data e comentário, com botão para registrar nova avaliação.
