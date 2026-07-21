# Implementação — Transferir Responsabilidade

Origem: achado registrado em `RELATORIO.md` (seção 3) durante o teste manual do executor no documento `stt_de_en`. Decisão do executor: implementar a melhoria proposta em vez de só documentar.

## Problema

"Transferir Responsabilidade" era um alias de "Iniciar Fluxo de Assinatura": criava um `SignatureFlow` e um `Signer` **novos e desconectados** no mesmo documento, em vez de reatribuir a responsabilidade existente. Consequências observadas e confirmadas no banco:

- Cada "transferência" virava um fluxo independente, com seu próprio ciclo de vida (`Fluxo stt_de_en` e `Transferência de responsabilidade - stt_de_en` como dois fluxos `Sequential 1/1` sem relação entre si).
- `Document.Status` nunca voltava para "em andamento" — permanecia `Completed` do início ao fim, mesmo com um novo signatário pendente.
- A permissão do backend (`document.CreatedByUserId == RequestingUserId`) não correspondia ao gate visual do frontend (qualquer signatário que já assinou via `signedAt`), então usuários não-criadores viam o botão mas recebiam erro ao usá-lo.
- O botão continuava visível para o usuário original depois de uma transferência já ter acontecido, porque o frontend não distinguia "já assinei uma vez" de "sou o responsável atual".

## O que muda

"Transferir Responsabilidade" passa a **reabrir o mesmo fluxo, no mesmo passo**, em vez de criar um fluxo novo. Fluxo novo continua exclusivo de mudança de departamento (`Movimentar Departamento`), que já funcionava corretamente e não foi alterado.

### Modelo de dados

Dentro do mesmo `SignatureFlow` e do mesmo `SignOrder`, uma transferência adiciona um **novo `Signer`** (`Pending`) ao lado do antigo (que permanece `Signed`, preservando o histórico/auditoria de quem assinou antes). O fluxo é reaberto (`IsCompleted = false`) e `CurrentStep` é realinhado para o passo transferido. Quando o novo responsável assina, a lógica de conclusão de fluxo já existente (`SignDocumentCommandHandler`) trata os dois signatários daquele passo como parte do mesmo grupo e conclui o fluxo normalmente — nenhuma mudança foi necessária ali.

### Backend

| Arquivo | Mudança |
|---|---|
| `server/AGE.SignatureHub.Domain/Entities/SignatureFlow.cs` | Novo método `ReopenForTransfer()` (`IsCompleted = false`, `CompletedAt = null`). |
| `server/AGE.SignatureHub.Application/DTOs/SignatureFlow/TransferSignatureResponsibilityDto.cs` | Novo DTO: `NewResponsibleName/Email/Document` + campos de identidade/auditoria preenchidos pelo controller. |
| `server/AGE.SignatureHub.Application/Features/SignatureFlows/Commands/TransferSignatureResponsibility/*` | Novo `Command` + `CommandHandler` + `Validator`. |
| `server/AGE.SignatureHub.API/Controllers/DocumentsController.cs` | Novo endpoint `POST /api/v1/documents/{id}/transfer-responsibility`, no mesmo controller e padrão de `transfer-department`. |
| `server/AGE.SignatureHub.Application/DTOs/Signer/SignerDto.cs` | Adicionado `CreatedAt` — necessário para o frontend saber quem é o responsável **atual** de cada etapa (ver seção Frontend). |

Regras de autorização e negócio no handler (`TransferSignatureResponsibilityCommandHandler`):

1. **Quem pode transferir:** apenas alguém que já possui uma assinatura `Signed` no documento, com o e-mail do JWT batendo o e-mail do signatário (mesmo modelo de identidade usado em `sign`/`reject` desde a correção de `RSK-003`) — não é mais "apenas o criador do documento". Se o requisitante tiver mais de uma assinatura no documento, usa a mais recente (`SignedAt` mais alto).
2. **Novo responsável != atual:** rejeita se o e-mail novo for igual ao atual (`400`).
3. **Só a última etapa:** para fluxos `Sequential`/`Hybrid`, só é permitido transferir a responsabilidade de um signatário cujo `SignOrder` seja igual a `flow.TotalSteps` (a última etapa). Sem essa trava, transferir uma etapa **já ultrapassada** de um fluxo em andamento faria `CurrentStep` voltar para trás, misturando o estado de etapas posteriores já concluídas. `Parallel` não tem essa restrição — não existe noção de "etapa anterior" nesse tipo de fluxo.
4. Sem essas condições → `NotFoundException` (documento inexistente) ou `BusinessException` (regra de negócio, mensagem exibida ao usuário) — mesmo padrão do resto do projeto.

Efeitos da transferência bem-sucedida: novo `Signer` (`Pending`, mesmo `SignOrder`/`Role` do anterior) adicionado ao fluxo existente; `flow.CurrentStep` realinhado para esse `SignOrder`; `flow.ReopenForTransfer()`; `document.UpdateStatus(PendingSignatures)`; `AuditLog` (`RESPONSIBILITY_TRANSFERRED`); e-mail de solicitação de assinatura enviado ao novo responsável com o link contendo o `InvitationToken` do novo `Signer` (mesmo mecanismo criado na correção de `RSK-003`, preservando a assinatura por link sem conta).

### Frontend (Angular)

