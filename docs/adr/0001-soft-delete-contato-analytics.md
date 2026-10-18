# 0001. Propagação de Soft Delete de Contatos para Analytics (EXISTS vs INNER JOIN)

Em conformidade com cenários de privacidade (LGPD), quando um contato é excluído logicamente (`deleted_at IS NOT NULL`), todas as suas respostas devem deixar de compor os cálculos e resumos de satisfação (NPS e CSAT). 

Para otimizar a performance de execução no SQLite e evitar custos desnecessários de junção e projeção de colunas, avaliamos e definimos a estratégia ideal por consulta:

1. **Uso de `WHERE EXISTS (SELECT 1 ...)` (Semi-join de alta performance)**:
   - **Aplicável a**: `GET /api/analytics/summary` e `GET /api/analytics/nps-by-month`.
   - **Motivo**: Essas consultas analíticas agregam métricas calculadas exclusivamente a partir de dados de `responses` e `surveys` (notas, tipos e datas). Como nenhuma coluna da tabela `contacts` precisa ser projetada ou agrupada, o `EXISTS` permite ao otimizador do SQLite realizar sondagem direta (*index lookup*) por chave primária, filtrando contatos inativos sem alocar buffers de tuplas de junção nem arriscar duplicações de cardinalidade.
   - **Padrão SQL**:
     ```sql
     WHERE r.deleted_at IS NULL
       AND EXISTS (
           SELECT 1 FROM contacts c 
           WHERE c.id = r.contact_id 
             AND c.company_id = @CompanyId 
             AND c.deleted_at IS NULL
       )
     ```

2. **Uso de `INNER JOIN` (Projeção e Agrupamento necessários)**:
   - **Aplicável a**: `GET /api/analytics/nps-by-segment` e `GET /api/responses/export`.
   - **Motivo**: Nestas consultas, colunas de `contacts` são indispensáveis no resultado final — `c.segment` para a cláusula `GROUP BY` no agrupamento por segmento, e `c.name` para a coluna de contato no arquivo CSV. O `INNER JOIN` com filtro `c.deleted_at IS NULL` é o caminho direto e idiomático.
