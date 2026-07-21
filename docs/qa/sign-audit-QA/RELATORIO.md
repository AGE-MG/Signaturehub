# Q.A. focado — Assinatura e Histórico de Auditoria

Origem: teste manual do executor, autenticado via Active Directory (`darlam.oliveira@advocaciageral.mg.gov.br`), em 2026-07-21 por volta de 12:04–12:15. Sequência: login AD (passou) → histórico de atividades (falhou) → criar documento (passou) → iniciar fluxo (passou) → assinar (erro inesperado).

Commit base da investigação: `35a4f82`. Nenhuma das duas causas exigiu voltar a um commit anterior — ambas foram diagnosticadas a partir do log da própria API (`server/AGE.SignatureHub.API/Logs/log-20260721.txt`) e de consulta direta ao banco local.

## 1. Histórico de atividades — `403 Forbidden`

### Causa

Regressão introduzida na rodada de correções anterior (`QA-2026-07-21-04`). Ao corrigir `RSK-001`/`BUG-2026-005` (auditoria acessível sem escopo), a rota `GET /api/v1/auditlogs/date-range` — usada pela tela "Histórico" do Angular (`age-signaturehub-web/src/app/features/internal/history/history.component.ts`) — foi restrita com `[Authorize(Roles = "Admin,Administrator")]`. Usuários provisionados via AD recebem apenas a role `User` (`AuthService.SignInWithActiveDirectoryAsync`, `AddToRoleAsync(user, "User")`), então qualquer usuário comum passou a receber `403` ao abrir a tela — não só o solicitante original de `RSK-001`/`RSK-004`, mas qualquer pessoa da organização.

### Correção

Trocada a exigência de role por escopo, no mesmo padrão já usado em `DocumentsController`:

- `AuditLogsController.GetAuditLogsByDateRange`: removida a restrição de role; volta a bastar `[Authorize]` simples, mas agora envia `RequestingUserId`, `RequestingUserEmail`, `RequestingUserDepartment` e `IsAdmin` (`User.IsInRole("Admin") || User.IsInRole("Administrator")`) na query.
- `GetAuditLogsByDateRangeQueryHandler`: se `IsAdmin`, comportamento inalterado (vê tudo). Caso contrário, filtra a lista para eventos onde `UserId == RequestingUserId` **ou** `DocumentId` pertence a um documento acessível ao usuário (reaproveita `IUnitOfWork.Documents.GetAccessibleDocumentsAsync`, a mesma regra de criador/signatário/departamento não confidencial já usada na listagem de documentos).

### Verificação

Testado com conta `QA-20260721-usr-a` (role `User`, não admin):

| Antes da correção | Depois da correção |
|---|---|
| `403 Forbidden` | `200 OK`, 31 eventos retornados |

Confirmado que os eventos de `USR-B` (usuário sem relação) **não** aparecem nos resultados de `USR-A` — o escopo funciona, não é um retrocesso para o vazamento original. `AUTH-001` completo repetido (31/31 aprovado): a rota continua exigindo autenticação, só não exige mais role de administrador.

## 2. Assinar documento — "erro inesperado"

### Causa

**Não relacionada às correções de autorização desta sessão.** O e-mail do JWT do usuário AD (`darlam.oliveira@advocaciageral.mg.gov.br`) confere exatamente com o e-mail do signatário criado no flutxo, então a checagem de identidade (`isOwningUser`) passou normalmente. O erro real, capturado no log da API:

```
[2026-07-21 12:14:53 ERR] AGE.SignatureHub.Infrastructure.Services.Signature.SignatureService - Error signing document electronically.
iText.Kernel.Exceptions.BadPasswordException: PdfReader is not opened with owner password
```

É um defeito pré-existente em `SignatureService` (`server/AGE.SignatureHub.Infrastructure/Services/Signature/SignatureService.cs`): ao abrir o PDF original com `iText.Kernel.Pdf.PdfReader` para aplicar o rodapé de verificação/assinatura, a biblioteca recusa a escrita quando o arquivo tem **restrições de permissão configuradas com senha de proprietário, mesmo sem senha de abertura** — um padrão comum em PDFs exportados de Word/LibreOffice/scanners com "restringir edição" habilitado. O código nunca chamava `PdfReader.SetUnethicalReading(true)` (a API do próprio iText para permitir a escrita nesses casos), então qualquer documento exportado dessa forma quebrava a assinatura eletrônica — e também quebraria a assinatura com certificado digital A1/A3, que usa o mesmo padrão de leitura em `SignWithCertficateAsync`/`PrepareVisualPdfForCertificateAsync`.

### Correção

Adicionado `pdfReader.SetUnethicalReading(true)` logo após a criação de cada `PdfReader` usado para **escrever/modificar** o PDF original (4 pontos em `SignatureService.cs`): `SignEletronicallyAsync`, `PrepareVisualPdfForCertificateAsync`, e os dois leitores de `SignWithCertficateAsync` (contagem de páginas e assinatura com certificado). O leitor usado em `ValidateSignatureAsync` (validação de assinatura pública, sobre documentos que o próprio sistema já gerou) não foi alterado — não é o caminho afetado e opera só em leitura.

### Verificação

Reproduzido o cenário exato fora do navegador (não tenho como logar como AD/SSO por aqui): gerei um PDF com senha de proprietário e sem senha de usuário (mesmo padrão de restrição), criei um documento e um fluxo com ele, e assinei via API.

| Antes da correção (log de 12:14–12:15) | Depois da correção (reprodução) |
|---|---|
| `500`, `BadPasswordException: PdfReader is not opened with owner password` | `200`, assinatura registrada (`status: Signed`, `signedAt` preenchido) |

Nenhuma nova ocorrência de `BadPasswordException` no log após a correção.

**Pendente de confirmação sua:** não consigo repetir sua sessão real (autenticação AD/SSO exige o navegador/domínio). O documento original do seu teste (`stt_de_en`, id `48e72aa8-1ef0-47a3-bb56-ea66eb465273`) continua em `PendingSignatures` no banco local — se puder tentar assinar de novo pela interface agora que o backend foi reiniciado com a correção, isso fecha a confirmação real. Se falhar de novo, quero o erro exato (a mensagem que aparece na tela e, se possível, o horário, para eu cruzar com o log).

## Resumo

| Item | Causa | Correção | Status |
|---|---:|---|---|
| Histórico de atividades (`403`) | Regressão da rodada anterior — Admin-only era restritivo demais | Escopo por acesso a documento, como no restante do sistema | Corrigido e verificado |
| Assinar documento (erro inesperado) | Bug pré-existente do iText com PDFs com restrição de permissão | `SetUnethicalReading(true)` nos leitores de escrita do `SignatureService` | Corrigido e verificado por reprodução; falta confirmação no seu ambiente real |

Nada foi commitado ainda nesta rodada.
