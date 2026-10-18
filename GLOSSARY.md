# CxManager

Sistema de gestão de experiência do cliente (Customer Experience) para acompanhamento de pesquisas NPS e CSAT, gestão de contatos e análise agregada de satisfação.

## Language

**Contact**:
Aluno cadastrado da rede de academias que pode receber pesquisas e responder avaliações de satisfação.
_Avoid_: Aluno, Estudante, Cliente, Usuário, Account

**Survey**:
Instrumento de avaliação de satisfação com métrica e escala pré-definidas (NPS na escala 0–10 ou CSAT na escala 1–5).
_Avoid_: Questionário, Formulário, Avaliação, Teste

**Response**:
Registro individual de nota e comentário emitido por um contato para uma pesquisa em um canal específico.
_Avoid_: Answer, Feedback, Voto, Opinião

**Segment**:
Classificação comercial ou modalidade de plano atribuída ao contato na academia (ex.: "Plano Black", "Plano Fit", "Corporativo").
_Avoid_: Categoria, Plano, Grupo, Perfil

**Channel**:
Canal de comunicação utilizado para a coleta da resposta da pesquisa (ex.: "whatsapp", "email", "link").
_Avoid_: Meio, Origem, Plataforma, Via

**Company**:
Organização ou rede de unidades proprietária dos contatos e pesquisas de satisfação.
_Avoid_: Academia, Tenant, Estabelecimento

**Promoter**:
Classificação atribuída a respostas de pesquisas NPS com nota 9 ou 10.
_Avoid_: Fã, Satisfeito

**Neutral**:
Classificação atribuída a respostas de pesquisas NPS com nota 7 ou 8.
_Avoid_: Passivo, Indiferente, Médio

**Detractor**:
Classificação atribuída a respostas de pesquisas NPS com nota entre 0 e 6.
_Avoid_: Insatisfeito, Crítico

**Soft Delete**:
Marcação lógica de exclusão via preenchimento de `deleted_at`, ocultando o registro de consultas e analytics e liberando constraints de unicidade ativas.
_Avoid_: Exclusão física, Hard delete, Purge
