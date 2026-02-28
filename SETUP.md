# Setup Guide - Finanzas Personales

## Complete Setup Instructions

### Step 1: Verify Prerequisites

Ensure you have installed:
- Docker Desktop (running)
- Node.js 18+ (`node --version`)
- .NET 8 SDK (`dotnet --version`)

### Step 2: Start Backend Services

From the project root:

```bash
docker compose up -d
```

This will:
1. Pull PostgreSQL 16 image
2. Build the .NET API image
3. Start both containers
4. Run database migrations automatically
5. Seed initial data (3 sample accounts)

Wait ~30 seconds for services to be fully ready.

### Step 3: Verify API is Running

Open your browser:
- API Health: http://localhost:5080/api/expense-accounts
- Swagger UI: http://localhost:5080/swagger

You should see 3 seeded accounts: Wise USD, Citi USD, Cash USD

### Step 4: Start Frontend

Open a new terminal and run:

```bash
cd web
npm install
npm run dev
```

The Vue app will start on http://localhost:5173

### Step 5: Test the Application

1. Navigate to http://localhost:5173
2. You'll be redirected to `/configuracion`
3. Click on "Cuentas de gasto" tab (should be active)
4. You should see the 3 seeded accounts
5. Try creating a new account
6. Try editing an existing account
7. Try deleting an account (soft delete)

## Troubleshooting

### API won't start
```bash
# Check logs
docker compose logs api

# Restart services
docker compose down
docker compose up -d
```

### Database connection issues
```bash
# Check if Postgres is running
docker compose ps

# Check Postgres logs
docker compose logs postgres
```

### Frontend can't connect to API
- Verify API is running on http://localhost:5080
- Check browser console for CORS errors
- Ensure `.env` file exists in `/web` (optional, defaults work)

### Port conflicts
If ports 5432, 5080, or 5173 are in use:
- Edit `docker-compose.yml` to change ports
- Update `VITE_API_BASE_URL` in web/.env
- Update `appsettings.json` connection string if needed

## Development Workflow

### Making API Changes

1. Edit code in `api/src/App.Api/`
2. Rebuild and restart:
   ```bash
   docker compose up -d --build api
   ```

### Making Frontend Changes

Hot reload is enabled. Just save your files in `web/src/` and changes appear instantly.

### Database Migrations

After modifying models:

```bash
cd api/src/App.Api
dotnet ef migrations add MigrationName
docker compose up -d --build api
```

### Viewing Database

Connect with any PostgreSQL client:
- Host: localhost
- Port: 5432
- Database: expenses
- User: finanzas
- Password: finanzas123

## Stopping Services

```bash
# Stop all containers
docker compose down

# Stop and remove volumes (deletes database)
docker compose down -v
```

## Production Build

### API
```bash
docker compose build api
```

### Frontend
```bash
cd web
npm run build
# Output in web/dist/
```

## Next Steps (Future Iterations)

- [ ] Add authentication
- [ ] Implement remaining configuration tabs
- [ ] Add Dashboard page
- [ ] Add transaction tracking
- [ ] Add reporting features
