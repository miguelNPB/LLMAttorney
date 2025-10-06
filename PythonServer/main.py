from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import httpx

# Abrimos apikey de gemini
try:
    with open("./Gemini_APIKEY.txt", "r", encoding="utf-8") as archivo:
        Gemini_APIKEY = archivo.read()
except FileNotFoundError:
    raise Exception(f"El archivo Gemini_APIKEY.txt no fue encontrado, crearlo y meter dentro la APIKEY de gemini")

GEMINI_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + Gemini_APIKEY

# estructura de json de entrada, pide un campo prompt que es un string
class Query(BaseModel):
    mode: str
    prompt: str
    LLMConfig: str

# se crea API
app = FastAPI(title="API server")

async def sendGeminiQuery(prompt, LLMConfig):

    async with httpx.AsyncClient() as client:
        response = await client.post(GEMINI_ENDPOINT, json=
        {
            "systemInstruction": {
                        "parts":[
                            {"text": LLMConfig}
                        ]
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

async def sendLlamaQuery(prompt, LLMConfig):
    return "NOTIMPLEMENTED"

# endpoint principal
@app.post("/ask")
async def ask_LLMAttorney(query: Query):
    try:
        if query.mode == "Gemini":
            answer = await sendGeminiQuery(query.prompt, query.LLMConfig)
            answer = answer.json()["candidates"][0].get("content")["parts"][0].get("text")
        elif query.mode == "Llama":
            answer = await sendLlamaQuery(query.prompt, query.LLMConfig)

        return {"answer": answer }
    except httpx.HTTPStatusError as e:
        raise HTTPException(status_code=e.response.status_code, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
