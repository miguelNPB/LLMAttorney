# imagen python
FROM python:3.11-slim

# Creamos el directorio principal dentro del docker
WORKDIR /app

# Primero instalamos dependencias 
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
RUN pip install -U "langchain[google-genai]"
RUN pip install -U "langchain[ollama]"
RUN pip install -U langchain-chroma
RUN pip install -qU langchain-google-genai
RUN pip install -qU langchain-ollama


# Copiamos el codigo y, si esta, el Gemini_APIKEY
COPY . .

# exponemos el puerto
EXPOSE 8000

# ejecutamos el docker
CMD ["gunicorn", "-w", "1", "-k", "uvicorn.workers.UvicornWorker", "main:app", "--bind", "0.0.0.0:8000", "--timeout", "900"]