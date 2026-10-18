# Especificação: Plataforma de Experiência do Cliente (Mini CX)

## Status
ready-for-agent

## Declaração do Problema

O time de Customer Experience (CX) de redes de academias com múltiplas unidades (como a Vita Bem-Estar) necessita de um sistema integrado, confiável e de alta performance para gerenciar o cadastro de empresas parceiras, criar e administrar pesquisas de satisfação (NPS e CSAT), gerenciar contatos (os alunos), coletar e manter o histórico de respostas por múltiplos canais (WhatsApp, e-mail, link web) e acompanhar métricas consolidadas em tempo real.

Sistemas existentes frequentemente misturam escalas de avaliação incompatíveis, desrespeitam solicitações de privacidade e exclusão de dados (LGPD), apresentam lentidão ou falhas de correspondência em buscas textuais com nomes acentuados e realizam agregações pesadas na memória da aplicação, degradando a performance. Além disso, soluções sem suporte nativo a multi-empresa e sem capacidade de gerenciar o ciclo de vida completo de pesquisas e respostas tornam a operação engessada.

## Solução

Uma plataforma completa de Customer Experience desenvolvida com Clean Architecture, ASP.NET Core Minimal APIs preparadas para Native AOT, Dapper com SQL nativo escrito à mão no SQLite e uma interface moderna e responsiva em React 19 + TypeScript 7 + MUI Material v9.

A plataforma garante:
1. CRUD completo para todas as entidades fundamentais do domínio: Empresas (`companies`), Pesquisas (`surveys`), Contatos (`contacts`) e Respostas (`responses`).
2. Isolamento multi-empresa estrito via resolução obrigatória de contexto (`X-Company-Id`), com tolerância zero a fallbacks ou empresas padrão implícitas.
3. Busca de contatos de alta performance, insensível a maiúsculas e acentos, utilizando tabela virtual SQLite FTS5 com o tokenizer `trigram` sincronizada via triggers de banco.
4. Integridade rigorosa de dados onde contatos e respostas excluídos logicamente (`deleted_at IS NOT NULL`) são automaticamente omitidos de todas as operações e relatórios (`WHERE EXISTS` em resumos e `INNER JOIN` em agrupamentos segmentados).
5. Agregações analíticas calculadas integralmente no motor SQL, com segregação matemática estrita de escalas (NPS 0–10 vs CSAT 1–5) e arredondamento exato `AwayFromZero` da diferença crua das porcentagens.
6. Cache em memória para métricas analíticas com invalidação reativa imediata a qualquer mutação em contatos, pesquisas ou respostas.
7. Frontend intuitivo e acessível com tabela paginada de contatos, gestão de pesquisas, gaveta lateral de histórico de avaliações, painel de indicadores com filtros, exportação CSV e tratamento completo de estados visuais.

## Histórias de Usuário

