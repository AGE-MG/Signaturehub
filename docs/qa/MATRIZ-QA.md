# Matriz de cenários de Q.A.

Status inicial de todos os casos: **Não executado**. Os resultados devem ser anotados no `REGISTRO-EXECUCAO.md`; esta matriz permanece como catálogo estável.

Legenda de tipo: `F` funcional, `N` negativo, `S` segurança, `R` resiliência, `C` concorrência e `UX` interface/acessibilidade.

## Autenticação e sessão

| ID | Pri. | Tipo | Perfil | Cenário | Resultado esperado | Automação indicada |
|---|---:|---|---|---|---|---|
| AUTH-001 | P0 | S | ANON | Acessar endpoint protegido sem JWT | `401`; nenhum dado retornado | Integração API |
| AUTH-002 | P0 | S | USR-A | Usar JWT expirado | `401`, sem tolerância além da política definida | Integração API |
| AUTH-003 | P0 | S | USR-A | Alterar assinatura, issuer, audience ou claims do JWT | Token rejeitado em todos os casos | Integração API |
| AUTH-004 | P0 | S | USR-A | Reutilizar refresh token depois de uma renovação bem-sucedida | Token anterior rejeitado; rotação preservada | Integração API |
| AUTH-005 | P0 | S | USR-A | Fazer logout e tentar renovar/reutilizar a sessão | Refresh token revogado; acesso posterior negado | Integração API |
| AUTH-006 | P0 | S | USR-A | Enviar credenciais internas no modo AD e vice-versa | Roteamento correto, sem fallback silencioso indevido | Integração API |
| AUTH-007 | P0 | F/S | USR-A | Login AD por usuário/senha | Autentica, provisiona/mapeia corretamente e emite JWT | Manual em homologação AD |
| AUTH-008 | P0 | F/S | USR-A | Windows SSO com identidade válida | Autentica apenas a identidade Windows corrente | Manual em estação de domínio |
| AUTH-009 | P1 | N | USR-A | Windows SSO desabilitado na configuração | Resposta controlada, sem fallback ou vazamento de diagnóstico | Integração configurada |
| AUTH-010 | P1 | S | USR-A | Consultar diagnóstico AD informando login de outra pessoa | Política explícita aplicada; dados de terceiros não expostos a usuário comum | Manual/API |
| AUTH-011 | P0 | S | ANON | Repetir login inválido em alta frequência | Limitação/bloqueio observável sem degradar usuários legítimos | Segurança/API |
| AUTH-012 | P0 | S | ANON | Registrar conta local quando cadastro público não for permitido | Operação negada; se permitido, regra e auditoria confirmadas | Integração API |
| AUTH-013 | P1 | N | ANON | Registrar e-mail existente com variação de caixa/espaços | Não cria duplicidade nem permite enumeração excessiva | Integração API |
| AUTH-014 | P1 | S | USR-A | Inserir URL externa em `returnUrl` no login | Redirecionamento permanece na aplicação | E2E |
| AUTH-015 | P1 | S | USR-A | Inspecionar armazenamento do navegador após login/logout | Segredos não vazam; dados de sessão são removidos no logout | E2E/manual |
| AUTH-016 | P1 | F/S | ANON | Navegar pelo frontend para dashboard, documentos e assinaturas sem sessão | Rotas internas redirecionam para `/login` e não exibem conteúdo privado | E2E |

## Papéis, usuários e isolamento organizacional

