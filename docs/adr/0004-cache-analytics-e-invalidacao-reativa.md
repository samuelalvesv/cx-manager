# 0004. Estratégia de Cache de Analytics com Invalidação Reativa

Para otimizar o tempo de resposta e a vazão dos endpoints analíticos (`/api/analytics/summary`, `nps-by-month` e `nps-by-segment`), decidimos implementar cache em memória escopado por empresa e parâmetros de filtro.

Como o soft delete de contatos, a inserção ou exclusão de respostas e a alteração de pesquisas impactam diretamente o universo de dados considerado nas agregações SQL, qualquer mutação nessas entidades (`POST`, `PUT` ou `DELETE` em `/api/contacts`, `/api/responses` ou `/api/surveys`) invalida imediatamente todas as entradas de cache analítico da respectiva empresa. Isso assegura consistência imediata entre operações de escrita e os números visualizados no painel de satisfação, eliminando dados obsoletos.
