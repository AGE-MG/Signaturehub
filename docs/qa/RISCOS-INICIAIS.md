# Registro inicial de riscos

Data da revisão: **2026-07-20**  
Método: revisão estática exploratória do código atual.  
Estado: estes itens são **candidatos a defeito**, não resultados de teste dinâmico. Cada risco deve ser confirmado ou descartado pelos casos indicados antes de uma correção.

## Resumo

| ID | Severidade preliminar | Confiança | Risco | Casos de confirmação |
|---|---|---|---|---|
| RSK-001 | Crítica | **Corrigido em 2026-07-21 (verificado)** | Endpoints de auditoria estavam acessíveis anonimamente e sem escopo | AUD-001 a AUD-004 |
| RSK-002 | Crítica | **Corrigido em 2026-07-21 (verificado)** | Qualquer usuário autenticado criava fluxo em documento alheio | SIGN-001, SIGN-002 |
| RSK-003 | Crítica | **Corrigido em 2026-07-21 (verificado, incl. fluxo de signatário externo via token de convite)** | Qualquer usuário assinava/rejeitava em nome de outro signatário usando apenas o `SignerId` | SIGN-008 a SIGN-012 |
| RSK-004 | Alta | **Corrigido em 2026-07-21 (verificado)** | Criador do documento era aceito do corpo enviado pelo cliente | DOC-002 |
| RSK-005 | Alta | Alta | Marcação individual de notificação não verifica o proprietário | NOTIF-001 |
| RSK-006 | Crítica | Alta | Chaves e senhas estão versionadas em configuração | OPS-002 |
| RSK-007 | Alta | Média | Verificação pública pode expor documento/versão e dados pessoais além do necessário | PUB-002 a PUB-004 |
| RSK-008 | Alta | Alta | Cadastro público cria conta ativa e e-mail confirmado sem verificação | AUTH-012, AUTH-013 |
| RSK-009 | Alta | Média | Não foi observada limitação de requisições em login, cadastro e consultas públicas | AUTH-011, PUB-003 |
| RSK-010 | Alta | Alta | Validação de webhook bloqueia loopback, mas aparenta permitir redes privadas e DNS rebinding | EXT-004 |
| RSK-011 | Alta | Média | Retry de job pode duplicar notificações internas por falta de chave de idempotência | NOTIF-004, EXT-008 |
| RSK-012 | Alta | Alta | Migração/snapshot/banco ainda não foram comprovados como convergentes | OPS-003 |
| RSK-013 | Alta | Média | Upload pode deixar arquivo órfão se banco falhar depois do storage | DOC-015, DOC-016 |
| RSK-014 | Alta | Média | Falha após persistir assinatura pode retornar erro e induzir repetição da mutação | SIGN-016, SIGN-025 |
| RSK-015 | Média | Alta | Consulta global de auditoria não aparenta paginação ou limite de intervalo | AUD-004, PERF-001 |
| RSK-016 | Média | Média | JWT e refresh token no `localStorage` ampliam impacto de eventual XSS | AUTH-015, OPS-002 |

## Evidências e impacto esperado

### RSK-001 — Auditoria sem autorização

`AuditLogsController` não possui `[Authorize]`, e suas consultas retornam logs por documento ou intervalo sem receber a identidade do solicitante. O impacto potencial inclui exposição de nomes, e-mails, motivos de rejeição, IDs e histórico operacional.

**Confirmação dinâmica em 2026-07-20:** `AUTH-001-B01` e `AUTH-001-B02` retornaram `200` sem JWT.

**Confirmação dinâmica em 2026-07-21 (`AUD-003`, `BUG-2026-005`, rodada `QA-2026-07-21-03`):** mesmo autenticado, `USR-A` (usuário comum) obteve `200` em `GET /api/v1/auditlogs/date-range` com 17 eventos do período, incluindo ações praticadas por `USR-B` (sem relação com `USR-A`), sem paginação. O problema não é apenas a ausência de `[Authorize]`: mesmo com identidade autenticada disponível, a consulta não aplica nenhum filtro de escopo.

