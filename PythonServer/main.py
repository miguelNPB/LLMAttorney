from fastapi import FastAPI, HTTPException, File, UploadFile, Form
from pydantic import BaseModel
from typing import Optional, Dict, Any
import httpx, os
import json

from rag import init_RAG, get_rag_data, override_case_RAG, get_case_data_id
from ollama_query import sendOllamaQuery

# Clase con la estructura del JSON pasado a este servidor para preguntas al LLM
class LLMAttorneyAskQuery(BaseModel):
    prompt: str #El texto de la pregunta o instrucción que se le da al modelo
    LLMConfig: str #La configuración o instrucciones para el modelo, que pueden incluir contexto adicional
    temperature: float # 0 = Estricto 1 = Creativo 
    json_schema: Optional[Dict[str, Any]] = None # Recibe el esquema JSON a estructurar la salida. Si es None, funciona en texto normal
    rag_use: bool # Flag que indica si queremos usar RAG para la consulta que se le hace al modelo
    rag_index: int = 0 # Indice del vector store a usar en caso de que se quiera usar RAG, por defecto el primero creado (en este caso el del codigo civil)



# --- Constantes
OLLAMA_HOST = os.getenv("OLLAMA_HOST", "http://localhost:11434")
OLLAMA_ENDPOINT_NVIDIA = "http://ollama-server:11434/v1"
OLLAMA_ENDPOINT_AMD_VULKAN = "http://host.docker.internal:11434/v1"
CASE_DATA_DIR = os.path.join(os.path.abspath(__file__), "case_rag")
# ---

# --- Variables
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


# ejecutado al iniciarse
@app.on_event("startup")
def startup():
    load_config()

    global vectorStores
    vectorStores = init_RAG(OLLAMA_HOST)

# endpoint principal para peticiones al LLM
@app.post("/ask")
def ask_LLMAttorney(query: LLMAttorneyAskQuery):
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

# endpoint para obtener el ID del caso usado actualmente en el RAG caso base
@app.post("/getcase-id")
def get_current_case_id():
    try:
        id = get_case_data_id()
        return id
    except httpx.HTTPStatusError as e:
        raise HTTPException(status_code=e.response.status_code, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

# endpoint para subir un pdf para sustituir el caso usado en el RAG caso base
@app.post("/upload-pdf")
def upload_pdf(id: int = Form(...),
    file: UploadFile = File(...)):

    # comprobar que es un pdf
    if not file.filename.lower().endswith(".pdf"):
        raise HTTPException(status_code=400, detail="ERROR, debe ser un .pdf")
    
    try:
        override_case_RAG(file, id, vectorStores, OLLAMA_HOST)
        return {"status": "success"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
