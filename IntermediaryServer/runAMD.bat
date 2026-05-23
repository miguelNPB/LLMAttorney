@echo off

cd /d "%~dp0"

:: Verificar si DockerEngine esta ejecutandose
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Docker no esta iniciado. Abre Docker Desktop y espera a que este listo y ejecuta el bat de nuevo.
    pause
    exit /b
)


:: Creamos el network para hablar con ollama desde el server
docker network create ollama-net

:: Limpiar contenedores previos para evitar errores de nombre ya en uso
docker rm -f ollama-server llmattorney-server >nul 2>&1

:: docker de ollama
echo Intentando iniciar Ollama con soporte de GPU (NVIDIA)...

:: Configuramos el server_config.json
SET "CONFIG_PATH=%~dp0\configs\server_config_AMD.json"
copy /y "%CONFIG_PATH%" "%~dp0\server_config.json" 
echo "%CONFIG_PATH%"

:: Obtenemos el nombre del modelo a usar
for /f "delims=" %%i in ('powershell -Command "(Get-Content '%CONFIG_PATH%' | ConvertFrom-Json).modelName"') do (
    set "MODEL_NAME=%%i"
)

if "%MODEL_NAME%"=="" (
    echo [ERROR] No se pudo leer el nombre del modelo del json de configuracion
    pause
    exit /b
)

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

echo Descargando el modelo "%MODEL_NAME%" (esto puede tardar la primera vez)...
:: Descargar el modelo principal a usar
docker exec ollama-server ollama pull "%MODEL_NAME%"

:: Descargar el modelo de embeddings
echo Descargando el modelo de embeddings
docker exec -it ollama-server ollama pull nomic-embed-text

:: Ejecutar el docker del servidor intermediario
echo Iniciando servidor Intermediario
docker build -t llmattorney .
docker run --rm -p 8000:8000 --network ollama-net -e OLLAMA_HOST=http://ollama-server:11434 --name llmattorney-server llmattorney

goto END

:TRY_LOCAL_OLLAMA

:: Verificar si ollama existe en PATH
where ollama >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Ollama no esta instalado en este sistema. Por favor instálalo desde: https://ollama.com/download
    exit /b
)

echo.
echo Iniciando Ollama local con soporte Vulkan...
echo.

start "" ollama serve

timeout /t 5 >nul

echo Descargando modelo (esto puede tardar la primera vez)...
ollama pull "%MODEL_NAME%"
echo Descargando el modelo de embeddings
ollama pull nomic-embed-text

echo Iniciando servidor Intermediario
docker build -t llmattorney .
docker run --rm -p 8000:8000 --network ollama-net -e OLLAMA_HOST=http://host.docker.internal:11434 --name llmattorney-server llmattorney

:END