**Correção aplicada e verificada em 2026-07-21 (rodada `QA-2026-07-21-04`):** `AuditLogsController` agora exige `[Authorize]`; `GET /auditlogs/date-range` restrito a `Admin`/`Administrator` (`USR-A` recebeu `403` na repetição do teste); `GET /auditlogs/document/{id}` agora exige a mesma regra de acesso ao documento usada em `DocumentsController`. `AUTH-001` completo (34/34) confirma que nenhuma das duas rotas responde mais sem JWT. Paginação/limite de intervalo (`RSK-015`) continua pendente.

### RSK-002 — Fluxos sem autenticação e autorização por objeto

`SignatureFlowController` não possui `[Authorize]`. O handler de criação busca o documento diretamente por ID e não valida criador, participação ou departamento. Uma exploração confirmada permitiria inserir signatários e alterar o fluxo de documento alheio.

**Confirmação dinâmica em 2026-07-20:** as leituras anônimas `AUTH-001-B03` e `AUTH-001-B04` retornaram `404` e `200`, respectivamente, demonstrando que alcançaram os handlers.

**Confirmação dinâmica em 2026-07-21 (AUTH-001):** `AUTH-001-B05` (criação de fluxo, GUID de documento nulo) retornou `400` de validação de negócio ("Document ID is required"), não `401`, confirmando que a requisição anônima passa pelo pipeline sem bloqueio de autenticação.

**Confirmação dinâmica em 2026-07-21 (`SIGN-002`, `BUG-2026-002`, rodada `QA-2026-07-21-03`):** com contas de teste reais, `USR-B` (sem qualquer vínculo com o documento) criou com sucesso (`201`) um fluxo de assinatura em um documento pertencente a `USR-A`. `CreateSignatureFlowCommandHandler` nunca compara o usuário autenticado com o criador, departamento ou signatários do documento — o risco está confirmado com efeito real, não apenas alcance do handler.

**Correção aplicada e verificada em 2026-07-21 (rodada `QA-2026-07-21-04`):** `SignatureFlowController` agora exige `[Authorize]`; criação de fluxo restrita ao criador do documento (`USR-B` recebeu `404` ao repetir `SIGN-002`); leitura de fluxo por ID e por documento agora exige a mesma regra de acesso ao documento (criador, signatário por e-mail ou departamento não confidencial). `SIGN-003`/`SIGN-004` (fluxo válido pelo criador) continuam aprovados, sem regressão.

### RSK-003 — `SignerId` funcionando como credencial

`SignersController` não possui `[Authorize]`. Os DTOs de assinatura e rejeição recebem `SignerId`, mas não token de convite, prova de posse do e-mail ou identidade autenticada vinculada ao signatário. Há também consulta pública de pendências por e-mail. O GUID reduz adivinhação casual, porém não substitui autorização e pode vazar por URLs, logs e respostas.

**Confirmação dinâmica em 2026-07-20:** `AUTH-001-B06` e `AUTH-001-B07` retornaram `400` e `404` sem JWT, demonstrando acesso anônimo aos handlers.

**Confirmação dinâmica em 2026-07-21 (AUTH-001):** `AUTH-001-B08` (assinar, `SignerId` nulo) retornou `400` de validação ("Signer ID is required"). `AUTH-001-B09` (rejeitar, `SignerId` nulo) retornou `404` com `"Entity \"signer\" ... was not found"` — evidência de que o handler de rejeição consultou o repositório de signatários sem qualquer verificação de identidade ou token de convite.

