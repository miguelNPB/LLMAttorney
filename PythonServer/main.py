from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import httpx

Gemini_APIKEY = ""

# se crea API
app = FastAPI(title="API server")

# estructura de json de entrada, pide un campo prompt que es un string
class Query(BaseModel):
    mode: str
    prompt: str

# endpoint principal
@app.post("/ask")
async def ask_llamafile(query: Query):
    try:
        if query.mode == "Gemini":
            endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key=" + 
        elif query.mode == "Llama":
            endpoint = ""


        # Envia el prompt al Llamafile vía HTTP
        async with httpx.AsyncClient() as client:
            response = await client.post(endpoint, json={
                "prompt": query.prompt
            })
        response.raise_for_status()
        data = response.json()

        return {"answer": data.get("text", "")}

    except httpx.HTTPStatusError as e:
        raise HTTPException(status_code=e.response.status_code, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
