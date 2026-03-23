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
docker run -d --rm --gpus all -p 11434:11434 -v ollama:/root/.ollama --network ollama-net -e OLLAMA_HOST=http://ollama-server:11434 --name ollama-server ollama/ollama

:: Fallback a VULKAN
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [WARNING] El arranque con Docker GPU fallo.
    echo Intentando usar Ollama local con Vulkan...
    echo.
    goto TRY_LOCAL_OLLAMA
)

echo Descargando el modelo (esto puede tardar la primera vez)...
:: Cambia "llama3" por el modelo que quieras usar
docker exec ollama-server ollama pull llama3
:: Descargando el modelo de embeddings
docker exec -it ollama-server ollama pull nomic-embed-text

docker exec ollama-server ollama list

docker build -t llmattorney .
docker run --rm -p 8000:8000 --network ollama-net -e OLLAMA_HOST=http://ollama-server:11434 --name llmattorney-server llmattorney

goto END

:TRY_LOCAL_OLLAMA

:: Verificar si ollama existe en PATH
where ollama >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Ollama no esta instalado en este sistema.
    echo Por favor instalalo desde:
    echo https://ollama.com/download
    echo.
    pause
    exit /b
)

echo.
echo Iniciando Ollama local con soporte Vulkan...
echo.

set OLLAMA_VULKAN=1
set OLLAMA_HOST=0.0.0.0:11434
start "" ollama serve

timeout /t 5 >nul

echo Descargando modelo llama3...
ollama pull llama3
echo Descargando el modelo de embeddings
ollama pull nomic-embed-text

echo.
echo Iniciando servidor Python...
echo.

docker build -t llmattorney .
docker run --rm -p 8000:8000 --network ollama-net -e OLLAMA_HOST=http://host.docker.internal:11434 --name llmattorney-server llmattorney

:END
pause