# Finanzas Personales - MVP 0 Complete

## 🎯 Project Overview

A local-first personal finance management system with bimonetary support (USD/ARS), built as a monorepo with .NET 8 backend and Vue 3 frontend.

## ✅ Delivered Features

### Backend (.NET 8 Minimal API)
- ✅ RESTful API with 5 endpoints for ExpenseAccount CRUD
- ✅ Entity Framework Core with PostgreSQL
- ✅ Database migrations with seed data
- ✅ Soft delete functionality
- ✅ Input validation and proper HTTP status codes
- ✅ DTOs for request/response (entities not exposed)
- ✅ Swagger/OpenAPI documentation
- ✅ CORS enabled for local development
- ✅ Docker containerization with auto-migration on startup

### Frontend (Vue 3 + Vite)
- ✅ Configuration page with tab navigation
- ✅ "Cuentas de gasto" tab fully implemented
- ✅ CRUD operations with modal forms
- ✅ Data table with sorting and filtering
- ✅ Form validation
- ✅ Success/error notifications
- ✅ Pinia state management
- ✅ Vue Router with redirect from root
- ✅ Clean CSS styling based on POC design
- ✅ Responsive layout

### Infrastructure
- ✅ Docker Compose with Postgres + API
- ✅ Environment variable configuration
- ✅ Health checks for database
- ✅ Volume persistence for database
- ✅ Automated startup scripts

## 📁 Project Structure

```
ger-finanzas/
├── api/
│   ├── Dockerfile
│   └── src/App.Api/
│       ├── Data/AppDbContext.cs
│       ├── Models/ExpenseAccount.cs
│       ├── Endpoints/ExpenseAccountEndpoints.cs
│       ├── Migrations/
│       ├── Program.cs
│       └── *.csproj, appsettings.json
├── web/
│   ├── src/
│   │   ├── components/
│   │   │   ├── AccountModal.vue
│   │   │   └── Notification.vue
│   │   ├── pages/
│   │   │   └── ConfiguracionPage.vue
│   │   ├── stores/
│   │   │   └── expenseAccount.js
│   │   ├── App.vue
│   │   ├── main.js
│   │   ├── router.js
│   │   └── style.css
│   ├── index.html
│   ├── package.json
│   └── vite.config.js
├── docker-compose.yml
├── README.md
├── SETUP.md
├── .gitignore
├── start.bat
└── test-api.ps1
```

## 🚀 Quick Start

### Option 1: Using start.bat (Windows)
```bash
start.bat
```

### Option 2: Manual
```bash
# Terminal 1: Start backend
docker compose up -d

# Terminal 2: Start frontend
cd web
npm install
npm run dev
```

### Access Points
- Frontend: http://localhost:5173
- API: http://localhost:5080/api/expense-accounts
- Swagger: http://localhost:5080/swagger
- Database: localhost:5432

## 🗄️ Database Schema

### Table: expense_accounts
| Column | Type | Constraints |
|--------|------|-------------|
| id | uuid | PRIMARY KEY |
| name | varchar(120) | NOT NULL |
| type | varchar | NOT NULL (Bank/Cash/CC) |
| currency | varchar | NOT NULL (USD/ARS) |
| is_active | boolean | NOT NULL, DEFAULT true |
| created_at | timestamp | NOT NULL |
| updated_at | timestamp | NOT NULL |

### Seed Data
- Wise USD (Bank, USD)
- Citi USD (Bank, USD)
- Cash USD (Cash, USD)

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/expense-accounts | List all accounts (filter: ?activeOnly=true) |
| GET | /api/expense-accounts/{id} | Get account by ID |
| POST | /api/expense-accounts | Create new account |
| PUT | /api/expense-accounts/{id} | Update account |
| DELETE | /api/expense-accounts/{id} | Soft delete account |

### Request/Response Examples

**POST /api/expense-accounts**
```json
{
  "name": "Santander USD",
  "type": "Bank",
  "currency": "USD"
}
```

**Response (201 Created)**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Santander USD",
  "type": "Bank",
  "currency": "USD",
  "isActive": true,
  "createdAt": "2024-02-28T01:22:24.123Z",
  "updatedAt": "2024-02-28T01:22:24.123Z"
}
```

## 🧪 Testing

Run the PowerShell test script:
```powershell
.\test-api.ps1
```

Or test manually with Swagger UI at http://localhost:5080/swagger

## 🎨 Design System

Based on POC files in `/POC/` folder:
- Clean, minimal design
- Apple-inspired typography
- Blue accent color (#3498db)
- Subtle shadows and borders
- Responsive tables and forms
- Modal-based CRUD operations

## 🔧 Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Backend | .NET | 8.0 |
| Backend Framework | Minimal API | - |
| ORM | Entity Framework Core | 8.0 |
| Database | PostgreSQL | 16 |
| Frontend | Vue | 3.4 |
| Build Tool | Vite | 5.0 |
| State Management | Pinia | 2.1 |
| Routing | Vue Router | 4.2 |
| Container | Docker | - |
| Orchestration | Docker Compose | 3.8 |

## 📝 Development Notes

### Code Quality
- Minimal, clean implementation
- English comments and naming
- Consistent conventions
- No unnecessary abstractions
- DTOs separate from entities
- Proper error handling

### Security
- CORS configured for local dev
- Input validation on all endpoints
- SQL injection protected (EF Core)
- No credentials in code

### Performance
- Async/await throughout
- Efficient EF Core queries
- Minimal API overhead
- Hot reload for frontend

## 🔜 Next Steps (Future MVPs)

1. **Authentication & Authorization**
   - User login/registration
   - JWT tokens
   - Protected routes

2. **Additional Configuration Tabs**
   - Tipo de cambio mensual
   - Tarjetas
   - Categorías de gastos
   - Objetivos de inversión

3. **Core Features**
   - Dashboard with KPIs
   - Gastos Fijos tracking
   - Tarjeta Visa management
   - Caja USD buckets
   - Inversiones tracking

4. **Enhancements**
   - Search and filtering
   - Bulk operations
   - Export to CSV/Excel
   - Charts and visualizations
   - Mobile responsive improvements

## 📄 License

Private project - All rights reserved

## 👤 Author

Created following the specifications in `initial-prompt.txt`

---

**Status**: ✅ MVP 0 Complete and Ready to Run
**Last Updated**: February 2024
