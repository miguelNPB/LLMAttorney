# LLMAttorney - IntermediaryServer

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

Para poder configurar los documentos disponbiles para usar en el RAG, modificar `rag_config.json`. Cada entrada tiene 3 campos, pathDatabase es la ruta donde se almacenará los embeddings generados, pathContent es la ruta al pdf con el contenido, y ragCollectionName es el nombre identificador del documento en el sistema RAG. 
En el caso del documento del caso del juego, no es posible configurarlo a través de rag_config, ya que utilizará el documento encontrado en `"./documentos_rag/case_data"`, y su id es el nombre del pdf. Durante la ejecución del juego es posible cambiarlo, de forma que se sobreescribirá el pdf encontrado en esa carpeta, y se regenerarán los embeddings en su carpeta `./vector_db/CasoBase_db`

### Ejecutar el servidor

El servidor se ejecuta desde un docker, recomendamos usar los .bat diseñados para ello, sino manualmente compilar el docker manualmente con ```docker build -t llmattorney .``` y ejecutarlo con este comando: ```docker run -p 8000:8000 llmattorney```

Para utilizar ollama se debe también activar en modo servidor para comunicarse con este servidor intermediario.  
Ollama tiene un modo de uso de GPU con Docker compatible de usar con NVIDIA, gracias al NVIDIA Container Toolkit, por lo que si se tiene una tarjeta gráfica NVIDIA llamar a `runNVIDIA.bat`, que ejecutará este servidor intermediario en un contendor Docker y Ollama con GPU en otro contendor Docker.

En el caso de AMD, Docker no da soporte para utilizar la GPU desde dentro de un contenedor con ROCm, que es el sistema de AMD para acelerar IA. Por ello debe ser ejecutado Ollama desde fuera del contenedor y comunicarse con este servidor intermediario, que sí estará en un contenedor de Docker.
De manera que para este caso se deberá tener instalado además localmente Ollama https://ollama.com/, y llamar a `runAMD.bat`.
En el caso de AMD, no podemos ejecutar ollama en docker usando GPU porque no tiene acceso a para usar GPU, y por lo tanto debe ser ejecutado fuera de él utilizando Vulkan. 


### Anotación a tener en cuenta en caso de problemas instalando

Este servidor es posible que no pueda ser instalado mientras haya un partido de fútbol de La Liga en vivo, esto es debido a que durante la emisión de un partido, La Liga utiliza a proveedores de internet para prohibir IPs para evitar la piratería, sin embargo, bloquea tan masivamente que bloquea a servidores inocentes, en nuestro caso al servidor cloudflare de los modelos de Ollama.   
Para saber más de este problema visitar: https://hayahora.futbol/  

En caso de problemas con la instalación durante partidos de fútbol de La Liga, utilizar la VPN de la UCM para evitar el bloqueo.