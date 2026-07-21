# Registro de execução de Q.A.

Copiar a seção “Rodada” para cada ciclo de teste. Não registrar segredos, senhas, JWTs, refresh tokens, PINs ou certificados.

## Rodada QA-AAAA-MM-DD-NN

### Identificação

| Campo | Valor |
|---|---|
| Data e horário | |
| Executor | |
| Ambiente | Local / Homologação |
| Commit testado | |
| Backend | |
| Frontend | |
| Banco/migration atual | |
| Navegador/SO | |
| Configurações especiais | |

### Estado dos serviços

| Componente | Estado/versão | Observação |
|---|---|---|
| API | | |
| Angular | | |
| PostgreSQL | | |
| Storage | | |
| Hangfire/worker | | |
| SMTP de teste | | |
| Receptor de webhook | | |
| Active Directory | | |

### Resultados

Use `Aprovado`, `Falhou`, `Bloqueado` ou `Não executado`.

| Caso | Resultado | Perfil | Evidência | Defeito | Observação |
|---|---|---|---|---|---|
| EXEMPLO-001 | Aprovado | USR-A | `evidencias/QA-AAAA-MM-DD-NN/EXEMPLO-001/` | — | |

### Defeito

#### BUG-AAAA-NNN — Título curto

| Campo | Valor |
|---|---|
| Caso relacionado | |
| Severidade | Crítica / Alta / Média / Baixa |
| Ambiente e commit | |
| Perfil | |
| Pré-condição | |
| Resultado atual | |
| Resultado esperado | |
| Frequência | Sempre / Intermitente |
| Evidência sanitizada | |

Passos para reproduzir:

1. 
2. 
3. 

Impacto para o usuário:

Mitigação temporária, se houver:

### Resumo da rodada

| Prioridade | Aprovados | Falharam | Bloqueados | Não executados |
|---|---:|---:|---:|---:|
| P0 | 0 | 0 | 0 | 0 |
| P1 | 0 | 0 | 0 | 0 |
| P2 | 0 | 0 | 0 | 0 |

Decisão: **Aprovado / Reprovado / Inconclusivo**

Riscos residuais e próximos passos:

- 

---

## Rodada QA-2026-07-20-01

### Identificação

| Campo | Valor |
|---|---|
| Data | 2026-07-20 |
| Executor | Usuário do projeto |
| Ambiente | Não informado |
| Commit testado | Não informado |
| Escopo | Proteção das rotas internas no frontend |

### Resultados

| Caso | Resultado | Perfil | Evidência | Defeito | Observação |
|---|---|---|---|---|---|
| AUTH-016 | Aprovado | ANON | Validação manual informada pelo executor | — | Tentativas de acessar dashboard, documentos e assinaturas redirecionaram para a tela de login. |
| AUTH-001 | Não executado | ANON | — | — | O redirecionamento do frontend não valida a autorização dos endpoints. Pendente chamada direta à API sem JWT. |

### Resumo da rodada

| Prioridade | Aprovados | Falharam | Bloqueados | Não executados |
|---|---:|---:|---:|---:|
| P0 | 0 | 0 | 0 | 1 |
| P1 | 1 | 0 | 0 | 0 |
| P2 | 0 | 0 | 0 | 0 |

Decisão: **Inconclusivo para autorização do backend; aprovado para proteção de rotas do frontend.**

Próximo passo: chamar diretamente um endpoint protegido, como `GET /api/v1/documents`, sem cabeçalho `Authorization` e confirmar resposta `401` sem conteúdo privado.

---

## Rodada QA-2026-07-20-02

### Identificação

| Campo | Valor |
|---|---|
| Data | 2026-07-20 |
| Executor | Usuário do projeto; resultado reproduzido e consolidado pelo Codex |
| Ambiente | Local — `https://localhost:5001` |
| Commit testado | Não informado; processo local em execução |
| Escopo | AUTH-001, requisições GET diretas sem JWT |
| Ambiente de execução | Python 3, biblioteca padrão, sem HTTPie |
| Executor técnico | `docs/RouteFiles/auth-001.py` |

### Resultados