| ID | Pri. | Tipo | Perfil | Cenário | Resultado esperado | Automação indicada |
|---|---:|---|---|---|---|---|
| ACL-001 | P0 | S | USR-A | Listar todos os usuários | `403`; somente administrador acessa | Integração API |
| ACL-002 | P0 | S | USR-A | Alterar as próprias roles por chamada direta | `403`; roles não mudam | Integração API |
| ACL-003 | P0 | S | ADMIN | Atribuir role inexistente ou privilegiada não permitida | Validação por allowlist e erro controlado | Integração API |
| ACL-004 | P0 | S | USR-A | Excluir outro usuário | `403`; alvo permanece ativo | Integração API |
| ACL-005 | P0 | F/S | ADMIN | Excluir a própria conta administrativa | Operação impedida ou política segura explicitamente confirmada | Integração API |
| ACL-006 | P1 | F | USR-A | Atualizar somente o próprio perfil | Apenas campos permitidos são alterados | Integração API |
| ACL-007 | P0 | S | USR-A | Alterar departamento/identificadores sincronizados pelo AD | Política de autoridade aplicada; sem ampliar acesso documental | Integração API |
| ACL-008 | P0 | S | USR-B | Listar documento não confidencial do departamento A | Documento não aparece | Integração API |
| ACL-009 | P0 | F/S | USR-A2 | Acessar documento não confidencial do mesmo departamento | Acesso conforme regra organizacional definida | Integração API |
| ACL-010 | P0 | S | USR-A2 | Acessar documento confidencial do mesmo departamento sem participação | `403` ou `404`; metadados não vazam | Integração API |
| ACL-011 | P0 | F/S | SIGN-A | Acessar documento no qual é signatário | Somente dados necessários e versões autorizadas | Integração API |
| ACL-012 | P0 | S | USR-B | Trocar IDs na URL/corpo para documento, usuário, notificação e integração de terceiros | Nenhuma leitura ou mutação de objeto alheio | Integração API |

## Documentos e arquivos

| ID | Pri. | Tipo | Perfil | Cenário | Resultado esperado | Automação indicada |
|---|---:|---|---|---|---|---|
| DOC-001 | P0 | F | USR-A | Criar PDF válido com dados mínimos | Documento criado como rascunho, hash e metadados persistidos | Integração API |
| DOC-002 | P0 | S | USR-A | Enviar `CreatedByUserId` de outro usuário | Servidor ignora/rejeita o valor e usa a identidade autenticada | Integração API |
| DOC-003 | P1 | N | USR-A | Enviar arquivo vazio | `400`; nenhum registro ou arquivo órfão | Integração API |
| DOC-004 | P1 | N/S | USR-A | Enviar arquivo acima do limite configurado | Rejeição antes da persistência; sem consumo excessivo | Integração API |
| DOC-005 | P1 | S | USR-A | Enviar executável renomeado como PDF ou extensão dupla | Tipo validado por conteúdo/política; arquivo não executável | Segurança/API |
| DOC-006 | P1 | N | USR-A | Título vazio, acima de 200 caracteres ou somente espaços | `400` com mensagem útil | Integração API |
| DOC-007 | P1 | N | USR-A | Expiração no passado, no instante atual e em fuso diferente | Datas inválidas rejeitadas de forma consistente em UTC | Integração API |
| DOC-008 | P0 | F/S | USR-A | Listar documentos por status | Somente documentos acessíveis e no status solicitado | Integração API |
| DOC-009 | P0 | S | USR-B | Obter ou baixar documento de outro departamento pelo GUID | `403`/`404`; conteúdo e nome não vazam | Integração API |
| DOC-010 | P0 | F | USR-A | Baixar original e cada versão assinada autorizada | Bytes, MIME, nome e hash correspondem à versão pedida | Integração API |
| DOC-011 | P1 | N | USR-A | Pedir versão inexistente, zero ou negativa | `404`/`400` controlado, sem fallback silencioso | Integração API |
| DOC-012 | P0 | F/S | USR-A | Excluir rascunho próprio | Soft delete consistente e documento deixa de ser acessível | Integração API |
| DOC-013 | P0 | S | USR-A2 | Excluir rascunho de outro usuário acessível pelo departamento | `403`; nada é alterado | Integração API |
| DOC-014 | P0 | N | USR-A | Excluir documento fora de rascunho | `400`; fluxo, signatários e arquivo permanecem | Integração API |
| DOC-015 | P1 | R | USR-A | Storage falhar durante upload | Operação falha sem registro parcial; tentativa pode ser refeita | Integração com fault injection |
| DOC-016 | P1 | R | USR-A | Banco falhar depois do upload | Não sobra arquivo inacessível ou existe rotina comprovada de limpeza | Integração com fault injection |
| DOC-017 | P0 | C | USR-A | Duas exclusões simultâneas do mesmo rascunho | Resultado determinístico, sem erro 500 ou eventos duplicados | Integração concorrente |
| DOC-018 | P0 | F/S | SIGN-A | Transferir documento para departamento de participante elegível | Transferência, auditoria e acesso resultante corretos | Integração API |
| DOC-019 | P0 | S | USR-B | Transferir documento sem ser criador/signatário autorizado | Operação negada | Integração API |
| DOC-020 | P1 | N | USR-A | Transferir para usuário não participante, mesmo departamento ou com motivo inválido | Regra de negócio aplicada e estado preservado | Integração API |

