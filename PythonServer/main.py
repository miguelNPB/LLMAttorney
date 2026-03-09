from importlib.resources import path

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import Optional, Dict, Any
#Text management
from langchain_community.document_loaders import PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
#Embeddings
from langchain_google_genai import GoogleGenerativeAIEmbeddings
from langchain_ollama import OllamaEmbeddings
#VectorStore
from langchain_community.vectorstores.chroma import Chroma
from langchain_core.vectorstores import InMemoryVectorStore
from langchain.tools import tool
from pathlib import Path
import httpx
import requests
import json
from guidance import models

import httpx, requests, json, bs4, getpass, os

vector_store = None
retriever = None

# Carga de archivos Rag
def load_RAG_file():

    path_civilCode = Path("/vector_db/CodigoCivil_db")
    path_civilCode.mkdir(parents=True, exist_ok=True)

    #EMBEDDINGS!
    #embeddings = GoogleGenerativeAIEmbeddings(model="models/gemini-embedding-001")
    embeddingsRag = OllamaEmbeddings(
        model="nomic-embed-text",
        base_url="http://host.docker.internal:11434"
    ) 

    if not any(path_civilCode.iterdir()):

        print("No existe el directorio del vector store, creando uno nuevo a partir del PDF...")

        # Carga del documento PDF dando la ruta y el modo de carga (todo en un bloque, sin array)
        loader = PyPDFLoader("./CodigoCivilYLegislacion.pdf", mode = "single")
        docs = loader.load()

        # División de todo el texto en sectores
        text_splitter = RecursiveCharacterTextSplitter(
            chunk_size=500,  # chunk size (characters)
            chunk_overlap=100,  # chunk overlap (characters)
            add_start_index=True,  # track index in original document
        )

        all_splits = text_splitter.split_documents(docs)   

        #Vector Store
        vector_store = Chroma.from_documents(
            documents=all_splits,
            embedding=embeddingsRag,
            persist_directory=str(path_civilCode),
            collection_name="codigo_civil"
        )

        vector_store.persist()

        #Retriever a partir del vector store
        retriever = vector_store.as_retriever()
        retriever_test = retriever.invoke("Quien tiene derecho a solicitar la nacionalidad española?")
        print(retriever_test[0].page_content)

    else:

        print("Si existe el directorio del vector store, cargando el vector store ya creado...")

        vector_store = Chroma(
            persist_directory=str(path_civilCode),
            embedding_function=embeddingsRag,
            collection_name="codigo_civil"
        )

        #Retriever a partir del vector store
        retriever = vector_store.as_retriever()
        retriever_test = retriever.invoke("Quien tiene derecho a solicitar la nacionalidad española?")
        print(retriever_test[0].page_content)


# Abrimos apikey de gemini
try:
    with open("./Gemini_APIKEY.txt", "r", encoding="utf-8") as archivo:
        Gemini_APIKEY = archivo.read()
except FileNotFoundError:
    raise Exception(f"El archivo Gemini_APIKEY.txt no fue encontrado, crearlo y meter dentro la APIKEY de gemini")

print("ALgo erno")

if not os.environ.get("GOOGLE_API_KEY"):
    os.environ["GOOGLE_API_KEY"]= Gemini_APIKEY

GEMINI_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key=" + Gemini_APIKEY
OLLAMA_ENDPOINT = "http://ollama-server:11434/api/chat"

# se crea API
app = FastAPI(title="LLMAttorney Server")

@app.on_event("startup")
def startup():
    load_RAG_file()

# --- 

# estructura de json de entrada, pide un campo prompt que es un string
class Query(BaseModel):
    mode: str
    prompt: str #El texto de la pregunta o instrucción que se le da al modelo
    LLMConfig: str #La configuración o instrucciones para el modelo, que pueden incluir contexto adicional
    temperature: float # 0 = Creativo 1 = Estricto
    max_length: int # Longitud maxima del output
    # Nuevo campo: Recibe el esquema JSON deseado. 
    # Si es None, funciona en modo texto normal.
    json_schema: Optional[Dict[str, Any]] = None
    rag_use: bool #Flag que indica si queremos usar RAG para la consulta que se le hace al modelo

# ---

#Este metodo se encarga de recibir una peticion del usuario y dar contexto al LLM dados los documentos que almacena en el vector store.
#devuelve dos datos, el content que es el texto plano con el contenido de los documentos recuperados y los propios documentos que usa para dar esa respuesta.
@tool(description="Devuelve el contexto relevante para una consulta dada, basado en los documentos almacenados en el vector store.",
      response_format="content_and_artifact")
def retrieve_context(query: str):
    retrieved_docs = vector_store.similarity_search(query, k=3)  # Recupera los 3 documentos más relevantes segun la vectorizacion

    #traduccion del texto de los documentos a un formato mas interpretable y ordenado para el LLM.
    serialized = "\n\n".join(
        (f"Source: {doc.metadata}\nContent: {doc.page_content}") for doc in retrieved_docs
    )

    return serialized, retrieved_docs

# ---

# Crea la query y la ejecuta para Gemini, los distintos datos que se aportan son
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
        response = await client.post(GEMINI_ENDPOINT, json=payload, timeout=60.0) 
        
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

    if json_schema:
        # Añadimos instrucciones explícitas y el esquema convertido a texto
        LLMConfig += (
            "\n\n### INSTRUCCIONES DE FORMATO:\n"
            "Debes responder EXCLUSIVAMENTE con un objeto JSON válido.\n"
            "No incluyas texto antes ni después del JSON.\n"
            f"Sigue estrictamente este esquema:\n{json.dumps(json_schema, indent=2)}"
        )

    payload = {
        "model": "llama3",
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
    
    if json_schema:
        payload["format"] = "json"
    
    try:
        response = requests.post(OLLAMA_ENDPOINT, json=payload)
        response.raise_for_status() # Lanza un error si la petición falla
        
        data = response.json()

        if json_schema:
            try:
                return json.loads(data['message']['content'])
            except json.JSONDecodeError:
                return {"answer": data['message']['content'], "error": "El modelo no devolvió un JSON válido"}
        
        return data['message']['content']
    
    except requests.exceptions.RequestException as e:
        return f"Error al conectar con Ollama: {e}"


# endpoint principal
@app.post("/ask")
async def ask_LLMAttorney(query: Query):


    #Comprobacion de uso de rag y obtencio de contexto
    if query.rag_use:
        contexto, docs_usados = retrieve_context(query.prompt)
        #Anyadimos el contexto a la configuracion del LLM para que lo use como referencia a la hora de generar la respuesta
        query.LLMConfig += (
            "\n\n### CONTEXTO DE APOYO:\n"
            "Utiliza esta informacion como apoyo de tu respuesta\n"
            "No incluyas enlaces ni citas exactas al contexto pasado, simplemente usalo para informarte y generar una respuesta mas precisa y fundamentada\n"
            f"{contexto}"
        )

    #Peticiones a los servidores de Gemini y Llama que residen en los respectivos dockers
    try:
        if query.mode == "Gemini":
            answer = await sendGeminiQuery(query.prompt, query.LLMConfig, query.temperature, query.max_length, query.json_schema)
        elif query.mode == "Llama":
            answer = await sendLlamaQuery(query.prompt, query.LLMConfig, query.temperature, query.max_length, query.json_schema)

        return answer
    except httpx.HTTPStatusError as e:
        raise HTTPException(status_code=e.response.status_code, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
