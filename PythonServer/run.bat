@echo off

cd /d "%~dp0"

docker build -t llmattorney .
docker run -p 8000:8000 llmattorney


docker compose up -d
docker exec -it ollama_service ollama run llama3:8b