## Fluxos e assinaturas

| ID | Pri. | Tipo | Perfil | Cenário | Resultado esperado | Automação indicada |
|---|---:|---|---|---|---|---|
| SIGN-001 | P0 | S | ANON | Criar fluxo de assinatura sem autenticação | `401`; nenhum fluxo criado | Integração API |
| SIGN-002 | P0 | S | USR-B | Criar fluxo em documento alheio usando o GUID | `403`/`404`; documento não muda | Integração API |
| SIGN-003 | P0 | F | USR-A | Criar fluxo paralelo válido | Todos os signatários ficam aptos conforme regra | Integração API |
| SIGN-004 | P0 | F | USR-A | Criar fluxo sequencial válido | Somente a primeira etapa fica apta | Integração API |
| SIGN-005 | P0 | F | USR-A | Criar fluxo híbrido com duas pessoas na mesma etapa | Etapas e participantes são liberados corretamente | Integração API |
| SIGN-006 | P1 | N | USR-A | Criar fluxo sem signatário, com e-mail inválido, ordem zero ou enum inválido | `400`; nenhum efeito parcial | Integração API |
| SIGN-007 | P1 | N | USR-A | Repetir e-mail/documento ou usar ordens com lacunas | Política explícita aplicada; fluxo não fica inalcançável | Integração API |
| SIGN-008 | P0 | S | ANON | Consultar pendências usando e-mail de terceiro | Dados privados não enumeráveis sem prova de identidade | Integração API |
| SIGN-009 | P0 | S | ANON | Consultar signatário por GUID | Dados do signatário não retornam sem autorização/token do convite | Integração API |
| SIGN-010 | P0 | S | ANON | Assinar conhecendo apenas o `SignerId` | Operação negada sem autenticação ou token forte vinculado ao convite | Integração API |
| SIGN-011 | P0 | S | USR-B | Assinar como outro signatário autenticado | Operação negada; signatário permanece pendente | Integração API |
| SIGN-012 | P0 | S | ANON | Rejeitar conhecendo apenas o `SignerId` | Operação negada sem autenticação ou token forte | Integração API |
| SIGN-013 | P0 | F | SIGN-A | Assinar eletronicamente na etapa correta | Nova versão, auditoria, hash e estado atualizados atomicamente | Integração/E2E |
| SIGN-014 | P0 | N | SIGN-A | Assinar antes de sua etapa | Rejeição sem criar versão ou alterar estado | Integração API |
| SIGN-015 | P0 | N | SIGN-A | Assinar documento expirado, rejeitado, cancelado ou concluído | Rejeição e nenhuma alteração | Integração API |
| SIGN-016 | P0 | C | SIGN-A | Enviar duas assinaturas simultâneas para o mesmo signatário | Apenas uma vence; uma única versão e auditoria são criadas | Integração concorrente |
| SIGN-017 | P0 | C | SIGN-A | Dois signatários da mesma etapa assinam simultaneamente | Versões não colidem e a etapa avança uma única vez | Integração concorrente |
| SIGN-018 | P0 | F/N | SIGN-A | Assinar com A1 válido, senha errada, certificado expirado e identidade divergente | Somente certificado válido e autorizado é aceito | Integração controlada |
| SIGN-019 | P0 | F/N | SIGN-A | Solicitar A3 no ambiente/navegador suportado e não suportado | Capacidade real é informada; não há falsa confirmação de assinatura | Manual/E2E |
| SIGN-020 | P1 | N/S | SIGN-A | Enviar certificado/PIN excessivamente grandes | Limites aplicados; sem exaustão de memória e sem logar segredo | Segurança/API |
| SIGN-021 | P0 | F | SIGN-A | Rejeitar com motivo válido | Documento e demais signatários assumem estados previstos; auditoria criada | Integração/E2E |
| SIGN-022 | P1 | N | SIGN-A | Rejeitar sem motivo ou repetir rejeição | Rejeição inválida não altera estado; repetição é segura | Integração API |
| SIGN-023 | P0 | F | SIGN-A | Concluir última assinatura | Fluxo e documento concluem, versão final e QR são consistentes | Integração/E2E |
| SIGN-024 | P0 | R | SIGN-A | Storage ou assinatura criptográfica falhar no meio da operação | Transação revertida e artefatos parciais tratados | Fault injection |
| SIGN-025 | P1 | R | SIGN-A | Notificação/Hangfire falhar após assinatura persistida | Assinatura continua válida e resposta não induz repetição perigosa | Fault injection |

