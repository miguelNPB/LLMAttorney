from fastapi import FastAPI, HTTPException
from contextlib import asynccontextmanager
from pydantic import BaseModel
from typing import Optional, Dict, Any
import httpx, os
import json

from rag import init_RAG, get_rag_data
from ollama_query import sendOllamaQuery


# Clase con la estructura del JSON pasado a este servidor
class LLMAttorneyQuery(BaseModel):
    prompt: str #El texto de la pregunta o instrucción que se le da al modelo
    LLMConfig: str #La configuración o instrucciones para el modelo, que pueden incluir contexto adicional
    temperature: float # 0 = Estricto 1 = Creativo 
    json_schema: Optional[Dict[str, Any]] = None # Recibe el esquema JSON a estructurar la salida. Si es None, funciona en texto normal
    rag_use: bool # Flag que indica si queremos usar RAG para la consulta que se le hace al modelo
    rag_index: int = 0 # Indice del vector store a usar en caso de que se quiera usar RAG, por defecto el primero creado (en este caso el del codigo civil)


# --- Variables

# --- Constantes
OLLAMA_HOST = os.getenv("OLLAMA_HOST", "http://localhost:11434")
OLLAMA_ENDPOINT_NVIDIA = "http://ollama-server:11434/v1"
OLLAMA_ENDPOINT_AMD_VULKAN = "http://host.docker.internal:11434/v1"

app = FastAPI(title="LLMAttorney Server")
vectorStores = []
useVulkan = False
modelName = ""
# --- 


# Carga la configuracion del servidor
def load_config():
    global useVulkan, modelName
    try:
        with open("./server_config.json", "r", encoding="utf-8") as f:
            config = json.load(f)
            useVulkan = config.get("useVulkan")
            modelName = config.get("modelName")
    except FileNotFoundError:
        raise RuntimeError("ERROR, server_config.json no encontrado")
    except json.JSONDecodeError:
        raise RuntimeError("ERROR, server_config.json contiene un formato JSON invalido")



@app.on_event("startup")
def startup():
    load_config()

    global vectorStores
    vectorStores = init_RAG(OLLAMA_HOST)

# endpoint principal
@app.post("/ask")
def ask_LLMAttorney(query: LLMAttorneyQuery):
    global vectorStores, useVulkan, modelName
    try:
        if query.rag_use:
            query.LLMConfig += get_rag_data(query.prompt, query.rag_index, vectorStores)  
        
        answer = sendOllamaQuery(query.prompt, query.LLMConfig, query.temperature, query.json_schema, modelName, OLLAMA_ENDPOINT_AMD_VULKAN if useVulkan else OLLAMA_ENDPOINT_NVIDIA)
        return answer
    except httpx.HTTPStatusError as e:
        raise HTTPException(status_code=e.response.status_code, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))