| Caso | Método | Status HTTP | Resultado | Rota |
|---|---|---:|---|---|
| AUTH-001-A01 | GET | 401 | Aprovado | `/api/v1/auth/notification-capabilities` |
| AUTH-001-A02 | GET | 401 | Aprovado | `/api/v1/auth/me` |
| AUTH-001-A05 | GET | 401 | Aprovado | `/api/v1/dashboard/stats` |
| AUTH-001-A06 | GET | 401 | Aprovado | `/api/v1/dashboard/recent-documents?count=5` |
| AUTH-001-A07 | GET | 401 | Aprovado | `/api/v1/dashboard/notifications?unreadOnly=false` |
| AUTH-001-A10 | GET | 401 | Aprovado | `/api/v1/documents` |
| AUTH-001-A11 | GET | 401 | Aprovado | `/api/v1/documents/by-status/Draft` |
| AUTH-001-A12 | GET | 401 | Aprovado | `/api/v1/documents/00000000-0000-0000-0000-000000000000` |
| AUTH-001-A13 | GET | 401 | Aprovado | `/api/v1/documents/00000000-0000-0000-0000-000000000000/download?version=1` |
| AUTH-001-A17 | GET | 401 | Aprovado | `/api/v1/external-services` |
| AUTH-001-A22 | GET | 401 | Aprovado | `/api/v1/users` |
| AUTH-001-B01 | GET | 200 | Falhou | `/api/v1/auditlogs/document/00000000-0000-0000-0000-000000000000` |
| AUTH-001-B02 | GET | 200 | Falhou | `/api/v1/auditlogs/date-range?startDate=2026-01-01T00:00:00Z&endDate=2026-01-02T00:00:00Z` |
| AUTH-001-B03 | GET | 404 | Falhou | `/api/v1/signatureflow/00000000-0000-0000-0000-000000000000` |
| AUTH-001-B04 | GET | 200 | Falhou | `/api/v1/signatureflow/document/00000000-0000-0000-0000-000000000000` |
| AUTH-001-B06 | GET | 400 | Falhou | `/api/v1/signers/pending/qa-auth-001-nao-existe@example.invalid` |
| AUTH-001-B07 | GET | 404 | Falhou | `/api/v1/signers/00000000-0000-0000-0000-000000000000` |

Critério aplicado: somente `401 Unauthorized` aprova o caso. Respostas `200`, `400` e `404` demonstram que a requisição sem JWT ultrapassou o middleware de autenticação e alcançou a rota/handler.

### Resumo da rodada

| Prioridade | Aprovados | Falharam | Bloqueados | Não executados |
|---|---:|---:|---:|---:|
| P0 | 11 | 6 | 0 | 3 mutações críticas |
| P1 | 0 | 0 | 0 | 0 |
| P2 | 0 | 0 | 0 | 0 |

Decisão: **Reprovado.** O frontend redireciona corretamente, mas há endpoints do backend acessíveis sem autenticação.

Riscos confirmados ou parcialmente confirmados: `RSK-001`, `RSK-002` e `RSK-003`.

Próximos passos:

1. Tratar as seis rotas de leitura como defeitos de autorização.
2. Decidir se as mutações `AUTH-001-B05`, `AUTH-001-B08` e `AUTH-001-B09` serão testadas antes da correção, exclusivamente em ambiente descartável.
3. Após a correção, repetir as 17 leituras e então o conjunto completo de 34 requisições.

---

## Rodada QA-2026-07-21-01

### Identificação

| Campo | Valor |
|---|---|
| Data | 2026-07-21 |
| Executor | Codex (assistente) |
| Ambiente | Local — `https://localhost:5001` |
| Commit testado | `70e34bc` |
| Escopo | AUTH-001 completo (34 requisições, incluindo mutações anônimas B05/B08/B09) |
| Executor técnico | `docs/RouteFiles/auth-001.py --include-mutations` |

### Defeito de ferramenta corrigido antes da execução

`docs/RouteFiles/api-routes.http` usava `###` (o mesmo prefixo do separador de bloco) em um comentário de duas linhas dentro do caso `AUTH-001-A14`. O parser do executor trata qualquer linha iniciada por `###` como um novo bloco, então a requisição `POST /api/v1/documents` ficava sem o ID do caso associado e era descartada silenciosamente — `AUTH-001-A14` nunca era executado, mesmo com `--include-mutations`. Corrigido trocando a segunda linha para `#` (comentário simples, não separador). Nenhuma outra ocorrência do padrão foi encontrada no arquivo.

### Resultados

| Caso | Método | Status HTTP | Resultado | Rota |
|---|---|---:|---|---|
| AUTH-001-A01 a A25 | GET/POST/PUT/DELETE | 401 | Aprovado (25/25) | Rotas com `[Authorize]` confirmado, incluindo `A14` (criar documento) após a correção do parser |
| AUTH-001-B01 | GET | 200 | Falhou | `/api/v1/auditlogs/document/{id}` |
| AUTH-001-B02 | GET | 200 | Falhou | `/api/v1/auditlogs/date-range` |
| AUTH-001-B03 | GET | 404 | Falhou | `/api/v1/signatureflow/{id}` |
| AUTH-001-B04 | GET | 200 | Falhou | `/api/v1/signatureflow/document/{id}` |
| AUTH-001-B05 | POST | 400 | Falhou | `/api/v1/signatureflow` — corpo: `{"errors":["Document ID is required"]}` |
| AUTH-001-B06 | GET | 400 | Falhou | `/api/v1/signers/pending/{email}` |
| AUTH-001-B07 | GET | 404 | Falhou | `/api/v1/signers/{id}` |
| AUTH-001-B08 | POST | 400 | Falhou | `/api/v1/signers/sign` — corpo: `{"errors":["Signer ID is required"]}` |
| AUTH-001-B09 | POST | 404 | Falhou | `/api/v1/signers/reject` — corpo: `{"message":"NOT_FOUND","errors":["Entity \"signer\" (00000000-0000-0000-0000-000000000000) was not found."]}` |

Critério aplicado: somente `401 Unauthorized` aprova o caso.

