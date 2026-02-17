# LLMAttorney - Unity

Ejecutamos un servidor de python para poder procesar las requests de prompts y en el servidor hacemos la llamada a Gemini, Ollama etc...
El servidor se ejecuta desde dentro de un docker que contiene todas las dependencias y así evitamos que funcione distinto en sistemas distintos, para ejecutarlo llamar a ```run.bat``` o compilar el docker con ```docker build -t llmattorney .``` y ejecutarlo desde cualquier cmd con ```docker run -p 8000:8000 llmattorney```

## Prerequisitos

### Tener instalado docker desktop

https://www.docker.com/get-started/

Una vez iniciado el docker desktop engine, ejecutar el run.bat dentro de PythonServer


### Gemini
Para que funcione GEMINI debes incluir la APIKEY en el fichero Gemini_APIKEY.txt dentro de la carpeta PythonServer, la APIKEY se encuentra en el notion en la pestaña de Gemini.

### Ollama 

Si usas nvidia: Cambiar constante USE_OLLAMA_VULKAN a FALSE en main.py

## Formato de prompts que acepta LLMAttorney

Recibe un json con los siguientes campos:
- mode: Puede ser `Gemini` o `Llama`
- prompt: El texto del prompt
- LLMConfig: La configuracion de como quieres que responda el LLM
- temperature: El valor de 'creatividad' del LLM. 0 es nada y 1 es el maximo de creatividad.
- json_schema: (OPCIONAL) Incluye un esquema JSON que devolverá el LLM. En LLMConfig hay que decirle como rellenarlo.

#### Prompt de ejemplo sin esquema json
```
{
	"mode":"Gemini",
	"prompt": "Hola gemini !",
  "LLMConfig": "Solo puedes contestar diciendo adios.",
  "temperature": 0.8,
  "max_length": 4000
}
```

#### Prompt de ejemplo con esquema json
```
{
	"mode":"Llama",
	"prompt": "Genera un personaje de rol",
  "LLMConfig": "Eres un master de dungeons n dragons",
  "temperature": 0.8,
  "max_length": 4000,
  "json_schema": {
    "type": "object",
    "properties": {
      "nombre": { "type": "string" },
      "clase": { "type": "string", "enum": ["Guerrero", "Mago", "Ladrón"] },
      "nivel": { "type": "integer", "minimum": 1, "maximum": 20 }
    },
    "required": ["nombre", "clase", "nivel"]
  }
}


```