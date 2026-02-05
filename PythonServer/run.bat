@echo off

cd /d "%~dp0"

:: 1. Verificar si DockerEngine esta ejecutandose
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker no esta iniciado. Abre Docker Desktop y espera a que este listo y ejecuta el bat de nuevo.
    pause
    exit /b
)

:: 2. Crea el docker del server de python
docker build -t llmattorney .
docker run -p 8000:8000 llmattorney

::docker compose up -d

pause