Achado novo em relação à rodada anterior: as três mutações pendentes (`B05`, `B08`, `B09`) foram executadas com GUID nulo em ambiente local descartável. Nenhuma alcançou 401 — todas passaram pela ausência de `[Authorize]` e chegaram à validação de negócio ou à consulta ao banco. `AUTH-001-B09` é a evidência mais forte: o corpo confirma que o handler de rejeição consultou o repositório de signatários (`NOT_FOUND` do agregado) sem qualquer verificação de identidade. Isso eleva a confiança de `RSK-002` e `RSK-003` de "parcialmente confirmado" para "confirmado" quanto à ausência de `[Authorize]`; a exploração completa (efeito real sobre um documento/signatário existente) continua pendente e exige massa de dados `QA-` e usuários de teste antes de ser executada.

### Resumo da rodada

| Prioridade | Aprovados | Falharam | Bloqueados | Não executados |
|---|---:|---:|---:|---:|
| P0 | 25 | 9 | 0 | 0 |
| P1 | 0 | 0 | 0 | 0 |
| P2 | 0 | 0 | 0 | 0 |

Decisão: **Reprovado.** AUTH-001 concluído (34/34 requisições executadas). Nove rotas seguem acessíveis sem autenticação, agora incluindo confirmação de que as três mutações críticas também alcançam o handler sem JWT.

Riscos confirmados: `RSK-001` (confirmado), `RSK-002` (confirmado quanto à ausência de `[Authorize]`), `RSK-003` (confirmado quanto à ausência de `[Authorize]`).

Próximos passos:

1. Priorizar a correção de `[Authorize]` em `AuditLogsController`, `SignatureFlowController` e `SignersController` antes de qualquer outra rodada com dados reais.
2. Para confirmar o impacto completo de `RSK-002`/`RSK-003` (criação de fluxo e assinatura/rejeição efetivas em documento alheio), criar massa de dados `QA-2026-07-21-` com usuários `USR-A`/`USR-B` reais — não é seguro tentar isso contra IDs reais antes da correção.
3. Após a correção, repetir o conjunto completo de 34 requisições e avançar para os casos `ACL-*` e `DOC-*` com contas de teste autenticadas.

---

## Rodada QA-2026-07-21-02

### Identificação

| Campo | Valor |
|---|---|
| Data | 2026-07-21 |
| Executor | Codex (assistente) |
| Ambiente | Local — `https://localhost:5001` |
| Commit testado | `70e34bc` |
| Escopo | ACL-001, ACL-002, ACL-004, ACL-008, ACL-009, ACL-010, DOC-001, DOC-002, DOC-009, DOC-012, DOC-013 |
| Massa de dados | 3 contas descartáveis via `POST /auth/register`: `qa-20260721-usr-a@example.invalid` (depto `QA-Dept-A`), `qa-20260721-usr-a2@example.invalid` (depto `QA-Dept-A`), `qa-20260721-usr-b@example.invalid` (depto `QA-Dept-B`). Senha local gerada para a rodada, não reaproveitada de nenhuma conta real. Documentos de teste com prefixo `QA-20260721-`, todos removidos ao final da rodada. |
| Executor técnico | Scripts Python ad hoc no scratchpad da sessão (não versionados no repositório) |

### Resultados

| Caso | Resultado | Perfil | Evidência | Defeito | Observação |
|---|---|---|---|---|---|
| DOC-001 | Aprovado | USR-A | `201`, documento criado como rascunho | — | |
| DOC-002 | **Falhou** | USR-A | `201`, `createdByUserId` retornado = ID de `USR-B` | `BUG-2026-001` | Corpo enviado por `USR-A` definiu `CreatedByUserId` = ID de `USR-B`; o servidor aceitou e usou esse valor, não a identidade do JWT. |
| ACL-008 | Aprovado | USR-B | `200`, documento de `USR-A` (depto A) não apareceu na listagem de `USR-B` (depto B) | — | |
| ACL-009 | Aprovado | USR-A2 | `200` ao acessar documento não confidencial do mesmo departamento | — | |
| ACL-010 | Aprovado | USR-A2 | `404` ao acessar documento confidencial do mesmo departamento sem participação | — | |
| DOC-009 (leitura) | Aprovado | USR-B | `404` ao acessar por GUID documento de outro departamento | — | |
| DOC-009 (download) | Aprovado | USR-B | `404`, nenhum byte de conteúdo retornado | — | |
| DOC-012 | Aprovado | USR-A | `204` ao excluir o próprio rascunho | — | |
| DOC-013 | Aprovado | USR-A2 | `403` ao tentar excluir rascunho de `USR-A` no mesmo departamento | — | |
| ACL-001 | Aprovado | USR-A | `403` ao listar usuários (role `User`) | — | |
| ACL-002 | Aprovado | USR-A | `403` ao tentar se autoatribuir a role `Admin` | — | |
| ACL-004 | Aprovado | USR-A | `403` ao tentar excluir a conta de `USR-B` | — | |

### Defeito

#### BUG-2026-001 — `CreatedByUserId` do corpo da requisição é aceito como identidade do criador