| Arquivo | Mudança |
|---|---|
| `core/models/document.model.ts` | Novo `TransferSignatureResponsibilityDto`; `createdAt?` adicionado a `SignatoryDto`. |
| `core/models/signer.model.ts` | `createdAt?` adicionado a `SignerDto`. |
| `core/services/document.service.ts` | Novo método `transferResponsibility(documentId, payload)` → `POST {baseUrl}/{id}/transfer-responsibility`. |
| `features/internal/documents/document-details/document-details.component.ts` | `submitFlowForm()` agora ramifica por `flowFormMode`: `'transfer'` chama `documentService.transferResponsibility(...)`; `'start'` continua chamando `signatureFlowService.create(...)` como antes. `canTransferResponsibility` reescrito (ver abaixo). |

**Correção do botão obsoleto:** `canTransferResponsibility` agora agrupa os signatários de cada fluxo por `SignOrder` e, dentro de cada grupo, resolve o "responsável atual" como o signatário com o `createdAt` mais recente (`resolveCurrentResponsible`). O botão só aparece se o usuário logado for esse responsável atual **e** o status dele for `Signed`. Isso resolve os dois problemas relatados: o botão desaparece para quem já transferiu, e some para todo mundo enquanto uma transferência está pendente (o responsável atual do grupo passa a ser o novo `Signer`, ainda `Pending`, então ninguém satisfaz a condição até ele assinar).

## Achado secundário (não corrigido, fora de escopo)

Durante a investigação, notei que o enum `SignatureStatus` no frontend (`Cancelled = 4, Expired = 5`) está **invertido** em relação ao backend (`Expired = 4, Cancelled = 5`) — `Pending`/`Signed`/`Rejected` batem, só esses dois trocam de posição. Isso pode fazer a UI rotular incorretamente um signatário expirado como "Cancelado" ou vice-versa. Não mexi nisso porque é ortogonal a este patch; registrando aqui para não perder o achado.

## Validação

Reproduzido via API com contas de teste (`QA-20260721-*`), já que o cenário completo depende de múltiplas identidades e não dá para simular login AD/SSO fora do navegador:

| Cenário | Resultado |
|---|---|
| Documento criado por `USR-A`, único signatário `USR-A2` (não é o criador) assina | Documento e fluxo concluídos, `status=Completed` |
| `USR-B` (nunca assinou este documento) tenta transferir | `400`, negado |
| Transferir para o mesmo e-mail do atual | `400`, negado |
| `USR-A2` transfere para um novo responsável externo | `200` — **mesmo fluxo** reaberto (não criou um segundo fluxo), `IsCompleted=false` |
| Estado após a transferência | Documento volta para `PendingSignatures`; dois signatários no mesmo `SignOrder` (antigo `Signed`, novo `Pending`) — histórico preservado |
| Documento continua no feed do criador (`USR-A`) durante a transferência | Presente |
| Novo responsável (conta criada e logada de verdade) vê a pendência em `GET /signers/pending/{email}` e o documento em `GET /documents` | Presente em ambos |
| Novo responsável assina | `200` — fluxo e documento **concluem de novo** (`IsCompleted=true`, `status=Completed`) |
| Fluxo sequencial de 2 etapas: signatário da etapa 1 (já concluída) tenta transferir enquanto a etapa 2 ainda está pendente | `400`, negado — protege contra retroceder `CurrentStep` |

19/19 casos aprovados na rodada completa (mais o caso do guard de etapa intermediária). `AUTH-001` repetido (31/31) confirma que o novo endpoint continua exigindo autenticação e nada regrediu. `ng build --configuration development` e `dotnet build` limpos.

### Confirmação real no navegador (executor, 2026-07-21 ~14:19–14:23)

O executor testou o fluxo completo na interface, com login AD real, em um novo documento (`E-Books-799`, fluxo sequencial com 2 signatários):

1. Criou `E-Books-799` como rascunho, iniciou o fluxo, assinou como primeiro signatário (`darlam.oliveira@...`, 14:19).
2. Usou "Transferir Responsabilidade" apontando para `qa-20260721-transfer-test@example.invalid`. Resultado na tela: **um único** card de fluxo ("Fluxo E-Books-799"), agora com os dois signatários — o antigo (`Assinado`) e o novo (`Pendente`) — em vez dos dois fluxos desconectados que o comportamento antigo produzia. O botão "Transferir Responsabilidade" desapareceu da tela do executor, confirmando a correção do gate (`resolveCurrentResponsible`).
3. Confirmado pelo banco: `SignatureFlows` com uma única linha (`IsCompleted=false` neste ponto), dois `Signers` no mesmo `SignOrder=1`.
4. Logou como `qa-test` na própria interface (não simulado): viu a pendência, assinou.
5. Resultado final na tela: mesmo card "Fluxo E-Books-799", **Concluído**, "2 de 2 assinados", os dois signatários com `Assinado` e seus horários reais (14:19 e 14:23).
6. Confirmado pelo banco: `Documents.Status = Completed`, `SignatureFlows.IsCompleted = true`, `CompletedAt = 2026-07-21 14:23:55` — batendo exatamente com a tela.

Com isso, tanto a parte de backend (reabertura do mesmo fluxo, sem criar um segundo) quanto a de frontend (botão obsoleto corrigido) estão confirmadas em uso real, não só pela suíte de API.

## Documentos de teste residuais

`QA-20260721-TRANSFER-round6` e `QA-20260721-TRANSFER-midflow` ficaram no banco local, fora do status `Draft` (não deletáveis pela regra `DOC-014`), junto com as contas `qa-20260721-transfer-*@example.invalid` criadas para o teste.
