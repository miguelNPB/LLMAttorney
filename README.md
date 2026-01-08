# LLMAttorney - Unity

Ejecutamos un servidor de python para poder procesar las requests de prompts y en el servidor hacemos la llamada a Gemini, Ollama etc...
El servidor se ejecuta desde dentro de un docker que contiene todas las dependencias y así evitamos que funcione distinto en sistemas distintos, para ejecutarlo llamar a ```run.bat``` o compilar el docker con ```docker build -t llmattorney .``` y ejecutarlo desde cualquier cmd con ```docker run -p 8000:8000 llmattorney```

## Prerequisitos

### Uvicorn

Primero instalar uvicorn, el gestor REST de la API de nuestro servidor de python y meterlo en el path  

Lo puedes instalar desde un cmd con ```pip install uvicorn``` y luego comprobar que la carpeta ```C:\Python310\Scripts\``` está en las variables de entorno, el path, para poder llamar a uvicorn desde cmd.  

Enlace: https://uvicorn.dev/  

### Gemini
Para que funcione GEMINI debes incluir la APIKEY en el fichero Gemini_APIKEY.txt dentro de la carpeta PythonServer, la APIKEY se encuentra en el notion en la pestaña de Gemini.

### Ollama (Llama, )
Para poder correr Llama necesitas Ollama. Los modelos se descargarán la primera vez que se corra el programa, son 5gb cada uno aproximadamente.  
También hace falta instalar el paquete de python, en un cmd ejecuta ```pip install ollama```.

En caso de tener amd seguir el siguiente tutorial: https://github.com/likelovewant/ollama-for-amd

Enlace: https://ollama.com