| Campo | Valor |
|---|---|
| Caso relacionado | DOC-002 (confirma `RSK-004`) |
| Severidade | Alta |
| Ambiente e commit | Local, `70e34bc` |
| Perfil | USR-A autenticado |
| Pré-condição | Usuário autenticado com JWT válido |
| Resultado atual | O documento é criado com `CreatedByUserId`, `OwningDepartment` e auditoria atribuídos ao usuário informado no campo `documentData.CreatedByUserId` do formulário, não ao usuário autenticado. |
| Resultado esperado | O servidor deve derivar o criador exclusivamente da claim de identidade do JWT e ignorar/rejeitar qualquer `CreatedByUserId` enviado pelo cliente. |
| Frequência | Sempre |
| Evidência sanitizada | Requisição `POST /api/v1/documents` (multipart) por `USR-A`, campo `documentData.CreatedByUserId` = GUID de `USR-B`; resposta `201` com `createdByUserId` = GUID de `USR-B`. |

Passos para reproduzir:

1. Autenticar como `USR-A` e obter um JWT válido.
2. Enviar `POST /api/v1/documents` (multipart/form-data) com um PDF válido e `documentData.CreatedByUserId` = ID de outro usuário (`USR-B`).
3. Observar que a resposta `201` traz `createdByUserId` = `USR-B`, e que o documento passa a pertencer ao departamento e à conta de `USR-B`.

Impacto para o usuário: um usuário autenticado pode criar documentos atribuídos a outra pessoa — falsificando autoria na auditoria, movendo o documento para o departamento de outra conta e transferindo o controle de exclusão/gestão para um terceiro sem o conhecimento dele. Confirmado experimentalmente: ao tentar limpar a massa de teste, `USR-A` (quem de fato enviou o arquivo) recebeu `404` ao tentar excluir o documento, enquanto `USR-B` (identidade forjada) conseguiu excluí-lo — ou seja, `USR-A` perde completamente o controle sobre um documento que ele mesmo enviou.

Mitigação temporária: nenhuma no cliente; requer correção no `CreateDocumentCommandHandler`/`DocumentsController` para descartar `CreatedByUserId` do corpo e usar `User.FindFirstValue(ClaimTypes.NameIdentifier)`.

### Resumo da rodada

| Prioridade | Aprovados | Falharam | Bloqueados | Não executados |
|---|---:|---:|---:|---:|
| P0 | 11 | 1 | 0 | 0 |
| P1 | 0 | 0 | 0 | 0 |
| P2 | 0 | 0 | 0 | 0 |

Decisão: **Reprovado.** O isolamento por departamento, confidencialidade e papel (ACL-001/002/004/008/009/010, DOC-009/012/013) funciona corretamente nos casos testados. `DOC-002` confirma `RSK-004` como defeito real e explorável, não apenas um risco estático.

Riscos confirmados: `RSK-004` (confirmado; eleva a severidade preliminar de "Alta" para confirmada com impacto de perda de controle sobre o próprio documento).

Riscos residuais e próximos passos:

- Corrigir `BUG-2026-001` (`RSK-004`) antes de repetir esta rodada com dados de produção.
- Ainda não testado nesta rodada: `ACL-003`, `ACL-005`, `ACL-006`, `ACL-007`, `ACL-011`, `ACL-012` (troca de IDs de notificação/integração), `DOC-003` a `DOC-020`, `SIGN-*`, `AUD-*`, `NOTIF-*`, `EXT-*`, `PUB-*`.
- Continuar priorizando a correção das rotas sem `[Authorize]` identificadas em `QA-2026-07-21-01` antes de avançar para `SIGN-*` com dados reais, já que essa área ainda está aberta.
- Massa de teste não totalmente removida: os documentos foram excluídos, mas as 3 contas `qa-20260721-usr-*@example.invalid` continuam ativas no banco local — `DELETE /api/v1/users/{id}` exige role `Admin`, que não estava disponível nesta rodada. Remover essas contas com uma conta administrativa antes de considerar o ambiente limpo.

---

## Rodada QA-2026-07-21-03

### Identificação

| Campo | Valor |
|---|---|
| Data | 2026-07-21 |
| Executor | Codex (assistente) |
| Ambiente | Local — `https://localhost:5001` |
| Commit testado | `70e34bc` |
| Escopo | SIGN-002, SIGN-003, SIGN-004, SIGN-006, SIGN-011, SIGN-014, SIGN-021, AUD-003 |
| Massa de dados | Reutiliza as contas `QA-20260721-` da rodada anterior; documentos novos com prefixo `QA-20260721-SIGN-*` |
| Executor técnico | Script Python ad hoc no scratchpad da sessão (não versionado) |

### Resultados

