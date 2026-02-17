from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from typing import Optional, Dict, Any
import httpx
import requests
import json
import guidance
from guidance import models, gen, select
from guidance import json as gen_json
import asyncio

'''
'''
# --- Constantes

GEMINI_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key="
OLLAMA_ENDPOINT_NVIDIA = "http://ollama-server:11434/v1"
OLLAMA_ENDPOINT_AMD_VULKAN = "http://host.docker.internal:11434/v1"
OLLAMA_USE_VULKAN = True

# ---

# Abrimos apikey de gemini
try:
    with open("./Gemini_APIKEY.txt", "r", encoding="utf-8") as archivo:
        Gemini_APIKEY = archivo.read()
except FileNotFoundError:
    raise Exception(f"El archivo Gemini_APIKEY.txt no fue encontrado, crearlo y meter dentro la APIKEY de gemini")

# se crea API
app = FastAPI(title="LLMAttorney Server")

# --- 

# estructura de json de entrada, pide un campo prompt que es un string
class Query(BaseModel):
    mode: str
    prompt: str
    LLMConfig: str
    temperature: float # 0 = Creativo 1 = Estricto
    max_length: int # Longitud maxima del output
    # Nuevo campo: Recibe el esquema JSON deseado
    # Si es None, funciona en modo texto normal
    json_schema: Optional[Dict[str, Any]] = None


# ---

# Crea la query y la ejecuta para Gemini
async def sendGeminiQuery(prompt, LLMConfig, temperature, max_length, json_schema=None):
    # Configuración base
    generation_config = {
        "temperature": temperature * 2, # Gemini usa escala 0.0 - 2.0
        "maxOutputTokens": max_length,
    }

    # Si hay esquema, activamos modo json
    if json_schema:
        generation_config["responseMimeType"] = "application/json"
        generation_config["responseSchema"] = json_schema

    payload = {
        "systemInstruction": {
            "parts": [{"text": LLMConfig}]
        },
        "generationConfig": generation_config,
        "contents": [
            {
                "parts": [{"text": prompt}]
            }
        ]
    }

    async with httpx.AsyncClient() as client:
        # Aumentamos timeout porque generar JSON complejo puede tardar unos segundos
        response = await client.post(GEMINI_ENDPOINT + Gemini_APIKEY, json=payload, timeout=60.0) 
        
        if response.status_code != 200:
            # Para ver el error real de Google si falla
            raise HTTPException(status_code=response.status_code, detail=response.text)
        
        data = response.json()
        response_raw = data["candidates"][0]["content"]["parts"][0]["text"]        
        if json_schema:
            json_limpio = json.loads(response_raw) # Convierte el texto de Gemini en objeto Python
            return json_limpio
        else:
            return { "answer": response_raw }

# Crea la query y la ejecuta para Ollama
async def sendLlamaQuery(prompt, LLMConfig, temperature, max_length, json_schema=None):
    lm = models.OpenAI(
        model="llama3",  # Modelo Ollama
        base_url= OLLAMA_ENDPOINT_AMD_VULKAN if OLLAMA_USE_VULKAN else OLLAMA_ENDPOINT_NVIDIA, 
        api_key="ollama"
    )

    with guidance.system():
        lm += LLMConfig

    with guidance.user():
        lm += prompt

    if json_schema:
        with guidance.assistant():
            lm += gen_json("result", schema=json_schema, temperature=temperature)

    return json.loads(lm['result'])

# endpoint principal
@app.post("/ask")
async def ask_LLMAttorney(query: Query):
    try:
        if query.mode == "Gemini":
            answer = await sendGeminiQuery(query.prompt, query.LLMConfig, query.temperature, query.max_length, query.json_schema)
        elif query.mode == "Llama":
            answer = await sendLlamaQuery(query.prompt, query.LLMConfig, query.temperature, query.max_length, query.json_schema)
        else:
            raise HTTPException(status_code=400)
        return answer
    except httpx.HTTPStatusError as e:
        raise HTTPException(status_code=e.response.status_code, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))