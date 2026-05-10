import json
import guidance
from guidance import models, gen, select
from guidance import json as gen_json


# Crea la query y la ejecuta para Ollama
def sendOllamaQuery(prompt, LLMConfig, temperature, json_schema, modelName, ollamaEndpoint):
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
        return json.loads(lm['result'])
    else:
        with guidance.assistant():
            lm += gen(name="result", temperature=temperature)
        return lm['result']