| Caso | Resultado | Perfil | Evidência | Defeito | Observação |
|---|---|---|---|---|---|
| SIGN-006 | Aprovado | USR-A | `400`, "At least one signer is required" | — | |
| SIGN-003 | Aprovado | USR-A | `201`, `totalSteps=1`, 2 signatários pendentes | — | Fluxo paralelo válido |
| SIGN-004 | Aprovado | USR-A | `201`, `totalSteps=2`, `currentStep=1` | — | Fluxo sequencial válido |
| SIGN-014 | Aprovado | Anônimo/qualquer portador do GUID | `400 BUSINESS_ERROR`, "signer is not authorized... at this time" | — | Regra de ordem da etapa é respeitada; a rejeição não é por identidade, é por estado |
| SIGN-002 | **Falhou** | USR-B | `201`, fluxo criado no documento de `USR-A` | `BUG-2026-002` | `USR-B` não é criador nem participante do documento |
| SIGN-011 | **Falhou (crítico)** | USR-B | `200`, assinatura registrada em nome de "QA Signer Externo" | `BUG-2026-003` | `USR-B` assinou usando apenas o `SignerId`, sem qualquer vínculo com o e-mail/identidade do signatário |
| SIGN-021 | **Falhou** | USR-B | `200`, rejeição registrada em nome de `USR-A2` | `BUG-2026-004` | `USR-B` rejeitou em nome de outro signatário usando apenas o `SignerId` |
| AUD-003 | **Falhou** | USR-A | `200`, 17 eventos retornados no período, incluindo ações de `USR-B` | `BUG-2026-005` (agrava `RSK-001`) | Consulta global de auditoria não filtra por identidade/escopo do requisitante |

### Defeitos

#### BUG-2026-002 — Fluxo de assinatura pode ser criado em documento de outro usuário

| Campo | Valor |
|---|---|
| Caso relacionado | SIGN-002 (confirma `RSK-002`) |
| Severidade | Crítica |
| Ambiente e commit | Local, `70e34bc` |
| Perfil | `USR-B` autenticado, sem relação com o documento |
| Pré-condição | JWT válido de qualquer usuário; GUID do documento alheio |
| Resultado atual | `POST /api/v1/signatureflow` cria o fluxo e retorna `201`, mesmo com `USR-B` não sendo criador, signatário nem participante do departamento do documento. |
| Resultado esperado | `403`/`404`; nenhum fluxo deveria ser criado. |
| Frequência | Sempre |
| Evidência sanitizada | `POST /api/v1/signatureflow` com `documentId` de documento de `USR-A`, token de `USR-B`; resposta `201` com o fluxo criado. |

Causa raiz: `SignatureFlowController` não tem `[Authorize]` e `CreateSignatureFlowCommandHandler` nunca compara o usuário autenticado com o criador/departamento/participantes do documento.

#### BUG-2026-003 — Qualquer usuário autenticado (ou anônimo) pode assinar em nome de outro signatário

| Campo | Valor |
|---|---|
| Caso relacionado | SIGN-011 (confirma `RSK-003`) |
| Severidade | **Crítica** — compromete a integridade jurídica da assinatura eletrônica, função central do produto |
| Ambiente e commit | Local, `70e34bc` |
| Perfil | `USR-B`, sem qualquer vínculo com o signatário alvo |
| Pré-condição | Conhecer (ou adivinhar/enumerar) o `SignerId` (GUID) de um signatário pendente |
| Resultado atual | `POST /api/v1/signers/sign` executa a assinatura completa — gera nova versão do PDF, hash, QR de verificação pública e registro de auditoria — atribuída ao nome/e-mail do signatário do `SignerId`, sem exigir prova de identidade, token de convite ou qualquer vínculo entre o token JWT do chamador e o signatário. |
| Resultado esperado | Operação negada sem token de convite forte vinculado ao signatário, ou sem autenticação correspondente à identidade do signatário. |
| Frequência | Sempre, para qualquer signatário elegível na etapa atual |
| Evidência sanitizada | `POST /api/v1/signers/sign` com `signerId` de "QA Signer Externo" (`qa-20260721-ext@example.invalid`), token de `USR-B`; resposta `200` com `status=2` (Signed) e `signedAt` preenchido. |

Passos para reproduzir:

1. Obter (por qualquer meio — URL de e-mail, log, resposta de API, enumeração) o `SignerId` de um signatário pendente.
2. Autenticar como qualquer usuário do sistema (o teste demonstrou que nem autenticação é exigida, conforme `AUTH-001-B08`).
3. Enviar `POST /api/v1/signers/sign` com esse `SignerId` e `signatureType: 1` (Eletrônica, sem certificado).
4. A assinatura é aceita e uma nova versão assinada do documento é gerada.

Impacto para o usuário: qualquer pessoa com acesso à aplicação (mesmo sem ser participante do documento) pode assinar eletronicamente em nome de qualquer signatário pendente, produzindo um documento com valor probatório indevido. Isso invalida a garantia central de uma plataforma de assinatura eletrônica.

Mitigação temporária: nenhuma no cliente. É necessário bloquear produção com este defeito aberto.

#### BUG-2026-004 — Qualquer usuário pode rejeitar em nome de outro signatário

| Campo | Valor |
|---|---|
| Caso relacionado | SIGN-021 (confirma `RSK-003`) |
| Severidade | Alta |
| Ambiente e commit | Local, `70e34bc` |
| Perfil | `USR-B`, sem vínculo com o signatário alvo |
| Pré-condição | Conhecer o `SignerId` de um signatário pendente |
| Resultado atual | `POST /api/v1/signers/reject` aceita a rejeição em nome do signatário do `SignerId`, sem checar identidade. |
| Resultado esperado | Operação negada sem prova de identidade vinculada ao signatário. |
| Frequência | Sempre |
| Evidência sanitizada | `POST /api/v1/signers/reject` com `signerId` de "QA Seq Signer 1" (e-mail de `USR-A2`), token de `USR-B`; resposta `200`, documento movido para `Rejected`. |