1. Como especialista de CX, eu quero selecionar a rede de academias no cabeçalho de navegação, para que todos os contatos, pesquisas e métricas visualizados sejam estritamente isolados para a minha organização.
2. Como cliente da API, eu quero que requisições sem a identificação da empresa sejam rejeitadas com erro 400 Bad Request, para que dados nunca sejam atribuídos ou lidos da empresa errada por engano.
3. Como administrador, eu quero listar, cadastrar, editar e desativar empresas parceiras, para que a plataforma possa atender novas redes e filiais de forma independente.
4. Como especialista de CX, eu quero listar todas as pesquisas ativas da minha empresa, para que eu visualize quais questionários estão disponíveis para avaliação dos alunos.
5. Como especialista de CX, eu quero cadastrar uma nova pesquisa especificando seu nome e tipo (NPS na escala 0–10 ou CSAT na escala 1–5), para que eu possa avaliar novas etapas da jornada do aluno.
6. Como especialista de CX, eu quero atualizar o nome de uma pesquisa ou excluí-la logicamente, para que pesquisas descontinuadas não fiquem visíveis para novas respostas.
7. Como especialista de CX, eu quero listar contatos cadastrados em uma tabela paginada, para que eu possa navegar pela base de alunos de forma fluida sem sobrecarregar o navegador ou o banco de dados.
8. Como especialista de CX, eu quero buscar contatos por correspondência parcial de nome ou e-mail sem distinção de acentos ou maiúsculas, para que eu encontre qualquer aluno rapidamente mesmo ao digitar sem acentuação.
9. Como especialista de CX, eu quero consultar os detalhes de um contato pelo seu identificador, para que eu possa inspecionar seu nome, e-mail e plano atual.
10. Como especialista de CX, eu quero cadastrar um novo contato informando nome, e-mail e plano opcional, para que novos alunos matriculados possam receber pesquisas de satisfação.
11. Como especialista de CX, eu quero que o sistema rejeite novos contatos com formato de e-mail inválido ou campos obrigatórios ausentes, para que a base de clientes mantenha alta qualidade de dados.
12. Como especialista de CX, eu quero que o sistema rejeite cadastros de e-mails duplicados na mesma empresa com status 409 Conflict, para que não existam perfis ativos concorrentes para o mesmo aluno.
13. Como especialista de CX, eu quero atualizar os dados cadastrais de um contato existente, para que mudanças de nome, e-mail ou plano fiquem sempre atualizadas.
14. Como especialista de CX, eu quero que a edição de um contato mantendo o seu próprio e-mail ocorra com sucesso sem acusar duplicidade, para que alterações em outros campos não gerem falsos conflitos.
15. Como especialista de CX, eu quero realizar a exclusão lógica (soft delete) de um contato, para que seu perfil ativo seja ocultado das listagens preservando a integridade referencial histórica.
16. Como especialista de CX, eu quero que o e-mail de um contato excluído logicamente fique livre para novo cadastro, para que ex-alunos que retornem à academia possam ser cadastrados novamente sem erros.
17. Como especialista de CX, eu quero registrar uma nova resposta de pesquisa para um contato, informando a nota, canal e comentário opcional, para que novas avaliações sejam registradas no sistema.
18. Como especialista de CX, eu quero que o sistema valide a nota da resposta conforme o tipo da pesquisa (0–10 para NPS e 1–5 para CSAT), rejeitando valores fora da escala com erro 400 Bad Request.
19. Como especialista de CX, eu quero atualizar ou excluir logicamente uma resposta individual, para que correções cadastrais ou exclusões por LGPD sejam aplicadas pontualmente.
20. Como especialista de CX, eu quero visualizar o histórico cronológico de avaliações de um contato exibindo pesquisa, nota, canal e comentário, para que eu compreenda a evolução do sentimento do aluno ao longo do tempo.
21. Como analista de CX, eu quero visualizar um cartão de resumo exibindo o score NPS geral da rede, para que eu monitore o nível global de lealdade dos alunos.
22. Como analista de CX, eu quero que o score NPS seja calculado estritamente sobre pesquisas do tipo NPS (escala 0–10) a partir da diferença das porcentagens cruas com arredondamento AwayFromZero, para que a métrica reportada seja matematicamente exata.
23. Como analista de CX, eu quero visualizar a distribuição de promotores (9–10), neutros (7–8) e detratores (0–6) com contagens e percentuais formatados em uma casa decimal, para que eu avalie a composição da satisfação.
24. Como analista de CX, eu quero visualizar o volume total de respostas válidas de todas as pesquisas, para que eu conheça a amostragem total de feedback recebido.
25. Como analista de CX, eu quero visualizar a média aritmética CSAT calculada estritamente sobre pesquisas do tipo CSAT (escala 1–5), para que a experiência de onboarding seja avaliada isoladamente sem distorcer o NPS.
26. Como analista de CX, eu quero que a média CSAT retorne nula quando não houver respostas CSAT no período filtrado, para que a ausência de dados não seja confundida com nota zero.
27. Como analista de CX, eu quero que respostas de contatos excluídos logicamente sejam omitidas de todas as métricas analíticas, para que solicitações de privacidade (LGPD) sejam respeitadas nos relatórios.
28. Como analista de CX, eu quero filtrar os indicadores de satisfação por intervalo de datas (inclusivo) e por pesquisa específica, para que eu analise o impacto de campanhas ou reformas pontuais.
29. Como analista de CX, eu quero visualizar a série temporal de NPS agrupada por mês em ordem cronológica, para que eu acompanhe a evolução mês a mês da satisfação dos alunos.
30. Como analista de CX, eu quero que meses sem nenhuma resposta NPS válida sejam omitidos do gráfico e da série mensal, para que períodos vazios não gerem divisão por zero ou distorções visuais.
31. Como analista de CX, eu quero visualizar o ranking de NPS por plano/segmento do contato (Plano Black, Plano Fit, Corporativo ou Sem segmento), para que eu identifique quais modalidades demandam melhorias operacionais.
32. Como analista de CX, eu quero exportar as respostas filtradas em arquivo CSV compatível com RFC 4180, para que eu realize auditorias externas com tratamento correto de vírgulas, aspas e acentuação.
33. Como usuário, eu quero visualizar esqueletos visuais (skeletons) durante o carregamento de dados na tela, para que a percepção de performance seja imediata e sem saltos bruscos de layout.
34. Como usuário, eu quero visualizar alertas amigáveis com botão de tentar novamente em caso de falha de conexão com a API, para que eu resolva instabilidades temporárias sem recarregar a página inteira.
35. Como usuário, eu quero visualizar mensagens contextuais de estado vazio quando uma busca ou filtro não encontrar resultados, para que eu entenda o motivo da tela limpa e ajuste meus critérios.
36. Como desenvolvedor, eu quero que a base de dados SQLite seja provisionada e carregada automaticamente a partir do seed inicial da Vita Bem-Estar em uma transação única na subida da API, para que o sistema fique funcional de forma reproduzível.
37. Como desenvolvedor, eu quero que qualquer mutação em contatos, pesquisas ou respostas limpe imediatamente o cache analítico da empresa, para que o painel de satisfação reflita os novos números sem leituras defasadas.

