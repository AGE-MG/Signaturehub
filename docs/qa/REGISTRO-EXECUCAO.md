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
