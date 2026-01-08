# imagen python
FROM python:3.11-slim

# Creamos el directorio principal dentro del docker
WORKDIR /app

# Primero instalamos dependencias 
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copiamos el codigo y, si esta, el Gemini_APIKEY
COPY . .

# exponemos el puerto
EXPOSE 8000

# ejecutamos el docker
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]