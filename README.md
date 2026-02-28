# Finanzas Personales - MVP 0

Local-first personal finance management system with .NET 8 backend and Vue 3 frontend.

## Prerequisites

- Docker Desktop
- Node.js 18+ and npm
- .NET 8 SDK (for local development)

## Quick Start

### 1. Start Database and API

```bash
docker compose up -d
```

This will start:
- PostgreSQL database on `localhost:5432`
- .NET API on `http://localhost:5080`
- Swagger UI at `http://localhost:5080/swagger`

### 2. Start Frontend

```bash
cd web
npm install
npm run dev
```

The Vue app will be available at `http://localhost:5173`

## Project Structure

```
/
├── docker-compose.yml
├── README.md
├── .gitignore
├── api/
│   ├── Dockerfile
│   └── src/App.Api/
│       ├── App.Api.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Data/
│       ├── Models/
│       └── Endpoints/
└── web/
    ├── package.json
    ├── vite.config.js
    └── src/
        ├── main.js
        ├── router.js
        ├── pages/
        ├── components/
        └── stores/
```

## API Endpoints

Base URL: `http://localhost:5080/api`

- `GET /api/expense-accounts` - List all accounts (filter: `?activeOnly=true`)
- `GET /api/expense-accounts/{id}` - Get account by ID
- `POST /api/expense-accounts` - Create new account
- `PUT /api/expense-accounts/{id}` - Update account
- `DELETE /api/expense-accounts/{id}` - Soft delete account

## Database Migrations

Migrations run automatically when the API container starts. To run manually:

```bash
cd api/src/App.Api
dotnet ef database update
```

## Development

### Run API locally (without Docker)

```bash
cd api/src/App.Api
dotnet run
```

### Run tests

```bash
cd api/tests
dotnet test
```

### Build for production

```bash
docker compose build
```

## Environment Variables

### API
- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string

### Frontend
- `VITE_API_BASE_URL` - API base URL (default: `http://localhost:5080`)

## Features (MVP 0)

- ✅ Configuration screen with tabs
- ✅ CRUD for Expense Accounts (Cuentas de gasto)
- ✅ Support for USD and ARS currencies
- ✅ Account types: Bank, Cash, Credit Card
- ✅ Soft delete functionality
- ✅ Form validation
- ✅ Success/error notifications

## Tech Stack

**Backend:**
- .NET 8 Minimal API
- Entity Framework Core
- PostgreSQL (Npgsql)
- Swagger/OpenAPI

**Frontend:**
- Vue 3 + Vite
- Vue Router
- Pinia (state management)
- Native CSS

**Infrastructure:**
- Docker Compose
- PostgreSQL 16