**Confirmação dinâmica em 2026-07-21 (`SIGN-011`/`SIGN-021`, `BUG-2026-003`/`BUG-2026-004`, rodada `QA-2026-07-21-03`) — risco materializado em impacto máximo:** com um fluxo de assinatura paralelo real (2 signatários pendentes), `USR-B` — autenticado, sem qualquer relação com o documento ou com os signatários — assinou eletronicamente (`POST /api/v1/signers/sign`, `200`) em nome de "QA Signer Externo" apenas informando o `SignerId`, gerando nova versão do PDF, hash e registro de auditoria como se fosse aquele signatário. Da mesma forma, `USR-B` rejeitou (`POST /api/v1/signers/reject`, `200`) em nome de outro signatário cujo e-mail pertencia a `USR-A2`. `SignersController` não tem `[Authorize]` e `SignDocumentCommandHandler`/`RejectDocumentCommandHandler` não verificam identidade alguma — apenas o estado do fluxo (ordem/etapa). Esta é a falha de maior severidade encontrada no projeto: compromete a integridade jurídica da assinatura eletrônica, a função central do produto.

**Correção aplicada e verificada em 2026-07-21 (rodada `QA-2026-07-21-04`):** `SignersController` agora exige `[Authorize]`; assinar/rejeitar exigem que o e-mail do JWT do requisitante corresponda ao e-mail do signatário do `SignerId` (`USR-B` recebeu `404` ao repetir `SIGN-011` e `SIGN-021`); `GET /signers/pending/{email}` exige que o requisitante seja dono do e-mail (ou administrador); `GetSignerById` exige a mesma regra de acesso ao documento.

**Ajuste em 2026-07-21 (rodada `QA-2026-07-21-05`) — token de convite por signatário:** a correção acima quebrava o requisito de `SIGN-EXT` (signatário externo sem conta assinando via link de e-mail, sem acesso interno ao sistema). Adicionado `Signer.InvitationToken` (segredo aleatório de 32 bytes, gerado por signatário, nunca exposto em `SignerDto`, enviado apenas no link do e-mail). `SignersController` voltou a aceitar chamadas anônimas em `sign`/`reject`/`GetSignerById`, agora autorizadas por `(e-mail do JWT == e-mail do signatário) OU (InvitationToken válido)`. Verificado: assinatura/rejeição/consulta anônimas com o token correto funcionam (`200`); sem token, com token errado, ou autenticado como outro usuário sem o token — todos negados (`404`/`400`). `GET /signers/pending/{email}` permanece restrito a usuário autenticado dono do e-mail (uso interno, sem caso de uso externo). **Pendente:** a página pública de assinatura no Angular (fora do `authGuard`) ainda não existe — o backend suporta o fluxo, mas falta a UI para o assinante externo efetivamente usá-lo.

### RSK-004 — Identidade do criador controlada pelo cliente

`CreateDocumentDto` inclui `CreatedByUserId`; o controller encaminha o valor recebido e o handler o usa para proprietário, departamento, auditoria e notificações. O servidor deveria derivar essa identidade exclusivamente do JWT.

**Confirmação dinâmica em 2026-07-21 (`BUG-2026-001`, ver `REGISTRO-EXECUCAO.md` rodada `QA-2026-07-21-02`):** `USR-A` autenticado enviou `documentData.CreatedByUserId` = ID de `USR-B` em `POST /api/v1/documents`; a resposta `201` retornou `createdByUserId` = `USR-B`. Ao tentar excluir o documento como limpeza de massa de teste, `USR-A` recebeu `404` (perdeu acesso ao próprio upload) e `USR-B` conseguiu excluí-lo normalmente — confirmando que o documento passa a pertencer de fato à identidade forjada, incluindo departamento e controle de exclusão.

**Correção aplicada e verificada em 2026-07-21 (rodada `QA-2026-07-21-04`):** `DocumentsController.CreateDocument` agora sobrescreve `documentData.CreatedByUserId` com a identidade do JWT antes de montar o comando. Repetição de `DOC-002` confirmou `createdByUserId` = `USR-A` (quem de fato autenticou), mesmo enviando o ID de `USR-B` no corpo.

### RSK-005 — Notificação de outro usuário

O endpoint é autenticado, mas `MarkNotificationAsReadCommand` carrega apenas `NotificationId`. O handler busca e altera a notificação sem comparar seu `UserId` com o usuário autenticado.

