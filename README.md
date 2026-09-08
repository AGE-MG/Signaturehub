# AGE SignatureHub

Plataforma da Advocacia-Geral do Estado de Minas Gerais (AGE-MG) para gestão, tramitação, assinatura e verificação de documentos eletrônicos.

Platform for the Minas Gerais State Attorney General's Office (AGE-MG) to manage, route, sign, and verify electronic documents.

## Documentação / Documentation

Escolha o componente para acessar sua documentação detalhada:

Choose a component to open its detailed documentation:

| Componente / Component | Descrição / Description | README |
| --- | --- | --- |
| Frontend | Aplicação Angular, interface, fluxos do usuário, configuração e execução. / Angular application, user interface, workflows, setup, and usage. | [Frontend README](./age-signaturehub-web/README.md) |
| Backend | API ASP.NET Core, regras de negócio, banco de dados, endpoints e infraestrutura. / ASP.NET Core API, business rules, database, endpoints, and infrastructure. | [Backend README](./server/README.md) |

- [Português (Brasil)](#português-brasil)
- [English (United States)](#english-united-states)

---

## Português (Brasil)

### Visão geral

O AGE SignatureHub centraliza o ciclo de vida de documentos que precisam de aprovação ou assinatura. A solução combina uma aplicação web responsiva com uma API responsável pelas regras de negócio, autenticação, persistência, auditoria e integrações.

Entre os principais recursos estão:

- autenticação por credenciais, JWT e sessão corporativa do Windows (SSO);
- controle de acesso baseado em papéis;
- envio, consulta, download, transferência e arquivamento de documentos;
- fluxos sequenciais, paralelos ou híbridos de assinatura;
- participantes com papéis de assinante, aprovador, testemunha e observador;
- acompanhamento de pendências, estados e notificações;
- histórico completo e trilha de auditoria;
- verificação pública de documentos;
- gestão de usuários, perfis e unidades responsáveis.

### Arquitetura

```text
Usuário
  │
  ▼
Frontend Angular 21 + Angular Material + SSR/Express
  │  API REST — HTTPS, JSON e multipart/form-data
  ▼
Backend ASP.NET Core / .NET 9
  │  Entity Framework Core + Npgsql
  ▼
PostgreSQL
```

O frontend nunca acessa o banco diretamente. Ele consome a API REST, que aplica as regras de negócio e persiste os dados no PostgreSQL.

### Tecnologias principais

- **Frontend:** Angular 21, Angular Material, TypeScript, RxJS, SCSS, Angular SSR, Express e Vitest.
- **Backend:** ASP.NET Core, .NET 9, Entity Framework Core, MediatR, FluentValidation, AutoMapper, Hangfire, Serilog e Swagger/OpenAPI.
- **Banco de dados:** PostgreSQL por meio do provedor Npgsql.
- **Segurança:** JWT Bearer, renovação de token, Windows Authentication/SSO e controle de acesso baseado em papéis.

### Estrutura do repositório

```text
Signaturehub/
├── age-signaturehub-web/   # Frontend Angular e servidor SSR
├── server/                 # API, domínio, aplicação e infraestrutura
├── docs/                   # Documentação complementar e materiais de QA
└── README.md               # Visão geral da solução
```

### Primeiros passos

Cada componente possui pré-requisitos, configuração e comandos próprios:

1. Configure o PostgreSQL e inicie a API seguindo o [README do backend](./server/README.md).
2. Configure a URL da API e inicie a aplicação seguindo o [README do frontend](./age-signaturehub-web/README.md).

---

## English (United States)

### Overview

AGE SignatureHub centralizes the lifecycle of documents that require approval or signature. The solution combines a responsive web application with an API responsible for business rules, authentication, persistence, auditing, and integrations.

Its main capabilities include:

- credential, JWT, and corporate Windows session (SSO) authentication;
- role-based access control;
- document upload, viewing, download, transfer, and archiving;
- sequential, parallel, or hybrid signature workflows;
- participants acting as signers, approvers, witnesses, or observers;
- pending-task, status, and notification tracking;
- complete history and audit trails;
- public document verification;
- user, profile, and owning-department management.

### Architecture

```text
User
  │
  ▼
Angular 21 + Angular Material + SSR/Express frontend
  │  REST API — HTTPS, JSON, and multipart/form-data
  ▼
ASP.NET Core / .NET 9 backend
  │  Entity Framework Core + Npgsql
  ▼
PostgreSQL
```

The frontend never accesses the database directly. It consumes the REST API, which applies business rules and persists data in PostgreSQL.

### Core technologies

- **Frontend:** Angular 21, Angular Material, TypeScript, RxJS, SCSS, Angular SSR, Express, and Vitest.
- **Backend:** ASP.NET Core, .NET 9, Entity Framework Core, MediatR, FluentValidation, AutoMapper, Hangfire, Serilog, and Swagger/OpenAPI.
- **Database:** PostgreSQL through the Npgsql provider.
- **Security:** JWT Bearer, token refresh, Windows Authentication/SSO, and role-based access control.

### Repository structure

```text
Signaturehub/
├── age-signaturehub-web/   # Angular frontend and SSR server
├── server/                 # API, domain, application, and infrastructure
├── docs/                   # Additional documentation and QA materials
└── README.md               # Solution overview
```

### Getting started

Each component has its own prerequisites, configuration, and commands:

1. Configure PostgreSQL and start the API by following the [backend README](./server/README.md).
2. Configure the API URL and start the application by following the [frontend README](./age-signaturehub-web/README.md).

## Ownership

Este projeto é de propriedade da Advocacia-Geral do Estado de Minas Gerais (AGE-MG).

This project is owned by the Minas Gerais State Attorney General's Office (AGE-MG).
