# Q.A. do SignatureHub

Este diretório contém o primeiro entregável da fase de Q.A. do SignatureHub. Ele transforma os fluxos do sistema em cenários rastreáveis e mantém separados os fatos observados, as hipóteses de risco e os resultados obtidos em execução.

## Arquivos

- `MATRIZ-QA.md`: catálogo priorizado de cenários funcionais, negativos, de segurança e resiliência.
- `RISCOS-INICIAIS.md`: riscos levantados por revisão estática que precisam de confirmação dinâmica.
- `REGISTRO-EXECUCAO.md`: modelo para registrar ambiente, resultado, evidência e defeitos de cada rodada.

## Objetivo da primeira rodada

A primeira rodada deve responder, nesta ordem:

1. Um usuário não autenticado consegue ler ou alterar dados privados?
2. Um usuário autenticado consegue agir em nome de outro usuário, signatário ou departamento?
3. Um identificador obtido ou adivinhado é suficiente para consultar, assinar ou rejeitar um documento?
4. Falhas de e-mail, webhook, Hangfire ou storage alteram o resultado da operação principal?
5. Os estados e versões do documento permanecem consistentes sob repetição e concorrência?

## Perfis mínimos de teste

| Código | Perfil | Dados necessários |
|---|---|---|
| `ANON` | Não autenticado | Sem token, cookie ou credencial Windows |
| `USR-A` | Usuário comum, departamento A | Conta interna e/ou AD, e-mail confirmado no cadastro local |
| `USR-A2` | Outro usuário do departamento A | Conta distinta de `USR-A` |
| `USR-B` | Usuário comum, departamento B | Não participa dos documentos de `USR-A` |
| `ADMIN` | Administrador | Role `Admin` ou `Administrator` |
| `SIGN-A` | Signatário interno | E-mail associado a `USR-A2` |
| `SIGN-EXT` | Signatário externo | E-mail sem conta local, se esse fluxo for suportado |

Não reutilizar a mesma conta em perfis diferentes. Testes de isolamento exigem sujeitos e tokens realmente distintos.

## Massa de dados mínima

Criar dados descartáveis com um prefixo identificável, como `QA-20260720-`:

- Um PDF pequeno e válido.
- Um PDF acima do limite configurado.
- Arquivos `.txt`, `.docx`, extensão dupla e conteúdo incompatível com a extensão.
- Um certificado A1 válido de teste, um expirado e um protegido por senha incorreta. Nunca usar certificado pessoal ou de produção.
- Um documento público e um confidencial por departamento.
- Documentos em cada estado: rascunho, aguardando assinaturas, parcialmente concluído, concluído, rejeitado e expirado.
- Fluxos paralelo, sequencial e híbrido, incluindo duas pessoas na mesma etapa.
- Duas integrações externas pertencentes a usuários diferentes.

## Prioridade e severidade

| Nível | Uso na matriz | Tratamento de defeito |
|---|---|---|
| `P0` | Controle de acesso, identidade, assinatura e integridade documental | Bloqueia homologação se falhar |
| `P1` | Fluxos principais, privacidade e resiliência | Deve ser resolvido antes de produção, salvo aceite formal |
| `P2` | Validações, UX e compatibilidade | Pode ser planejado, desde que não amplifique risco |

Severidade do defeito:

- **Crítica:** acesso não autorizado amplo, assinatura/rejeição indevida, comprometimento de chaves ou perda de integridade documental.
- **Alta:** exposição relevante, escalada de privilégio, ação em nome de outro usuário ou indisponibilidade do fluxo principal.
- **Média:** comportamento incorreto com contorno viável, exposição limitada ou falha operacional recuperável.
- **Baixa:** problema cosmético ou de usabilidade sem impacto relevante sobre dados e decisões.

## Regras de execução

1. Executar primeiro todos os casos `P0`, depois `P1` e `P2`.
2. Para autorização, testar a API diretamente; esconder botões no Angular não é controle de acesso.
3. Registrar a requisição sem expor senha, JWT, refresh token, PIN, certificado ou segredo de webhook.
4. Em respostas inesperadas, preservar status HTTP, corpo sanitizado, horário, usuário e IDs dos dados de teste.
5. Repetir casos de mutação ao menos uma vez para detectar falta de idempotência.
6. Remover a massa de teste somente após anexar evidências e correlacionar logs.
7. Não modificar configuração ou implementação do Active Directory durante esta rodada.

## Critério de saída para produção

- Todos os `P0` executados e aprovados.
- Nenhum defeito crítico ou alto aberto sem mitigação e aceite formal.
- Segredos removidos do código, substituídos e validados no ambiente alvo.
- Migrações, snapshot do EF e histórico do banco reconciliados no ambiente de homologação.
- Build de backend e frontend aprovado.
- Caminho completo de criação, assinatura, rejeição, conclusão, download e verificação pública aprovado.
- Falhas simuladas de e-mail e webhook não revertem nem invalidam mutações do documento.
- Evidências e versão do commit testado registradas em `REGISTRO-EXECUCAO.md`.