### RSK-006 — Segredos versionados

O `appsettings.json` rastreado contém senha de banco, chave de assinatura JWT e segredo de webhook. Mesmo que sejam valores locais ou antigos, devem ser tratados como comprometidos, removidos da configuração versionada e rotacionados nos ambientes onde tenham sido usados. Remover apenas do commit atual não apaga o histórico Git.

### RSK-007 — Superfície pública de verificação

A verificação anônima busca o documento por GUID e retorna título, nome original, hash e lista de signatários com nome e e-mail. A consulta não demonstra no controller uma regra explícita que restrinja a publicação a versões/estados assinados e publicáveis. É necessário validar também documento confidencial, rascunho, rejeitado e excluído, além da base legal para dados pessoais apresentados.

### RSK-008 — Cadastro público ativo

`POST /auth/register` é anônimo. O serviço cria usuário ativo, marca `EmailConfirmed = true` e atribui `User`. Em ambiente corporativo com AD isso pode contornar o processo de identidade se cadastro local não for uma decisão explícita.

### RSK-009 — Abuso automatizado

Não foi encontrada configuração de rate limiting no pipeline da API. Login, cadastro, refresh, consulta pública e qualquer endpoint de e-mail podem ser alvos de força bruta, enumeração ou esgotamento de recursos.

### RSK-010 — SSRF em integrações do usuário

A validação exige HTTPS e bloqueia localhost/loopback, mas não bloqueia explicitamente faixas privadas, link-local, metadata cloud, IPv6 local nem revalida o IP após DNS. Como o servidor realiza a chamada, um usuário pode tentar alcançar serviços internos.

### RSK-011 — Duplicação em retries

`DocumentNotificationJob` persiste notificações internas antes das entregas externas e permite três tentativas automáticas. Uma falha posterior à persistência pode executar o job novamente e inserir notificações duplicadas, pois não foi observada deduplicação por `EventId` e destinatário.

### RSK-012 — Estado de migrações não comprovado

A aplicação da migração de integrações externas não foi comprovada após o alerta de mudanças pendentes no modelo. Antes de homologar, é necessário testar banco vazio e upgrade de uma cópia representativa, conferindo migrations, snapshot e `__EFMigrationsHistory`.

### RSK-013 — Arquivo órfão no upload

O arquivo é enviado ao storage antes da criação ser persistida no banco. Se a obtenção do criador ou o `SaveChanges` falhar, não foi observada compensação que remova o arquivo já gravado.

### RSK-014 — Sucesso persistido apresentado como erro

No fluxo de assinatura há commit antes de algumas operações posteriores. Se uma etapa pós-commit falhar fora do isolamento esperado, a API pode responder com erro embora a assinatura esteja persistida; o usuário pode tentar novamente e gerar conflito ou duplicação.

### RSK-015 — Consulta de auditoria sem limites

A consulta por período aceita datas diretamente e retorna a lista completa. Intervalos extensos podem expor volume excessivo e causar pressão sobre banco, memória e serialização.

### RSK-016 — Tokens no armazenamento persistente do navegador

O Angular mantém access e refresh token em `localStorage`. Isso é comum, mas qualquer XSS no mesmo origin consegue lê-los. A decisão deve ser acompanhada por CSP rigorosa, prevenção sistemática de XSS, curta duração do access token e rotação/revogação do refresh token.

## Ordem recomendada de confirmação

1. `RSK-001` a `RSK-006`: autorização, identidade e segredos.
2. `RSK-007` a `RSK-010`: privacidade e superfícies anônimas/externas.
3. `RSK-011` a `RSK-014`: consistência, idempotência e recuperação.
4. `RSK-015` e `RSK-016`: endurecimento e capacidade operacional.

Uma confirmação dinâmica deve virar defeito rastreável com requisição sanitizada, resultado atual, resultado esperado, impacto, ambiente, commit e evidência. Um item descartado deve registrar por que o comportamento é seguro, para evitar que a mesma hipótese seja reaberta sem novos fatos.
