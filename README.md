# LLMAttorney - Unity

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