Impacto: um terceiro mal-intencionado pode bloquear/derrubar um fluxo de assinatura legítimo de outra pessoa apenas conhecendo o GUID do signatário — nega serviço direcionado ao processo de negócio.

#### BUG-2026-005 — Consulta de auditoria por período não filtra por identidade do requisitante

| Campo | Valor |
|---|---|
| Caso relacionado | AUD-003 (agrava `RSK-001`, relacionado a `RSK-015`) |
| Severidade | Alta |
| Ambiente e commit | Local, `70e34bc` |
| Perfil | `USR-A`, usuário comum autenticado |
| Pré-condição | Qualquer JWT válido |
| Resultado atual | `GET /api/v1/auditlogs/date-range` retorna todos os eventos do período (17 eventos), incluindo ações de `USR-B`, sem qualquer filtro por departamento, participação ou role administrativa. Também não há paginação. |
| Resultado esperado | Escopo restrito ao que o perfil do requisitante pode legitimamente ver (ou `403` para usuário comum), conforme `README.md` da matriz de QA. |
| Frequência | Sempre |
| Evidência sanitizada | `GET /api/v1/auditlogs/date-range?startDate=2026-07-21T00:00:00Z&endDate=2026-07-22T00:00:00Z` com token de `USR-A`; 17 registros retornados, incluindo eventos com detalhes de nome/e-mail de signatários vinculados a `USR-B`. |

### Resumo da rodada

| Prioridade | Aprovados | Falharam | Bloqueados | Não executados |
|---|---:|---:|---:|---:|
| P0 | 4 | 4 | 0 | 0 |
| P1 | 0 | 0 | 0 | 0 |
| P2 | 0 | 0 | 0 | 0 |

Decisão: **Reprovado — bloqueio crítico.** A regra de negócio de ordem de assinatura (`SIGN-014`) funciona corretamente, mas a ausência total de verificação de identidade em `SignatureFlowController` e `SignersController` permite que qualquer usuário crie fluxos em documentos alheios, assine e rejeite em nome de outros signatários. `BUG-2026-003` é o defeito mais grave encontrado até agora no projeto.

Riscos confirmados integralmente (não apenas ausência de `[Authorize]`, mas efeito real comprovado): `RSK-002`, `RSK-003`. Novo defeito: `BUG-2026-005` (agrava `RSK-001`).

Riscos residuais e próximos passos:

- **Bloquear a fase de homologação até `BUG-2026-003` ser corrigido.** É o achado de maior severidade da rodada.
- 3 documentos de teste (`QA-20260721-SIGN-002-alvo`, `-SIGN-003-parallel`, `-SIGN-004-sequencial`) não puderam ser removidos pela aplicação porque saíram do status `Draft` (regra `DOC-014` funcionando corretamente); permanecem no banco local como massa de teste residual até limpeza administrativa direta ou reset do ambiente local.
- Ainda não testado: `SIGN-005` (híbrido), `SIGN-007` a `SIGN-010`, `SIGN-012` a `SIGN-020`, `SIGN-022` a `SIGN-025`, `NOTIF-*`, `EXT-*`, `PUB-*`.
- Ordem recomendada: corrigir `BUG-2026-001` a `BUG-2026-005` (autorização e identidade) antes de investir mais tempo testando `NOTIF-*`/`EXT-*`/`PUB-*`, já que a causa raiz é comum (ausência de verificação de identidade do requisitante nos handlers de `SignatureFlow` e `Signers`, e escopo ausente em `AuditLogs`).

---

## Rodada QA-2026-07-21-04 — Verificação das correções

### Identificação

| Campo | Valor |
|---|---|
| Data | 2026-07-21 |
| Executor | Codex (assistente) |
| Ambiente | Local — `https://localhost:5001` |
| Commit testado | Correções aplicadas sobre `70e34bc` (não commitado nesta sessão) |
| Escopo | Repetição de AUTH-001 (34 requisições), rodada `ACL-*`/`DOC-*` (14 casos) e rodada `SIGN-*`/`AUD-003` (8 casos) |

### Correções aplicadas

| Defeito | Arquivos alterados | Estratégia |
|---|---|---|
| `BUG-2026-001` (`RSK-004`) | `DocumentsController.cs` | `CreatedByUserId` agora é sempre sobrescrito pela identidade do JWT antes de montar o comando; o valor do formulário é ignorado. |
| `BUG-2026-002` (`RSK-002`) | `SignatureFlowController.cs`, `CreateSignatureFlowCommand(Handler).cs`, `GetSignatureFlowByIQuery.cs`/`Handler.cs`, `GetFlowsByDocumentQuery.cs`/`Handler.cs` | Controller passa a exigir `[Authorize]`; criação de fluxo só é aceita se o requisitante for o criador do documento; leitura de fluxo (por ID ou por documento) exige a mesma regra de acesso já usada em `DocumentsController` (criador, signatário por e-mail ou departamento não confidencial). |
| `BUG-2026-003`/`BUG-2026-004` (`RSK-003`) | `SignersController.cs`, `SignDocumentCommand(Handler).cs`, `RejectDocumentCommand(Handler).cs`, `GetSignerByIdQuery.cs`/`Handler.cs` | Controller exige `[Authorize]`; assinar/rejeitar só são aceitos se o e-mail do JWT do requisitante corresponder ao e-mail do signatário do `SignerId`; consulta de pendências por e-mail (`/signers/pending/{email}`) exige que o requisitante autenticado seja o dono do e-mail ou administrador; `GetSignerById` exige a mesma regra de acesso ao documento. |
| `BUG-2026-005` (`RSK-001`) | `AuditLogsController.cs`, `GetAuditLogsByDocumentQuery.cs`/`Handler.cs` | Controller exige `[Authorize]`; `date-range` (consulta global) restrito a `Admin`/`Administrator`; consulta por documento exige acesso ao documento (mesma regra de `DocumentsController`). |

