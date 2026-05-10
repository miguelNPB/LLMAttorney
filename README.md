# LLMAttorney 

## Requisitos

- Windows 11 o 10
- Tener Docker Desktop https://www.docker.com/products/docker-desktop/

## Como usar

En paralelo a la ejecución de la build del simulador, se debe tener corriendo el servidor que gestione el procesamiento de las peticiones al modelo y Ollama, para ello ir a la carpeta `PythonServer`, configurar el servidor y luego ejecutar el .bat correspondiente

### Configurar el servidor
El servidor toma un json llamado `server_config.json` para configurarse y elegir su usar vulkan y además el modelo a usar.  
run.bat toma una configuración encontrada en `configs/server_config_noVulkan` y la copia a server_config.json
runVulkan.bat toma una configuración encontrada en `configs/server_config_vulkan` y la copia a server_config.json

### Ejecutar el servidor
Ejecutamos un servidor de python para poder procesar las requests de prompts y en el servidor hacemos la llamada a Ollama.  

El servidor se ejecuta desde un docker, recomendamos usar los .bat diseñados para ello, sino manualmente compilar el docker manualmente con ```docker build -t llmattorney .``` y ejecutarlo desde cualquier cmd con ```docker run -p 8000:8000 llmattorney```

Para ollama se debe también activar un servidor que se comunicará con el servidor llmattorney.  
Ollama tiene un modo de uso de GPU con Docker compatible de usar con NVIDIA, por lo que si se tiene una tarjeta gráfica NVIDIA llamar a `run.bat`, que ejecutará el servidor llmattorney en docker y ollama en docker. 
En el caso de AMD, no podemos ejecutar ollama en docker porque no es soportado, y por lo tanto debe ser ejecutado fuera de él. Por lo que en este caso se deberá tener instalado además Ollama https://ollama.com/. Para estas tarjetas gráficas, llamar a `runVulkan.bat` que ejecutará el servidor llmatorney en docker y ollama normal


## Como analizar telemetría

Para la telemetría se analiza en `TelemetryAnalyzer`, los datos deben ser puestos en la carpeta `/data` y los resultados saldrán en la carpeta `/results`  
Además, al ejecutar el main.py, es necesario pasar un argumento con el nombre de los títulos de las gráficas, por ejemplo: `python main.py "Estudiantes Derecho"`  
