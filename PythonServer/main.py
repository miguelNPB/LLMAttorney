from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import httpx
import requests


# Abrimos apikey de gemini
try:
    with open("./Gemini_APIKEY.txt", "r", encoding="utf-8") as archivo:
        Gemini_APIKEY = archivo.read()
except FileNotFoundError:
    raise Exception(f"El archivo Gemini_APIKEY.txt no fue encontrado, crearlo y meter dentro la APIKEY de gemini")

GEMINI_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + Gemini_APIKEY
OLLAMA_ENDPOINT = "http://ollama:11434/api/generate"
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

# Crea la query y la ejecuta para Gemini
async def sendGeminiQuery(prompt, LLMConfig, temperature, max_length):

    async with httpx.AsyncClient() as client:
        response = await client.post(GEMINI_ENDPOINT, json=
        {
            "systemInstruction": {
                    "parts":[
                        {"text": LLMConfig}
                    ]
                },
            "generationConfig": {
                    "temperature": temperature * 2, # USAN RANGO 0-2
                    "maxOutputTokens": max_length,
                },
            "contents": [
                {
                    "parts": [
                        {"text": prompt}
                    ]
                }
            ]
        })
    response.raise_for_status()
    return response

# Crea la query y la ejecuta para Ollama
async def sendLlamaQuery(prompt, LLMConfig, temperature, max_length):    
    payload = {
        "model": "llama3:8b",
        "messages": [
            {"role": "system", "content": LLMConfig},
            {"role": "user", "content": prompt}
        ],
        "options": {
            "temperature": temperature,
            "num_predict": max_length
        },
        "stream": False  # Importante: para recibir la respuesta de una sola vez
    }
    try:
        response = requests.post(OLLAMA_ENDPOINT, json=payload)
        response.raise_for_status() # Lanza un error si la petición falla
        
        data = response.json()
        return data['message']['content']
    
    except requests.exceptions.RequestException as e:
        return f"Error al conectar con Ollama: {e}"


# endpoint principal
@app.post("/ask")
async def ask_LLMAttorney(query: Query):
    try:
        if query.mode == "Gemini":
            answer = await sendGeminiQuery(query.prompt, query.LLMConfig, query.temperature, query.max_length)
            answer = answer.json()["candidates"][0].get("content")["parts"][0].get("text")
        elif query.mode == "Llama":
            answer = await sendLlamaQuery(query.prompt, query.LLMConfig, query.temperature, query.max_length)

        return {"answer": answer }
    except httpx.HTTPStatusError as e:
        raise HTTPException(status_code=e.response.status_code, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
