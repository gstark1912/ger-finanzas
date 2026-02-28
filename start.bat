@echo off
echo Starting Finanzas Personales...
echo.
echo Starting Docker services (DB + API)...
docker compose up -d
echo.
echo Waiting for services to be ready...
timeout /t 10 /nobreak > nul
echo.
echo Services started!
echo - API: http://localhost:5080
echo - Swagger: http://localhost:5080/swagger
echo - Database: localhost:5432
echo.
echo To start the frontend, run:
echo   cd web
echo   npm install
echo   npm run dev
echo.
echo Frontend will be available at: http://localhost:5173
pause
