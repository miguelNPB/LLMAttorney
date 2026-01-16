@echo off

cd /d "%~dp0"

:: Crea el docker del server de python
docker build -t llmattorney .
docker run -p 8000:8000 llmattorney

docker compose up -d