## Decisões de Implementação

### Costuras e Fronteiras de Arquitetura
- **Costura Superior de Testes**: Camada de endpoints Minimal APIs executada via `WebApplicationFactory` sobre instância SQLite real, cobrindo o fluxo completo da requisição até a persistência.
- **Costura de Domínio**: Modelos puros e funções de cálculo em C# testados isoladamente na memória para garantir regras de validação de escalas, arredondamento e classificação de notas.
- **Costura Estrutural**: Testes automatizados de arquitetura com `NetArchTest` assegurando que referências e dependências apontem exclusivamente para dentro da Clean Architecture.

### CRUD Completo de Entidades (ADR-0005)
- Operações de leitura, criação, edição e exclusão lógica para Empresas (`/api/companies`), Pesquisas (`/api/surveys`), Contatos (`/api/contacts`) e Respostas (`/api/responses`).
- Validação de escalas no cadastro de respostas: `0–10` para pesquisas NPS e `1–5` para pesquisas CSAT.

### Isolamento Multi-Empresa Obrigatório (ADR-0003)
- Todas as tabelas operacionais (`contacts`, `surveys`) contêm `company_id INTEGER NOT NULL REFERENCES companies(id)`.
- A identificação da empresa é resolvida obrigatoriamente a partir do cabeçalho `X-Company-Id` (ou parâmetro de consulta `companyId`).
- A ausência do identificador de empresa resulta em rejeição na entrada da API com status `400 BadRequest` e mensagem `{ "error": "O identificador da empresa é obrigatório." }`, sem adoção de valores padrão ou empresas implícitas.
- O endpoint público `GET /api/companies` permite a listagem das empresas disponíveis para seleção no frontend.

### Busca Textual e Tabela Virtual FTS5 (ADR-0002)
- Busca de contatos realizada através da tabela virtual `contacts_fts` no SQLite utilizando o tokenizer `trigram`.
- Sincronização atômica e transparente implementada via triggers SQLite de banco (`AFTER INSERT`, `AFTER UPDATE` e `AFTER DELETE` na tabela `contacts`).
- Filtragem de contatos ativos garantida através da junção da tabela virtual com a física (`c.deleted_at IS NULL AND c.company_id = @CompanyId`).

