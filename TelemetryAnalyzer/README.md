# LLMAttorney - Telemetría

En esta carpeta está el código del analizador de Python para sacar gráficas sobre trazas de la telemetría.

### Donde encontrar las trazas json

Se encuentran en `C:\Users\{usuario}\AppData\LocalLow\LLMAttorney\LLMAttorney`.  
Ejemplo de nombre de traza: `telemetry_events_309_671932645.json`

### Como analizarla

Los archivos json con trazas deben ser colocados en la carpeta `TelemetryAnalyzer/data` y los resultados saldrán en la carpeta `TelemetryAnalyzer/results`  
Además, al ejecutar el main.py, es necesario pasar un argumento con el nombre de los títulos de las gráficas, por ejemplo: `python main.py "Estudiantes Derecho"`  
