@echo off

cd /d "%~dp0"

:: 1. Verificar si DockerEngine esta ejecutandose
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker no esta iniciado. Abre Docker Desktop y espera a que este listo y ejecuta el bat de nuevo.
    pause
    exit /b
)


:: 2. Creamos el network para hablar con ollama desde el server
docker network create ollama-net

:: 3. Limpiar contenedores previos para evitar errores de nombre ya en uso
docker rm -f ollama-server llmattorney-server >nul 2>&1

:: 4. docker de ollama
echo Intentando iniciar Ollama con soporte de GPU (NVIDIA)...

:: Intentar con soporte de GPU
docker run -d --rm --gpus all -p 11434:11434 -v ollama:/root/.ollama --network ollama-net -e OLLAMA_HOST=0.0.0.0 --name ollama-server ollama/ollama

:: Fallback a cpu
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo El arranque con GPU fallo. Activando modo CPU...
    echo.
    
    docker run --rm -d -p 11434:11434 -v ollama:/root/.ollama --network ollama-net -e OLLAMA_HOST=0.0.0.0 --name ollama-server ollama/ollama
)

echo Descargando el modelo (esto puede tardar la primera vez)...
:: Cambia "llama3" por el modelo que quieras usar
docker exec ollama-server ollama pull llama3

:: 5. docker del server de python
docker build -t llmattorney .
docker run --rm -p 8000:8000 --network ollama-net --name llmattorney-server llmattorney


pause