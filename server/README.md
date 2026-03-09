# AGE SignatureHub API

Sistema de gerenciamento de assinaturas digitais para a Advocacia-Geral do Estado de Minas Gerais (AGE-MG).

## Tecnologias

- **.NET 9.0** - Framework principal
- **PostgreSQL** - Banco de dados
- **Entity Framework Core** - ORM
- **MediatR** - CQRS Pattern
- **FluentValidation** - Validação
- **AutoMapper** - Mapeamento de objetos
- **Hangfire** - Background jobs
- **Serilog** - Logging
- **Swagger/OpenAPI** - Documentação da API
- **JWT** - Autenticação

## Arquitetura

O projeto segue os princípios da **Clean Architecture**:
```
AGE.SignatureHub/
├── AGE.SignatureHub.Domain/          # Entidades e regras de negócio
├── AGE.SignatureHub.Application/     # Casos de uso e DTOs
├── AGE.SignatureHub.Infrastructure/  # Implementações (DB, Email, Storage)
└── AGE.SignatureHub.API/             # Controllers e configurações
```

## Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/) (opcional)

## Instalação e Configuração

### 1. Clone o repositório
```bash
git clone https://github.com/age-mg/signaturehub.git
cd signaturehub
```

### 2. Configure o banco de dados

Atualize a connection string em `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=signaturehub;Username=postgres;Password=sua_senha"
}
```

### 3. Execute as migrations
```bash
cd AGE.SignatureHub.Infrastructure
dotnet ef database update --startup-project ../AGE.SignatureHub.API/AGE.SignatureHub.API.csproj
```

Ou use o script PowerShell:
```powershell
.\update-database.ps1
```

### 4. Execute a aplicação
```bash
cd AGE.SignatureHub.API
dotnet run
```

A API estará disponível em: `https://localhost:5001` ou `http://localhost:5000`

## Documentação da API

Acesse o Swagger UI em: `https://localhost:5001` (ou `http://localhost:5000`)

### Principais Endpoints

#### Documentos
- `POST /api/v1/documents` - Criar novo documento
- `GET /api/v1/documents/{id}` - Obter documento por ID
- `GET /api/v1/documents/by-status/{status}` - Listar por status
- `GET /api/v1/documents/{id}/download` - Baixar documento

#### Fluxos de Assinatura
- `POST /api/v1/signatureflows` - Criar fluxo de assinatura
- `GET /api/v1/signatureflows/{id}` - Obter fluxo por ID
- `GET /api/v1/signatureflows/by-document/{documentId}` - Listar fluxos do documento

#### Assinantes
- `GET /api/v1/signers/pending/{email}` - Listar assinaturas pendentes
- `POST /api/v1/signers/sign` - Assinar documento
- `POST /api/v1/signers/reject` - Rejeitar documento
- `GET /api/v1/signers/{id}` - Obter assinante por ID

#### Auditoria
- `GET /api/v1/auditlogs/document/{documentId}` - Logs do documento
- `GET /api/v1/auditlogs/date-range` - Logs por período

## Autenticação

A API utiliza **JWT Bearer Token**. Inclua o token no header:
```
Authorization: Bearer {seu_token_jwt}
```

## Funcionalidades Principais

### Gerenciamento de Documentos
- Upload de documentos para assinatura
- Versionamento automático
- Controle de expiração
- Download de versões específicas

### Fluxos de Assinatura
- Fluxo sequencial (ordem definida)
- Fluxo paralelo (todos assinam simultaneamente)
- Fluxo híbrido (combinação dos dois)
- Múltiplos signatários com diferentes papéis

### Tipos de Assinatura
- Assinatura eletrônica simples (PIN/OTP)
- Assinatura digital ICP-Brasil (A1/A3)
- Validação de certificados digitais
- Carimbo de tempo qualificado

### Notificações
- Email de solicitação de assinatura
- Lembretes automáticos
- Notificação de conclusão/rejeição
- Webhooks para integração

### Auditoria
- Log completo de todas as ações
- Rastreabilidade (IP, device, timestamp)
- Relatórios de assinaturas
- Exportação de trilha de auditoria

### Background Jobs
- Verificação de documentos expirados (a cada hora)
- Lembretes de assinatura pendente
- Limpeza de logs antigos (diariamente)

## Monitoramento

### Hangfire Dashboard
Acesse em: `https://localhost:5001/hangfire`

Visualize e gerencie:
- Jobs agendados
- Jobs recorrentes
- Histórico de execuções
- Filas de processamento

## Testes
```bash
# Executar todos os testes
dotnet test

# Executar testes com cobertura
dotnet test /p:CollectCoverage=true
```

## Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente (Development/Production) | Development |
| `ConnectionStrings__DefaultConnection` | String de conexão PostgreSQL | - |
| `Storage__Provider` | Provedor de storage (LocalFileSystem/AzureBlob) | LocalFileSystem |
| `Email__SmtpServer` | Servidor SMTP | smtp.gmail.com |
| `Email__SmtpPort` | Porta SMTP | 587 |

## Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença

Este projeto é propriedade da Advocacia-Geral do Estado de Minas Gerais (AGE-MG).

## Autores

- **Equipe de Desenvolvimento AGE-MG**

## Suporte

Para suporte, envie um email para desenvolvimento@advocaciageral.mg.gov.br