## Auditoria, notificações e integrações

| ID | Pri. | Tipo | Perfil | Cenário | Resultado esperado | Automação indicada |
|---|---:|---|---|---|---|---|
| AUD-001 | P0 | S | ANON | Consultar auditoria por documento | `401`; nenhum evento retornado | Integração API |
| AUD-002 | P0 | S | USR-B | Consultar auditoria de documento inacessível | `403`/`404`; detalhes não vazam | Integração API |
| AUD-003 | P0 | S | USR-A | Consultar auditorias globais por intervalo | Apenas perfil autorizado e escopo previsto | Integração API |
| AUD-004 | P1 | N/S | ADMIN | Usar intervalo invertido, enorme ou sem paginação | Validação/limite aplicado; serviço permanece responsivo | Integração API |
| AUD-005 | P0 | F | USR-A | Executar criação, fluxo, assinatura, rejeição, transferência e exclusão | Ações críticas possuem ator, alvo, instante e contexto corretos | Integração API |
| NOTIF-001 | P0 | S | USR-A | Marcar notificação pertencente a `USR-B` usando o GUID | Operação negada; notificação de `USR-B` permanece não lida | Integração API |
| NOTIF-002 | P1 | F | USR-A | Marcar uma e todas as próprias notificações | Somente notificações do usuário mudam | Integração API |
| NOTIF-003 | P1 | F | USR-A | Filtrar somente não lidas | Contagem e lista permanecem consistentes | Integração/E2E |
| NOTIF-004 | P1 | R/C | Sistema | Hangfire repetir o mesmo job após falha parcial | Não gera notificações internas duplicadas | Integração de job |
| NOTIF-005 | P0 | R | Sistema | SMTP indisponível em criação/assinatura/rejeição/conclusão | Operação principal permanece concluída e falha é observável | Fault injection |
| NOTIF-006 | P0 | R | Sistema | Webhook global retorna erro ou timeout | Operação principal e canal interno permanecem funcionais | Fault injection |
| EXT-001 | P0 | S | USR-A/USR-B | Listar, alterar, ativar e excluir integração do outro usuário pelo GUID | Sempre negado ou `404`; isolamento por `UserId` | Integração API |
| EXT-002 | P1 | F/S | USR-A | Criar integração e consultar novamente | Segredo aparece uma única vez e não reaparece em leitura/edição | Integração API |
| EXT-003 | P1 | N | USR-A | Nome duplicado, vazio, longo ou eventos inválidos | Rejeição sem criar/alterar conexão | Integração API |
| EXT-004 | P0 | S | USR-A | Usar HTTP, credenciais na URL, localhost, loopback, IP privado, IPv6 local e DNS que resolve internamente | Política SSRF bloqueia destinos não confiáveis na validação e no envio | Segurança/API |
| EXT-005 | P1 | F/S | Sistema | Receber webhook válido | Assinatura HMAC corresponde exatamente a `timestamp.payload` | Integração com receptor local controlado |
| EXT-006 | P1 | F | Sistema | Assinar evento não selecionado ou integração inativa | Nenhuma entrega ocorre | Integração de job |
| EXT-007 | P1 | R | Sistema | Uma integração falhar e outra responder | Falha fica isolada; demais integrações e e-mail são processados | Fault injection |
| EXT-008 | P1 | R/C | Sistema | Timeout e três retries do job | Sem duplicar notificações; status da última entrega é coerente | Integração de job |

