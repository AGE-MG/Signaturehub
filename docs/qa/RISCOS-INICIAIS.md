# Registro inicial de riscos

Data da revisão: **2026-07-20**  
Método: revisão estática exploratória do código atual.  
Estado: estes itens são **candidatos a defeito**, não resultados de teste dinâmico. Cada risco deve ser confirmado ou descartado pelos casos indicados antes de uma correção.

## Resumo

| ID | Severidade preliminar | Confiança | Risco | Casos de confirmação |
|---|---|---|---|---|
| RSK-001 | Crítica | Confirmado | Endpoints de auditoria estão acessíveis anonimamente | AUD-001 a AUD-004 |
| RSK-002 | Crítica | Parcialmente confirmado | Leitura de fluxos não exige autenticação; criação ainda não foi executada | SIGN-001, SIGN-002 |
| RSK-003 | Crítica | Parcialmente confirmado | Consultas de signatários alcançam os handlers anonimamente; assinatura/rejeição ainda não foram executadas | SIGN-008 a SIGN-012 |
| RSK-004 | Alta | Alta | Criador do documento é aceito do corpo enviado pelo cliente | DOC-002 |
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

### RSK-002 — Fluxos sem autenticação e autorização por objeto

`SignatureFlowController` não possui `[Authorize]`. O handler de criação busca o documento diretamente por ID e não valida criador, participação ou departamento. Uma exploração confirmada permitiria inserir signatários e alterar o fluxo de documento alheio.

**Confirmação dinâmica parcial em 2026-07-20:** as leituras anônimas `AUTH-001-B03` e `AUTH-001-B04` retornaram `404` e `200`, respectivamente, demonstrando que alcançaram os handlers. A criação anônima permanece não executada.

### RSK-003 — `SignerId` funcionando como credencial

`SignersController` não possui `[Authorize]`. Os DTOs de assinatura e rejeição recebem `SignerId`, mas não token de convite, prova de posse do e-mail ou identidade autenticada vinculada ao signatário. Há também consulta pública de pendências por e-mail. O GUID reduz adivinhação casual, porém não substitui autorização e pode vazar por URLs, logs e respostas.

**Confirmação dinâmica parcial em 2026-07-20:** `AUTH-001-B06` e `AUTH-001-B07` retornaram `400` e `404` sem JWT, demonstrando acesso anônimo aos handlers. Assinatura e rejeição anônimas permanecem não executadas.

### RSK-004 — Identidade do criador controlada pelo cliente

`CreateDocumentDto` inclui `CreatedByUserId`; o controller encaminha o valor recebido e o handler o usa para proprietário, departamento, auditoria e notificações. O servidor deveria derivar essa identidade exclusivamente do JWT.

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
