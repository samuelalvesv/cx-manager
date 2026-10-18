# 0003. Arquitetura Multi-Empresa com Resolução Estrita de Contexto

Para suportar múltiplas empresas de forma agnóstica sem acoplamento a identificadores fixos no código de negócio e acesso a dados, decidimos isolar todas as tabelas principais (`contacts`, `surveys`) com uma coluna `company_id BIGINT NOT NULL REFERENCES companies(id)`.

A resolução da empresa ativa em cada requisição HTTP é gerenciada pelo serviço `ICompanyContext`, que inspeciona obrigatoriamente o cabeçalho `X-Company-Id` (ou parâmetro de consulta `companyId`). Decidimos **não adotar fallbacks, valores padrões ou escolha implícita de empresa**: o fornecimento da empresa é estritamente obrigatório. Requisições que omitirem a identificação da empresa são rejeitadas imediatamente na camada de entrada com código `400 BadRequest` e envelope `{ "error": "O identificador da empresa é obrigatório." }`.

Todas as consultas SQL, índices de unicidade, comandos e instâncias de cache operam estritamente vinculados ao `company_id` informado, garantindo isolamento total e determinístico entre organizações.
