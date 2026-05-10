from langchain_community.document_loaders import PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_ollama import OllamaEmbeddings
from langchain_community.vectorstores.chroma import Chroma
from langchain_core.vectorstores import InMemoryVectorStore
from langchain.tools import tool

import datetime
from pathlib import Path
from dataclasses import dataclass
from fastapi import HTTPException
import os
import json

# Clase para manejar datos de un fichero a usar en RAG
@dataclass
class RagFile:
    pathDatabase: str # Donde se va a guardar los datos de RAG
    pathContent: str # Al PDF de contenido
    ragCollectionName: str # Nombre a usar

# Metodo privado para obtener datos de los ficheros a usar para el rag 
def _get_RAG_files():
    ragsFiles = []
    try:
        with open("./rag_config.json", "r", encoding="utf-8") as f:
            ragConfig = json.load(f)

            for ragFileJSON in ragConfig:
                ragFile = RagFile(
                    pathDatabase=ragFileJSON.get("pathDatabase"),
                    pathContent=ragFileJSON.get("pathContent"),
                    ragCollectionName=ragFileJSON.get("ragCollectionName")
                )

                ragsFiles.append(ragFile)
    except FileNotFoundError:
        raise RuntimeError("ERROR, rag_config.json no encontrado")
    except json.JSONDecodeError:
        raise RuntimeError("ERROR, rag_config.json contiene un formato JSON invalido")

    return ragsFiles


# Metodo publico para inicializar el sistema de RAG y que cargue los ficheros a usar. Devuelve el array de vectorStores a usar para utilizar rag
def init_RAG(OLLAMA_HOST):
    startTime = datetime.datetime.now()

    vectorStores = []
    
    for rag_file in _get_RAG_files():
        databasePath = Path(rag_file.pathDatabase)
        databasePath.mkdir(parents=True, exist_ok=True)


        embeddingsRag = OllamaEmbeddings(
            model="nomic-embed-text",
            base_url=OLLAMA_HOST
        )

        if not any(databasePath.iterdir()):
            print("No existe el directorio del database vector store, creando uno nuevo a partir del PDF...")

            if not os.path.exists(rag_file.pathContent):
                raise Exception(f"Archivo RAG no encontrado: {rag_file.pathContent}")

            # Carga del documento PDF dando la ruta y el modo de carga
            loader = PyPDFLoader(rag_file.pathContent, mode = "single")
            docs = loader.load()

            # Division de todo el texto en sectores
            text_splitter = RecursiveCharacterTextSplitter(
                chunk_size=500,  # chunk size en characters
                chunk_overlap=100,  # chunk overlap en characters
                add_start_index=True,
            )

            all_splits = text_splitter.split_documents(docs)   

            #Vector Store
            vector_store = Chroma.from_documents(
                documents=all_splits,
                embedding=embeddingsRag,
                persist_directory=str(databasePath),
                collection_name=rag_file.ragCollectionName
            )

            vector_store.persist()

            vectorStores.append(vector_store)
        else:
            print("Si que existe el directorio del vector store, cargando el vector store ya creado...")

            vector_store = Chroma(
                persist_directory=str(databasePath),
                embedding_function=embeddingsRag,
                collection_name=rag_file.ragCollectionName
            )

            vectorStores.append(vector_store)

    endTime = datetime.datetime.now()
    print(f"Tiempo de carga y vectorizacion: {endTime-startTime}")
    return vectorStores


# Metodo publico a llamar para obtener datos de un documento con RAG. Devuelve el texto obtenido
def get_rag_data(prompt, ragIndex, vectorStores):
    if ragIndex >= len(vectorStores) or ragIndex < 0:
        raise HTTPException(status_code=400, detail=f"RAG index {ragIndex} is out of range. Available vector stores: 0 to {len(vectorStores)-1}")

    print("vector_store:", vectorStores[ragIndex])

    retriever_output = vectorStores[ragIndex].as_retriever().invoke(prompt)  # Recupera los documentos relevantes para la consulta

    contexto = "\n\n".join([doc.page_content for doc in retriever_output])

    print("Contexto generado: ", contexto)

    # Sumamos el contexto a la configuracion del LLM para que lo use como referencia a la hora de generar la respuesta
    context = (
        "\n\n### CONTEXTO DE APOYO:\n"
        "Utiliza esta informacion como apoyo de tu respuesta\n"
        "No incluyas enlaces ni citas exactas al contexto pasado, simplemente usalo para informarte y generar una respuesta mas precisa y fundamentada\n"
        f"{contexto}"  # Aqui se añade el contenido recuperado al final de la configuracion del LLM
    )

    return context
