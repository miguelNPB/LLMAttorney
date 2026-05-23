# imagen python
FROM python:3.11-slim

# Creamos el directorio principal dentro del docker
WORKDIR /app

# Primero instalamos dependencias 
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copiamos el codigo
COPY . .

# exponemos el puerto
EXPOSE 8000

# ejecutamos el docker
ENV PYTHONUNBUFFERED=1
CMD ["gunicorn", "-w", "1", "-k", "uvicorn.workers.UvicornWorker", "main:app", "--bind", "0.0.0.0:8000", "--timeout", "900", "--capture-output", "--log-level", "info"]