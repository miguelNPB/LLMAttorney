# LLMAttorney - MediatorServer

Esta carpeta almacena el servidor de python intermediario, que gestiona las peticiones enviadas del simulador para configurar y hacer peticiones al LLM, en este caso Ollama.  
## Requisitos

- Windows 11 o 10
- Tener Docker Desktop https://www.docker.com/products/docker-desktop/
- Si tienes tarjeta gráfica AMD, Ollama https://ollama.com/
## Como usar

En paralelo a la ejecución de la build del simulador, se debe tener corriendo el servidor que gestione el procesamiento de las peticiones al modelo y Ollama, para ello configurar el servidor y luego ejecutar el .bat correspondiente según la marca de GPU del ordenador.

### Configurar el servidor
El servidor toma un json llamado `server_config.json` para configurarse y elegir si usar vulkan y además el modelo a usar.  
run.bat toma una configuración encontrada en `configs/server_config_noVulkan` y la copia a server_config.json
runVulkan.bat toma una configuración encontrada en `configs/server_config_vulkan` y la copia a server_config.json

### Ejecutar el servidor

El servidor se ejecuta desde un docker, recomendamos usar los .bat diseñados para ello, sino manualmente compilar el docker manualmente con ```docker build -t llmattorney .``` y ejecutarlo con este comando: ```docker run -p 8000:8000 llmattorney```

Para utilizar ollama se debe también activar en modo servidor para comunicarse con este servidor intermediario.  
Ollama tiene un modo de uso de GPU con Docker compatible de usar con NVIDIA, gracias al NVIDIA Container Toolkit, por lo que si se tiene una tarjeta gráfica NVIDIA llamar a `run.bat`, que ejecutará este servidor intermediario en un contendor Docker y Ollama con GPU en otro contendor Docker.

En el caso de AMD, Docker no da soporte para utilizar la GPU desde un contenedor con ROCm, que es el sistema de AMD para acelerar IA. Por ello debe ser ejecutado Ollama desde fuera del contenedor y comunicarse con este servidor intermediario, que sí estará en un contenedor de Docker.  
Además, dado que ROCm está desarrollado para Linux y con poco soporte para Windows, Ollama ofrece Vulkan como alternativa para GPUs AMD.  
De manera que para este caso se deberá tener instalado además localmente Ollama https://ollama.com/, y llamar a `runVulkan.bat`.
En el caso de AMD, no podemos ejecutar ollama en docker usando GPU porque no tiene acceso a para usar GPU, y por lo tanto debe ser ejecutado fuera de él utilizando Vulkan. 