### Resultados

| Rodada repetida | Resultado |
|---|---|
| AUTH-001 completo (34 requisições) | **34/34 aprovado** — todas as rotas retornam `401` sem JWT, incluindo as 9 que antes falhavam. |
| `ACL-*`/`DOC-*` (14 casos, incl. `DOC-002`) | **14/14 aprovado** — `DOC-002` agora retorna `createdByUserId` = identidade do JWT do requisitante, ignorando o valor forjado no corpo. |
| `SIGN-002` | `404` (antes `201`) — `USR-B` não consegue mais criar fluxo em documento alheio. |
| `SIGN-011` | `404` (antes `200`) — `USR-B` não consegue mais assinar em nome de outro signatário. |
| `SIGN-021` | `404` (antes `200`) — `USR-B` não consegue mais rejeitar em nome de outro signatário. |
| `SIGN-003`, `SIGN-004`, `SIGN-006`, `SIGN-014` | Continuam aprovados — nenhuma regressão nas regras de negócio de fluxo (paralelo, sequencial, validação e ordem de etapas). |
| `AUD-003` | `403` (antes `200`) — `USR-A` (role `User`) não consegue mais consultar auditoria global por período. |

### Resumo da rodada

| Prioridade | Aprovados | Falharam | Bloqueados | Não executados |
|---|---:|---:|---:|---:|
| P0 | 8 | 0 | 0 | 0 |
| P1 | 0 | 0 | 0 | 0 |
| P2 | 0 | 0 | 0 | 0 |

Decisão: **Aprovado para os 5 defeitos corrigidos nesta rodada.** `BUG-2026-001` a `BUG-2026-005` foram verificados e não reproduzem mais nos cenários testados. Nenhuma regressão observada nas regras de negócio já validadas (isolamento por departamento/confidencialidade, ordem de etapas de assinatura, exclusão de rascunho).

Riscos residuais e próximos passos:

- Esta rodada não é suficiente para reabrir a homologação: `SIGN-005`, `SIGN-007` a `SIGN-010`, `SIGN-012`, `SIGN-013`, `SIGN-015` a `SIGN-020`, `SIGN-022` a `SIGN-025`, `DOC-003` a `DOC-008`, `DOC-010`, `DOC-011`, `DOC-014` a `DOC-020`, `NOTIF-*`, `EXT-*`, `PUB-*`, `OPS-*` e os demais `ACL-*`/`AUD-*` continuam **não executados**.
- O modelo de correção para `SignersController` exige `[Authorize]` (JWT válido, confirmado pelo `401` de `AUTH-001-B08`/`B09` na rodada repetida) **e** correspondência entre o e-mail do JWT e o e-mail do signatário do `SignerId`. Isso fecha o ataque confirmado (terceiro autenticado assinando/rejeitando em nome de outro signatário), mas **exige que todo signatário tenha uma conta local com e-mail cadastrado** — o cenário de signatário externo sem conta (`SIGN-EXT`, citado como "se esse fluxo for suportado" no `README.md`) deixa de funcionar com esta correção. Se o produto precisa suportar assinatura por e-mail sem conta, será necessário um mecanismo de token de convite dedicado por signatário (não coberto por esta correção) em vez de exigir login. Confirmar com o time de produto qual comportamento é o pretendido antes de homologar.
- 2 documentos de teste (`QA-20260721-SIGN-003-parallel`, `-SIGN-004-sequencial`) permanecem no banco local por terem saído do status `Draft`; seguem como massa residual.
- As 3 contas `qa-20260721-usr-*@example.invalid` continuam ativas e precisam de uma conta `Admin` para remoção.
- Nenhuma alteração foi commitada nesta sessão; revisar o diff e decidir sobre commit/PR antes de prosseguir.

---

## Rodada QA-2026-07-21-05 — Token de convite por signatário (assinante externo)

### Contexto da decisão

Após a rodada anterior, foi levantado que a correção de `RSK-003` (exigir e-mail do JWT = e-mail do signatário) quebra um requisito real de produto: (a) um funcionário já autenticado via AD/SSO deve poder usar essa identidade, e (b) uma pessoa **externa à organização, sem conta local**, precisa conseguir assinar/rejeitar apenas aquele documento pelo link recebido por e-mail, sem ganhar acesso interno ao sistema. Investigação no frontend Angular (`age-signaturehub-web`) confirmou que **não existe hoje nenhuma tela pública de assinatura** — a única forma de assinar pela UI atual é logado, via lista de pendências (`pending-signatures`, atrás de `authGuard`). Ou seja, a assinatura externa nunca foi implementada na tela; só existia uma brecha na API que coincidia com essa possibilidade. Decisão: implementar o mecanismo de token no backend nesta sessão; a tela pública no Angular fica para uma sessão dedicada futura.

