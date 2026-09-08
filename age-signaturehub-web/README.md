# AGE SignatureHub Web

Frontend web do **AGE SignatureHub**, plataforma da Advocacia-Geral do Estado de Minas Gerais (AGE-MG) para gestão, tramitação, assinatura e verificação de documentos eletrônicos.

This is the web frontend for **AGE SignatureHub**, the Minas Gerais State Attorney General's Office (AGE-MG) platform for managing, routing, signing, and verifying electronic documents.

- [Português (Brasil)](#português-brasil)
- [English (United States)](#english-united-states)

---

## Português (Brasil)

### Escopo

O AGE SignatureHub centraliza o ciclo de vida de documentos que precisam de aprovação ou assinatura. A aplicação web oferece uma área pública para consulta de informações e verificação de autenticidade, além de uma área interna protegida para servidores e usuários autorizados.

O frontend contempla:

- autenticação por credenciais e sessão do Windows (SSO), com controle de acesso baseado em perfis;
- painel com indicadores, documentos recentes e notificações;
- envio, listagem, consulta, download, arquivamento e exclusão de documentos;
- definição de título, descrição, expiração, confidencialidade e unidade responsável;
- transferência de documentos entre usuários/unidades, com justificativa;
- criação e acompanhamento de fluxos de assinatura;
- participantes com papéis de assinante, aprovador, testemunha ou observador;
- assinatura eletrônica e suporte previsto nos contratos da API para certificados digitais A1/A3 e biometria;
- rejeição de solicitações de assinatura, com registro do motivo;
- consulta de pendências, histórico e trilha de auditoria;
- gestão de perfil, usuários e papéis administrativos;
- verificação pública de documentos por identificador e versão;
- páginas públicas de início, perguntas frequentes e política de privacidade.

### Como funciona

1. O usuário entra com suas credenciais ou com a sessão corporativa do Windows.
2. Após a autenticação, o frontend armazena a sessão e envia o token de acesso às chamadas da API.
3. Um documento é enviado com seus metadados e pode receber um fluxo com participantes, papéis e ordem de assinatura.
4. Cada participante assina, aprova, acompanha ou rejeita conforme seu papel e a etapa atual do fluxo.
5. O sistema atualiza o estado do documento, as pendências, as notificações e os registros de auditoria.
6. Documentos disponibilizados para consulta podem ser verificados na área pública por seu identificador e, quando aplicável, pela versão.

O navegador não se conecta diretamente ao banco de dados. Este frontend consome a API REST do SignatureHub; a API executa as regras de negócio e persiste os dados no PostgreSQL.

### Tecnologias

#### Frontend

- Angular 21 e Angular Router;
- Angular Material e Angular CDK;
- TypeScript 5.9;
- RxJS 7.8;
- SCSS;
- Angular SSR com servidor Express;
- `ngx-mask` para máscaras de entrada;
- `ng-icons` para a biblioteca de ícones;
- Vitest para testes unitários;
- npm para gerenciamento de dependências e scripts.

#### Integração e backend

- API REST em ASP.NET Core / .NET 9;
- autenticação JWT, renovação de token e integração com Windows Authentication/SSO;
- Entity Framework Core para acesso e mapeamento de dados;
- Npgsql como provedor do PostgreSQL;
- PostgreSQL como banco de dados relacional da solução;
- Swagger/OpenAPI para documentação da API.

### Arquitetura resumida

```text
Navegador
   │
   ▼
Angular 21 + Angular Material + SSR/Express
   │ HTTPS / JSON / multipart-form-data
   ▼
API REST ASP.NET Core (.NET 9)
   │ Entity Framework Core + Npgsql
   ▼
PostgreSQL
```

### Configuração e execução

#### Pré-requisitos

- Node.js compatível com Angular 21;
- npm 11 ou versão compatível;
- API do AGE SignatureHub em execução;
- PostgreSQL configurado para a API.

Instale as dependências:

```bash
npm install
```

A URL da API é definida em:

- `src/environments/environment.ts` para desenvolvimento;
- `src/environments/environment.prod.ts` para produção.

Inicie o servidor de desenvolvimento:

```bash
npm start
```

Acesse `http://localhost:4200/`. O servidor recarrega a aplicação quando os arquivos-fonte são alterados.

Gere o build de produção:

```bash
npm run build
```

Os artefatos são gravados em `dist/`. Como o projeto utiliza SSR, o servidor gerado pode ser iniciado com:

```bash
npm run serve:ssr:age-signaturehub-web
```

Execute os testes unitários:

```bash
npm test
```

> A criação e a migração do banco PostgreSQL são responsabilidades do projeto da API, localizado em `../server`; este repositório web contém apenas o cliente da aplicação e o servidor de renderização Angular.

---

## English (United States)

### Scope

AGE SignatureHub centralizes the lifecycle of documents that require approval or signature. The web application provides a public area for information and authenticity checks, as well as a protected internal area for staff and authorized users.

The frontend covers:

- credential-based and Windows session (SSO) authentication with role-based access control;
- a dashboard with metrics, recent documents, and notifications;
- document upload, listing, viewing, download, archiving, and deletion;
- title, description, expiration, confidentiality, and owning-department metadata;
- document transfers between users/departments with a required reason;
- signature-flow creation and tracking;
- participants acting as signers, approvers, witnesses, or observers;
- electronic signatures, with API contracts designed to support A1/A3 digital certificates and biometrics;
- signature-request rejection with a recorded reason;
- pending-task, history, and audit-trail views;
- profile, user, and administrative role management;
- public document verification by identifier and version;
- public home, FAQ, and privacy policy pages.

### How it works

1. The user signs in with credentials or a corporate Windows session.
2. After authentication, the frontend stores the session and attaches the access token to API requests.
3. A document is uploaded with its metadata and can be assigned a workflow containing participants, roles, and a signing order.
4. Each participant signs, approves, monitors, or rejects the request according to their role and the current workflow step.
5. The system updates the document status, pending tasks, notifications, and audit records.
6. Documents made available for consultation can be checked in the public area by identifier and, when applicable, version.

The browser does not connect directly to the database. This frontend consumes the SignatureHub REST API; the API applies business rules and persists data in PostgreSQL.

### Technologies

#### Frontend

- Angular 21 and Angular Router;
- Angular Material and Angular CDK;
- TypeScript 5.9;
- RxJS 7.8;
- SCSS;
- Angular SSR with an Express server;
- `ngx-mask` for input masks;
- `ng-icons` for icons;
- Vitest for unit testing;
- npm for dependency and script management.

#### Integration and backend

- ASP.NET Core / .NET 9 REST API;
- JWT authentication, token refresh, and Windows Authentication/SSO integration;
- Entity Framework Core for data access and mapping;
- Npgsql as the PostgreSQL provider;
- PostgreSQL as the solution's relational database;
- Swagger/OpenAPI for API documentation.

### Architecture overview

```text
Browser
   │
   ▼
Angular 21 + Angular Material + SSR/Express
   │ HTTPS / JSON / multipart-form-data
   ▼
ASP.NET Core REST API (.NET 9)
   │ Entity Framework Core + Npgsql
   ▼
PostgreSQL
```

### Setup and usage

#### Prerequisites

- a Node.js version compatible with Angular 21;
- npm 11 or a compatible version;
- a running AGE SignatureHub API;
- PostgreSQL configured for the API.

Install dependencies:

```bash
npm install
```

The API URL is configured in:

- `src/environments/environment.ts` for development;
- `src/environments/environment.prod.ts` for production.

Start the development server:

```bash
npm start
```

Open `http://localhost:4200/`. The development server reloads the application when source files change.

Create a production build:

```bash
npm run build
```

Build artifacts are written to `dist/`. Because the project uses SSR, the generated server can be started with:

```bash
npm run serve:ssr:age-signaturehub-web
```

Run unit tests:

```bash
npm test
```

> PostgreSQL database creation and migrations belong to the API project under `../server`; this web repository contains only the application client and the Angular rendering server.