### Estratégia de Soft Delete e Agregações de Analytics (ADR-0001)
- Registros de contatos, pesquisas e respostas com `deleted_at IS NOT NULL` são omitidos de todas as operações de leitura.
- Para o resumo geral (`GET /api/analytics/summary`) e evolução mensal (`GET /api/analytics/nps-by-month`), a filtragem de contatos ativos utiliza `WHERE r.deleted_at IS NULL AND EXISTS (SELECT 1 FROM contacts c WHERE c.id = r.contact_id AND c.company_id = @CompanyId AND c.deleted_at IS NULL) AND EXISTS (SELECT 1 FROM surveys s WHERE s.id = r.survey_id AND s.deleted_at IS NULL)`.
- Para o agrupamento por segmento (`GET /api/analytics/nps-by-segment`) e exportação CSV (`GET /api/responses/export`), utiliza-se `INNER JOIN contacts c` devido à necessidade de projetar `c.segment` e `c.name`.

### Cache em Memória com Invalidação Reativa (ADR-0004)
- Resultados analíticos de resumo e séries temporais são mantidos em cache na memória particionados por empresa e filtros aplicados.
- Qualquer operação de escrita em contatos (`/api/contacts`), pesquisas (`/api/surveys`) ou respostas (`/api/responses`) invalida imediatamente as entradas de cache analítico da respectiva empresa.

### Padronização de Contratos e Erros
- Respostas de falha de validação ou regra de negócio utilizam o envelope `{ "error": "mensagem legível em pt-BR" }` para status `400`, `404` e `409`.
- Cadastros bem-sucedidos retornam status `201 Created` e cabeçalho HTTP `Location` apontando para a rota do recurso.
- Propriedades JSON serializadas estritamente em `camelCase` e instantes temporais em formato ISO 8601 UTC.

### Arquitetura Frontend
- Aplicação Single Page em React 19, TypeScript 7 em modo estrito e MUI Material v9.
- Tokens semânticos centralizados em `src/theme/`, serviços HTTP tipados em `src/services/` com cancelamento por `AbortSignal`.
- Navegação organizada no AppBar: Painel de Satisfação, Gestão de Contatos e Gestão de Pesquisas.
- Seletor dinâmico de empresa no cabeçalho superior propagando o cabeçalho `X-Company-Id` para todas as requisições.

## Decisões de Teste

- **Princípio do Teste Eficaz**: Testar comportamentos externos observáveis pelo cliente da API (rotas, status codes, corpos JSON, filtros e persistência no banco), evitando mocks repetitivos de métodos de repositório.
- **Suíte 1: Testes de Conferência Analítica (`CxManager.IntegrationTests`)**:
  - Validação estrita de 100% dos números da tabela de conferência contra o SQLite populado com o `seed.json` original.
- **Suíte 2: Ciclo de Vida do Contato e Soft Delete (`CxManager.IntegrationTests`)**:
  - Cadastro, bloqueio de e-mail duplicado, soft delete, liberação de e-mail e exclusão do resumo analítico.
- **Suíte 3: Cálculos de Domínio NPS e Escalas (`CxManager.UnitTests`)**:
  - Validação de regras de escala (NPS 0–10 vs CSAT 1–5), score cru e arredondamento `AwayFromZero`.
- **Suíte 4: Isolamento Arquitetural (`CxManager.ArchitectureTests`)**:
  - Regras com `NetArchTest` garantindo que Domain e Contract não possuam dependências externas, que Infrastructure não conheça API nem Contract, e que Application dependa exclusivamente de Domain e Contract.

## Fora de Escopo

- Autenticação de usuários, credenciais de login ou geração de tokens JWT (o isolamento multi-empresa é operado via cabeçalho de contexto).
- Exclusão física (hard delete) de linhas de contatos, pesquisas ou respostas do banco.
- Bancos de dados separados fisicamente por empresa (o isolamento é garantido a nível de linha relacional com `company_id`).

## Notas Adicionais

- A aplicação disponibiliza dois modos de inicialização documentados no README:
  1. Modo Direto: `dotnet run --project CxManager.Api` + `npm run dev`.
  2. Modo Orquestrado: `dotnet run --project CxManager.AppHost` via .NET Aspire.
- A importação do seed inicial é totalmente autossuficiente e idempotente, executando em transação única no startup da API caso a base esteja vazia.
