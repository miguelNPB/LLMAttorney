# LLMAttorney - Analizador de evaluación de modelos LLM

En esta carpeta se en cuentra el script de python para poder analizar y sacar gráficas de los datos obtenidos.  

Se usan las trazas de telemetría obtenidas para analizar los eventos de los tiempos, y además se añaden unas trazas extra con las puntuaciones subjetivas del investigador de las respuestas del LLM.   
Estas puntuaciones tienen `eventType == 7`, en el campo `phase` que va del 1 al 4 para distinguir entre fases y el campo `score`, con la puntuación del 1 al 5. Un ejemplo:
```
{
"phase": 2,
"score": 4,
"eventType": 7
}
```

### Como analizar

Los archivos json con trazas deben ser colocados en la carpeta `LLMModelEvaluationAnalyzer/data` y los resultados saldrán en la carpeta `LLMModelEvaluationAnalyzer/results`  
Además, al ejecutar el main.py, es necesario pasar un argumento con el nombre de los títulos de las gráficas, por ejemplo: `python main.py "Qwen 2.5"`  