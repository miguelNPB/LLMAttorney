import json
import math
import guidance
from guidance import models, gen
from guidance import json as gen_json

#Limpia el json de valores anormales para que fastApi pueda interpretarlo correctamente
def clean_json(obj):

    if isinstance(obj, float) or isinstance(obj, int):
        if not math.isfinite(obj):
            return 0.0
        return obj
    elif isinstance(obj, dict):

        return {
            key: clean_json(value)
            for key, value in obj.items()
        } 
    elif isinstance(obj, list):

        return [
            clean_json(item)
            for item in obj
        ]
    
    return obj

# Crea la query y la ejecuta para Ollama
def sendOllamaQuery(prompt, LLMConfig, temperature, json_schema, modelName, ollamaEndpoint):
    
    try:
        lm = models.OpenAI(
            model=modelName,  # Modelo Ollama
            base_url= ollamaEndpoint, 
            api_key="ollama"
        )

        print("Modelo Llama configurado, preparando prompt...")

        with guidance.system():
            lm += LLMConfig

        print("guidance system preparado, añadiendo prompt...")

        with guidance.user():
            lm += prompt

        print("guidance user preparado, añadiendo prompt...")

        if json_schema:
            with guidance.assistant():
                lm += gen_json("result", schema=json_schema, temperature=temperature)

            raw_result = lm['result']
            parsed = json.loads(raw_result)
            cleaned = clean_json(parsed)

            return cleaned
        
        else:
            with guidance.assistant():
                lm += gen(name="result", temperature=temperature)

            return lm['result']
        
    #Error de decodificacion JSON           
    except json.JSONDecodeError:

        return {
            "success": False,
            "error": "Invalid JSON returned by LLM"
        }
    #Error general
    except Exception as e:

        return {
            "success": False,
            "error": str(e)
        }