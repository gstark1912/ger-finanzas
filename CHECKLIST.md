# Verification Checklist

Use this checklist to verify the complete setup is working correctly.

## ✅ Pre-flight Checks

- [ ] Docker Desktop is running
- [ ] Node.js 18+ is installed (`node --version`)
- [ ] .NET 8 SDK is installed (`dotnet --version`)
- [ ] Ports 5432, 5080, and 5173 are available

## ✅ Backend Verification

### 1. Start Services
```bash
docker compose up -d
```

- [ ] Postgres container starts successfully
- [ ] API container builds and starts successfully
- [ ] No errors in logs: `docker compose logs`

### 2. Database Check
- [ ] Database migrations ran automatically
- [ ] Seed data was inserted (3 accounts)
- [ ] Can connect to postgres://finanzas:finanzas123@localhost:5432/expenses

### 3. API Endpoints
Visit http://localhost:5080/api/expense-accounts

- [ ] Returns JSON array with 3 accounts
- [ ] Status code is 200 OK
- [ ] Accounts have: Wise USD, Citi USD, Cash USD

### 4. Swagger UI
Visit http://localhost:5080/swagger

- [ ] Swagger UI loads correctly
- [ ] Shows 5 endpoints for expense-accounts
- [ ] Can execute test requests from Swagger

### 5. API Tests
Run: `.\test-api.ps1`

- [ ] GET all accounts succeeds
- [ ] POST create account succeeds
- [ ] GET single account succeeds
- [ ] PUT update account succeeds
- [ ] DELETE account succeeds

## ✅ Frontend Verification

### 1. Install Dependencies
```bash
cd web
npm install
```

- [ ] All packages install without errors
- [ ] No vulnerability warnings (or acceptable)

### 2. Start Dev Server
```bash
npm run dev
```

- [ ] Vite starts on http://localhost:5173
- [ ] No compilation errors
- [ ] Hot reload is working

### 3. UI Navigation
Visit http://localhost:5173

- [ ] Redirects to /configuracion automatically
- [ ] Navigation bar displays correctly
- [ ] "Configuración" link is active (blue underline)

### 4. Configuration Page
- [ ] Page title shows "Configuración"
- [ ] Tab bar displays 5 tabs
- [ ] "Cuentas de gasto" tab is active
- [ ] Other tabs are disabled/grayed out

### 5. Expense Accounts Table
- [ ] Table displays with 5 columns: Nombre, Tipo, Moneda, Estado, Acciones
- [ ] Shows 3 seeded accounts
- [ ] Account types are translated (Bank → Banco, Cash → Efectivo)
- [ ] Active badges show green
- [ ] Edit and Delete buttons are visible

### 6. Create Account
Click "+ Agregar cuenta"

- [ ] Modal opens with form
- [ ] Form has fields: Nombre, Tipo, Moneda
- [ ] Tipo dropdown has: Banco, Efectivo, Tarjeta de Crédito
- [ ] Moneda dropdown has: USD, ARS
- [ ] Cancel button closes modal
- [ ] Create button submits form

Fill form and submit:
- [ ] Success notification appears (green)
- [ ] New account appears in table
- [ ] Modal closes automatically

### 7. Edit Account
Click "Editar" on any account

- [ ] Modal opens with existing values pre-filled
- [ ] Can modify all fields
- [ ] "Activa" checkbox is visible
- [ ] Save button updates account

Submit changes:
- [ ] Success notification appears
- [ ] Table updates with new values
- [ ] Modal closes

### 8. Delete Account
Click "Eliminar" on any account

- [ ] Confirmation dialog appears
- [ ] Clicking OK soft-deletes account
- [ ] Success notification appears
- [ ] Account removed from table (or marked inactive)

### 9. Error Handling
Try creating account with empty name:

- [ ] Error notification appears (red)
- [ ] Form validation prevents submission

Stop API (`docker compose stop api`) and try any operation:

- [ ] Error notification shows connection error
- [ ] UI doesn't crash

## ✅ Integration Tests

### Full Flow Test
1. [ ] Start with 3 seeded accounts
2. [ ] Create new account "Test Bank USD"
3. [ ] Edit it to "Test Bank ARS" with currency ARS
4. [ ] Delete it
5. [ ] Verify only 3 accounts remain
6. [ ] Refresh page - data persists

### Browser Console
Open DevTools (F12):

- [ ] No JavaScript errors
- [ ] No CORS errors
- [ ] API calls show in Network tab
- [ ] Responses are valid JSON

## ✅ Code Quality Checks

### Backend
- [ ] All files compile without warnings
- [ ] Migrations folder exists with InitialCreate
- [ ] Swagger annotations are present
- [ ] CORS is configured
- [ ] Connection string uses environment variable

### Frontend
- [ ] No ESLint errors (if configured)
- [ ] All components use Composition API
- [ ] Pinia store is properly structured
- [ ] Router redirects work
- [ ] CSS follows POC design

## ✅ Documentation

- [ ] README.md has quick start instructions
- [ ] SETUP.md has detailed setup guide
- [ ] PROJECT.md has complete overview
- [ ] All code has English comments
- [ ] API endpoints are documented

## ✅ Docker

- [ ] `docker compose ps` shows 2 running containers
- [ ] `docker compose logs api` shows no errors
- [ ] `docker compose logs postgres` shows database ready
- [ ] Containers restart automatically
- [ ] Data persists after `docker compose restart`

## 🎉 Final Verification

If all items are checked:
- ✅ MVP 0 is complete and working
- ✅ Ready for development
- ✅ Ready for next iteration

## 🐛 Troubleshooting

If any check fails, see SETUP.md "Troubleshooting" section.

Common issues:
- Port conflicts → Change ports in docker-compose.yml
- API not connecting → Check Docker logs
- Frontend errors → Clear node_modules and reinstall
- Database issues → `docker compose down -v` and restart