### Implementação

| Item | Descrição |
|---|---|
| `Signer.InvitationToken` | Novo campo no domínio (`Signer.cs`), gerado com `RandomNumberGenerator` (32 bytes, base64 URL-safe, 43 caracteres) na criação do signatário. Validado com `CryptographicOperations.FixedTimeEquals` (comparação em tempo constante). |
| Migração `AddSignerInvitationToken` | Coluna `InvitationToken` (varchar 64, `NOT NULL`, default `''`) na tabela `Signers`. Aplicada ao banco local. |
| `SignDocumentDto`/`RejectDocumentDto` | Novo campo opcional `InvitationToken`. |
| `SignDocumentCommandHandler`/`RejectDocumentCommandHandler` | Autoriza se `(e-mail do JWT == e-mail do signatário) OU (token informado é válido para o signatário)`. Sem JWT e sem token válido → `404` (mesmo padrão de ocultação de existência já usado no projeto). |
| `GetSignerByIdQueryHandler` | Mesma regra, mais acesso por documento (criador/departamento) para uso interno. |
| `SignersController` | `[Authorize]` removido do controller; mantido apenas em `GET /signers/pending/{email}` (uso interno, self-service). `sign`/`reject`/`GetSignerById` voltam a aceitar chamadas anônimas, agora gated pelo token em vez de JWT obrigatório. |
| URLs de e-mail | `CreateSignatureFlowCommandHandler` e `SignDocumentCommandHandler` (notificação da próxima etapa) agora incluem `?token={InvitationToken}` no link de assinatura enviado por e-mail. |
| `docs/RouteFiles/api-routes.http` | `AUTH-001-B07/B08/B09` movidos da seção "deve retornar 401" para a seção C (rotas intencionalmente públicas), com nota explicando o novo modelo. `AUTH-001` volta a ser 31/31 aprovado (não 34, porque essas 3 rotas não são mais gated por JWT por design). |

### Resultados (verificação dinâmica com massa `QA-20260721-INVITE-*`)

| Cenário | Resultado |
|---|---|
| Assinar anônimo (sem JWT), sem `InvitationToken` | `404` — negado |
| Assinar anônimo com `InvitationToken` **errado** | `404` — negado |
| `USR-B` autenticado (não é o signatário), sem token | `404` — negado |
| **Assinar anônimo (sem JWT) com o `InvitationToken` correto** | `200` — assinatura registrada com sucesso, como o signatário externo |
| `GetSignerById` anônimo com token correto | `200` |
| `GetSignerById` anônimo sem token | `404` — negado |
| Rejeitar anônimo com token correto | `200` — rejeição registrada |
| Rejeitar anônimo com token errado | `400` — negado |

O mecanismo fecha o ataque original (`BUG-2026-003`/`BUG-2026-004`: qualquer um assinando/rejeitando com apenas o `SignerId`) **e** preserva a assinatura anônima para signatário externo, desde que ele tenha o segredo de posse (`InvitationToken`) — que só é entregue por e-mail, nunca por uma resposta de API (`SignerDto` não expõe o campo).

### Resumo da rodada

| Prioridade | Aprovados | Falharam | Bloqueados | Não executados |
|---|---:|---:|---:|---:|
| P0 | 8 | 0 | 0 | 0 |

Decisão: **Aprovado.** `RSK-003` permanece corrigido sem regredir o requisito de assinatura externa.

Riscos residuais e próximos passos:

- **Token armazenado em texto plano no banco** (não hash). Escolha deliberada: o token precisa ser reenviado em e-mails de etapas posteriores de um fluxo sequencial/híbrido (`NotifyNextSigners`), então um hash unidirecional exigiria regenerar o token a cada reenvio. Isso é aceitável como um segredo de posse de propósito único e vida curta (documento pendente), mas é um ponto de dureza a revisitar — considerar hashing com regeneração em caso de reenvio manual de convite, se essa funcionalidade for adicionada.
- **Sem expiração própria do token.** Hoje o token vive enquanto o signatário estiver `Pending`; não há TTL independente do estado do fluxo. Avaliar se isso é suficiente ou se um prazo de expiração explícito é necessário.
- **Página pública de assinatura no Angular ainda não existe.** O backend já suporta o fluxo (`GET /signers/{id}?token=`, `POST /signers/sign` e `/reject` com `invitationToken` no corpo), mas falta a rota pública (fora de `authGuard`), o componente de UI e os testes em navegador. Ficou fora do escopo desta sessão — tratar como funcionalidade nova em sessão dedicada, com teste real no navegador antes de considerar concluída.
- Signatários criados **antes** desta migração têm `InvitationToken = ''` (default da migração) e não conseguem usar o caminho anônimo — apenas o caminho autenticado por e-mail continua funcionando para eles. Não afeta os dados de produção reais ainda, pois nenhuma migração foi aplicada em homologação/produção nesta sessão.
