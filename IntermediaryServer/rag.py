from langchain_community.document_loaders import PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_ollama import OllamaEmbeddings
from langchain_community.vectorstores.chroma import Chroma
import datetime
from pathlib import Path
from dataclasses import dataclass
from fastapi import HTTPException
import os
import json
import shutil

# Clase para manejar datos de un fichero a usar en RAG
@dataclass
class RagFile:
    pathDatabase: str # Donde se va a guardar los datos de RAG
    pathContent: str # Al PDF de contenido
    ragCollectionName: str # Nombre a usar

# Metodo publico a llamar para obtener datos de un documento con RAG. Devuelve el texto obtenido
def get_rag_data(prompt, ragIndex, vectorStores):
    if ragIndex >= len(vectorStores) or ragIndex < 0:
        raise HTTPException(status_code=400, detail=f"RAG index {ragIndex} is out of range. Available vector stores: 0 to {len(vectorStores)-1}")

    retriever_output = vectorStores[ragIndex].as_retriever().invoke(prompt)  # Recupera los documentos relevantes para la consulta

    contexto = "\n\n".join([doc.page_content for doc in retriever_output])

    # Sumamos el contexto a la configuracion del LLM para que lo use como referencia a la hora de generar la respuesta
    context = (
        "\n\n### CONTEXTO DE APOYO:\n"
        "Utiliza esta informacion como apoyo de tu respuesta\n"
        "No incluyas enlaces ni citas exactas al contexto pasado, simplemente usalo para informarte y generar una respuesta mas precisa y fundamentada\n"
        f"{contexto}"  # Aqui se añade el contenido recuperado al final de la configuracion del LLM
    )

    return context

# Metodo publico para obtener el id del caso actual
def get_case_data_id():
    caseDataFolder = "./documentos_rag/case_data"
    # Verificamos si la carpeta existe antes de escanear
    if not os.path.exists(caseDataFolder):
        return -1

    try:
        for entrada in os.scandir(caseDataFolder):
            if entrada.is_file() and entrada.name.lower().endswith('.pdf'):
                id, _ = os.path.splitext(entrada.name)
                return id
    except Exception as e:
        raise RuntimeError(f"ERROR cogiendo el id del pdf del caso: {str(e)}")
        
    return -1

# Metodo publico para cambiar el pdf caso de RAG, borra los pdf y borra la carpeta de vector_db suya y actualiza los vectorStore
def override_case_RAG(newPDF, id, vectorStores, OLLAMA_HOST):
    db_path = "./vector_db/CasoBase_db"
    caseDataFolder = "./documentos_rag/case_data"

    # borramos antiguo caso
    # se quita la referencia para quitar locks de Chroma
    if len(vectorStores) > 0 and vectorStores[0] is not None:
        try:
            vectorStores[0].delete_collection()
        except Exception as e:
            print(f"Aviso interno al intentar borrar coleccion en Chroma: {e}")
        
        vectorStores[0] = None

    if os.path.exists(caseDataFolder):
        try:
            shutil.rmtree(caseDataFolder)
            print(f"PDF eliminado en: {caseDataFolder}")
        except Exception as e:
            raise RuntimeError(f"ERROR al borrar la base de datos vectorial existente: {str(e)}")
    
    # copiamos nuevo pdf con su id
    os.makedirs(caseDataFolder, exist_ok=True)
    caseDataPDFPath = os.path.join(caseDataFolder, f"{id}.pdf")
    try:
        with open(caseDataPDFPath, "wb") as buffer:
            shutil.copyfileobj(newPDF.file, buffer)
        print(f"Nuevo PDF del caso copiado correctamente en: {caseDataPDFPath}")
    except Exception as e:
        raise RuntimeError(f"ERROR al copiar el nuevo archivo PDF al servidor: {str(e)}")

    # sobreescribimos vectorStore
    caseDataPDFPath = _get_case_RAG_file()
    if caseDataPDFPath != None:
        caseRagFile = RagFile(
            pathDatabase="./vector_db/CasoBase_db",
            pathContent=caseDataPDFPath,
            ragCollectionName="caso_base"
        )
        _load_rag_file(vectorStores, caseRagFile, 0, OLLAMA_HOST)
        print("Cargado nuevo rag del nuevo pdf de caso base")
    else:
        raise RuntimeError("ERROR sobreescribiendo RAG del caso")


# Metodo publico para inicializar el sistema de RAG y que cargue los ficheros a usar. Devuelve el array de vectorStores a usar para utilizar rag
def init_RAG(OLLAMA_HOST):
    startTime = datetime.datetime.now()

    ragsFiles = _get_RAG_files()
    vectorStores = [None] * (1 + len(ragsFiles))
    
    # rag del caso
    caseDataPDFPath = _get_case_RAG_file()
    if caseDataPDFPath != None:
        caseRagFile = RagFile(
            pathDatabase="./vector_db/CasoBase_db",
            pathContent=caseDataPDFPath,
            ragCollectionName="caso_base"
        )
        _load_rag_file(vectorStores, caseRagFile, 0, OLLAMA_HOST)

    # resto
    i = 1
    for rag_file in _get_RAG_files():
        _load_rag_file(vectorStores, rag_file, i, OLLAMA_HOST)
        i = i + 1

    endTime = datetime.datetime.now()
    print(f"Tiempo de carga y vectorizacion: {endTime-startTime}")
    return vectorStores


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

def _get_case_RAG_file():
    caseDataFolder = "./documentos_rag/case_data"

    if not os.path.exists(caseDataFolder):
        return None

    caseDataPath = next(
        (os.path.abspath(f.path) for f in os.scandir(caseDataFolder) if f.is_file() and f.name.lower().endswith('.pdf')), 
        None # default none si no encuentra
    )

    return caseDataPath

# metodo privado para cargar un rag al vectorStores
def _load_rag_file(vectorStores, rag_file, index, OLLAMA_HOST):
    if (index >= len(vectorStores)):
        print("ERROR, intentado modificar un index del array de vectorStores fuera de rango")
        return

    databasePath = Path(rag_file.pathDatabase)
    databasePath.mkdir(parents=True, exist_ok=True)


    embeddingsRag = OllamaEmbeddings(
        model="nomic-embed-text",
        base_url=OLLAMA_HOST
    )

    if not any(databasePath.iterdir()):
        print(f"No existe el directorio del database con vector store de {databasePath}, creando uno nuevo a partir del PDF...")

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

        vectorStores[index] = vector_store
        print(f"Vector stores creados en {databasePath}")
    else:
        print(f"Existe el directorio del vector store {databasePath}, cargando su vector store...")

        vector_store = Chroma(
            persist_directory=str(databasePath),
            embedding_function=embeddingsRag,
            collection_name=rag_file.ragCollectionName
        )

        vectorStores[index] = vector_store
        print(f"Vector stores creados en {databasePath}")
