from importlib.resources import path
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
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
import guidance
from guidance import models, gen, select
from guidance import json as gen_json
import asyncio
import datetime

import httpx, requests, json, bs4, getpass, os

vector_store = None
retriever = None

# Carga de archivos Rag
def load_RAG_file():

    a = datetime.datetime.now()

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
        loader = PyPDFLoader("./documentos_rag/boe_codigo_civil_legislacion_complementaria.pdf", mode = "single")
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

        b = datetime.datetime.now()

        print(f"Tiempo de carga y vectorizacion: {b-a}")

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

        b = datetime.datetime.now()

        print(f"Tiempo de carga y vectorizacion: {b-a}")

        #Retriever a partir del vector store
        retriever = vector_store.as_retriever()
        retriever_test = retriever.invoke("Quien tiene derecho a solicitar la nacionalidad española?")
        print(retriever_test[0].page_content)

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
    # Nuevo campo: Recibe el esquema JSON deseado
    # Si es None, funciona en modo texto normal
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
            lm += gen_json("result", schema=json_schema, temperature=temperature, max_tokens=max_length)
        return json.loads(lm['result'])
    else:
        with guidance.assistant():
            lm += gen(name="result", temperature=temperature, max_tokens=max_length)
        return lm['result']

    

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
        else:
            raise HTTPException(status_code=400)
        return answer
    except httpx.HTTPStatusError as e:
        raise HTTPException(status_code=e.response.status_code, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))