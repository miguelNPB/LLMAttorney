# LLMAttorney - Unity

Para mandar un prompt a LLMAttorney debemos mandar un JSON esquematizado como sale abajo. Dentro de Unity se han creado unas clases para evitar el tener que estructurar el JSON a mano.

Para obtener una respuesta de la LLM, debemos llamar a ```LLMAttorney_API.Instance.SendPrompt``` y pasar los argumentos necesarios. La LLM procesará la petición y devolverá la respuesta a la función pasada en el argumento ```onComplete```. 

Para esquematizar la respuesta del LLM, si pasamos null al campo JsonSchema, el LLM devolverá un string como hace cualquier llm como chatgpt en su web.  
Si queremos que responda en formato JSON, con campos con variables de tipos, debemos pasar un JsonSchema. Para crearlo se crea haciendo new, y luego se tienen que añadir en properties clases de tipo PropertyInfo, que contienen un valor que pasar al inicializar con el tipo de variable posible.  
Hay 5 tipos: String, Float, Integer, Boolean, Object (un propertyInfo que contiene otros propertyInfo), y Array (Array de strings)  
Luego de pasar el prompt, al recibirlo hay que serializarlo en unas clases que crearemos, que contienen los mismos campos y mismos nombres que hemos creado en el JsonSchema.

### Código de ejemplo
```
using System;
using UnityEngine;

public class Test
{
    [Serializable]
    private class Campo3
    {
        public string subcampo1;
        public string subcampo2;
    }
    [Serializable]
    private class ExampleClass
    {
        public string campo1;
        public float campo2;
        public Campo3 campo3;
        public string[] campo4;
    }

    public void RecieveResponse(bool success, string answer)
    {
        // deserializamos la respuesta
        ExampleClass jsonResponse = JsonUtility.FromJson<ExampleClass>(answer);

        // obtenemos los datos !
        Debug.Log("CAMPO1: " + jsonResponse.campo1);
        Debug.Log("CAMPO2: " + jsonResponse.campo2);
        Debug.Log("CAMPO3-subcampo1: " + jsonResponse.campo3.subcampo1);
        Debug.Log("CAMPO3-subcampo2: " + jsonResponse.campo3.subcampo2);
        foreach(var iterm in jsonResponse.campo4)
        {
            Debug.Log("CAMPO4: " + iterm);
        }
    }


    void Start()
    {
        JsonSchema schema = new JsonSchema();

        schema.properties.Add("campo1", new PropertyInfo(JsonDataType.String));
        schema.properties.Add("campo2", new PropertyInfo(JsonDataType.Float));

        PropertyInfo subProperty = new PropertyInfo(JsonDataType.Object);
        subProperty.properties.Add("subcampo1", new PropertyInfo(JsonDataType.String));
        subProperty.properties.Add("subcampo2", new PropertyInfo(JsonDataType.String));
        schema.properties.Add("campo3", subProperty);

        schema.properties.Add("campo4", new PropertyInfo(JsonDataType.Array));

        LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveResponse, "Rellena el campo 1 con un pais y el campo 2 con el numero de habitantes, luego rellena el campo 3 con nombres de perros, y el cmapo 4 con nombres de mujeres", "", schema);
    }
}
```

# LLMAttorney - Servidor

Ejecutamos un servidor de python para poder procesar las requests de prompts y en el servidor hacemos la llamada a Gemini, Ollama etc...
El servidor se ejecuta desde dentro de un docker que contiene todas las dependencias y así evitamos que funcione distinto en sistemas distintos, para ejecutarlo llamar a ```run.bat``` o compilar el docker con ```docker build -t llmattorney .``` y ejecutarlo desde cualquier cmd con ```docker run -p 8000:8000 llmattorney```

## Prerequisitos

### Tener instalado docker desktop

https://www.docker.com/get-started/

Una vez iniciado el docker desktop engine, ejecutar el run.bat dentro de PythonServer


### Gemini
Para que funcione GEMINI debes incluir la APIKEY en el fichero Gemini_APIKEY.txt dentro de la carpeta PythonServer, la APIKEY se encuentra en el notion en la pestaña de Gemini.

(Actualmente no funciona, usar ollama)

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