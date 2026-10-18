# 0004. Estratégia de Cache de Analytics com Invalidação Reativa

Para otimizar o tempo de resposta e a vazão dos endpoints analíticos (`/api/analytics/summary`, `nps-by-month` e `nps-by-segment`), decidimos implementar cache em memória escopado por empresa e parâmetros de filtro.

Como o soft delete ou a alteração de contatos impacta diretamente o universo de respostas válidas consideradas nas agregações, qualquer mutação no cadastro de contatos (`POST`, `PUT` ou `DELETE /api/contacts/{id}`) invalida imediatamente todas as entradas de cache analítico da respectiva empresa. Isso garante consistência imediata entre as operações de escrita e os números apresentados no painel de satisfação, sem retenção de dados defasados.