## Verificação pública e privacidade

| ID | Pri. | Tipo | Perfil | Cenário | Resultado esperado | Automação indicada |
|---|---:|---|---|---|---|---|
| PUB-001 | P0 | F | ANON | Validar QR/URL de versão assinada existente | Hash, versão, estado e assinaturas correspondem ao artefato | Integração/E2E |
| PUB-002 | P0 | S | ANON | Consultar documento em rascunho, confidencial, rejeitado, excluído ou sem versão assinada | Somente estados publicáveis retornam dados | Integração API |
| PUB-003 | P0 | S | ANON | Enumerar GUIDs e versões | Respostas não facilitam enumeração nem vazam metadados privados | Segurança/API |
| PUB-004 | P1 | S/Priv. | ANON | Avaliar nome e e-mail dos signatários na resposta pública | Apenas dados estritamente necessários e autorizados são expostos | Revisão LGPD/API |
| PUB-005 | P0 | N | ANON | Alterar bytes do PDF e comparar com hash público | Alteração detectável; verificação não indica validade indevida | Manual/integração |
| PUB-006 | P1 | N | ANON | Pedir versão inexistente, zero, negativa ou de outro documento | `404`/`400` consistente, sem fallback para versão diferente | Integração API |

## Operação, configuração e experiência do usuário

| ID | Pri. | Tipo | Perfil | Cenário | Resultado esperado | Automação indicada |
|---|---:|---|---|---|---|---|
| OPS-001 | P0 | S | Operação | Iniciar aplicação sem chave JWT/issuer/audience | Falha rápida e diagnóstico sem revelar segredos | Teste de configuração |
| OPS-002 | P0 | S | Operação | Examinar repositório, artefatos e logs | Nenhuma senha, chave, token, PIN ou certificado real versionado/logado | SAST/secret scan |
| OPS-003 | P0 | F | Operação | Aplicar migrações em banco vazio e atualizar banco da versão anterior | Schema e snapshot convergem sem perda de dados | Pipeline de banco |
| OPS-004 | P1 | R | Operação | Reiniciar API e worker durante job | Jobs retomam com idempotência e estado coerente | Teste de recuperação |
| OPS-005 | P1 | S | ANON | Acessar Swagger e Hangfire fora de desenvolvimento | Swagger conforme política; Hangfire exige administrador | Integração de ambiente |
| OPS-006 | P1 | S | Site externo | Fazer requisição CORS com origem não permitida | Navegador bloqueia; credenciais não são liberadas | Segurança/E2E |
| UX-001 | P1 | UX | USR-A | Receber `401`, `403`, `404`, validação e indisponibilidade | Mensagem clara, sem stack trace e sem perder dados digitados | E2E |
| UX-002 | P2 | UX | USR-A | Navegar por teclado e leitor de tela nos fluxos críticos | Foco, rótulos, contraste e anúncios de erro utilizáveis | Manual/a11y scanner |
| UX-003 | P2 | UX | USR-A | Usar viewport móvel e conexão lenta | Layout utilizável, progresso e bloqueio contra duplo clique | E2E/manual |
| UX-004 | P1 | UX/S | USR-A | Abrir duas abas, expirar sessão e fazer logout em uma delas | Estado converge sem mostrar dados privados após logout | E2E |
| PERF-001 | P1 | R | USR-A | Listar grande volume de documentos, notificações e auditorias | Paginação/limite evita timeout e consumo excessivo | Performance API |
| PERF-002 | P1 | R | Vários | Executar logins e downloads concorrentes dentro da carga esperada | Latência e erros permanecem nos limites acordados | Teste de carga |
