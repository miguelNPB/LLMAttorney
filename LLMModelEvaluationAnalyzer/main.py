import sys
from pathlib import Path
import os
import pandas as pd
import matplotlib.pyplot as plt
import warnings
import numpy as np

# Ignora los futureWarnings en consola
warnings.simplefilter(action='ignore', category=FutureWarning)

DATA_DIR = Path(__file__).resolve().parent / "data"
RESULTS_DIR = Path(__file__).resolve().parent / "results"

# comprueba si las carpetas existen y si hay datos
def check_data_folders():
    # Comprobar si existe la carpeta, si no existe, crearla y salir
    if not DATA_DIR.exists():
        DATA_DIR.mkdir(parents=True, exist_ok=True)
        print("Error, no hay archivos de datos, añadir datos .json a la carpeta /data.")
        sys.exit(1)

    # Comprobar si hay archivos
    has_files = any(f.is_file() for f in DATA_DIR.iterdir())

    if not has_files:
        print("Error, no hay archivos de datos, añadir datos .json a la carpeta /data.")
        sys.exit(1)

    # Check carpeta results
    if not RESULTS_DIR.exists():
        RESULTS_DIR.mkdir(parents=True, exist_ok=True)

# concatena los json de la carpeta data y saca un dataframe con los datos
def get_data():
    dataframes = []

    for archivo in os.listdir(DATA_DIR):
        #Carga JSON
        if archivo.endswith(".json"):
            df = pd.read_json(os.path.join(DATA_DIR, archivo), orient="records")
            dataframes.append(df)


    database = pd.concat(dataframes)

    return database

# Sacar una grafica con 4 barras con tiempos medios entre peticion y respuesta por fase
def analyze_time_between_query_and_response(queryPostEvents, queryRecievedEvents, plot_title):
    queryPostEvents["4"] = queryPostEvents["4"].astype(str)
    queryRecievedEvents["4"] = queryRecievedEvents["4"].astype(str)

    merged_df = pd.merge(
        queryPostEvents, 
        queryRecievedEvents, 
        on="4", 
        suffixes=('_sent', '_rec')
    )

    if merged_df.empty:
        print("No hay coincidencias de mensajes para calcular tiempos.")
        return

    merged_df['latency'] = round((merged_df['timestamp_rec'] - merged_df['timestamp_sent']).dt.total_seconds(), 1) 

    
    means = merged_df.groupby("5_sent")['latency'].mean()
    stds = merged_df.groupby("5_sent")['latency'].std().fillna(0)


    categories = ['Fase 1', 'Fase 2', 'Fase 3', 'Fase 4']
    mean_values = [means.get(i, 0) for i in [1, 2, 3, 4]]
    
    plt.figure(figsize=(10, 6))
    plt.title(f"{plot_title} - Segundos medios entre petición y respuesta enviados al LLM")
    colors = ['#4e79a7', '#f28e2b', "#2bf25d", "#f22b7e"]
    bars = plt.bar(categories, mean_values, color=colors)

    i = 1
    y_Limit = means.max()
    offset_up = y_Limit * 0.025
    offset_down = y_Limit * 0.125
    threshold = y_Limit * 0.9
    for bar in bars:
        yval = bar.get_height()
        text_pos = yval + offset_up
        if text_pos > threshold:
            text_pos = yval - offset_down
            
        plt.text(bar.get_x() + bar.get_width()/2, text_pos, f"Media: {means.get(i, 0):.2f}\nDesv: {stds.get(i, 0):.2f}", 
                 ha='center', va='bottom', bbox=dict(boxstyle='round', facecolor='white', alpha=0.5))
        i = i + 1

    plt.savefig(RESULTS_DIR / f"{plot_title}_response_times.jpg")
    plt.close()

# Metodo usado para el quality score de las puntuaciones de modelos, no usado con telemetria
def analyze_quality_answer_score(qualityAnswerScore, plot_title):
    if len(qualityAnswerScore) < 1:
        print("No hay contenido para analizar")
        return 
    
    phases = [1, 2, 3, 4]
    categories = ['Fase 1', 'Fase 2', 'Fase 3', 'Fase 4']
    colors = ['#4e79a7', '#f28e2b', "#2bf25d", "#f22b7e"]
    
    means = []
    stds = []

    for phase in phases:
        phase_data = qualityAnswerScore[qualityAnswerScore["phase"] == phase]["score"]
        means.append(phase_data.mean() if not phase_data.empty else 0)
        stds.append(phase_data.std() if not phase_data.empty else 0)

    plt.figure(figsize=(8, 6))
    plt.title(f"{plot_title} - Media de calidad de respuesta")
    
    bars = plt.bar(categories, means, color=colors, capsize=7, ecolor='black')

    plt.ylim(0, 5.5)
    plt.yticks(range(6))

    i = 0
    y_Limit = 5
    offset_up = y_Limit * 0.035
    offset_down = y_Limit * 0.175
    threshold = y_Limit * 0.9
    for bar in bars:
        yval = bar.get_height()
        text_pos = yval + offset_up
        if text_pos > threshold:
            text_pos = yval - offset_down
            
        plt.text(bar.get_x() + bar.get_width()/2, text_pos, f"Media: {means[i]:.2f}\nDesv: {stds[i]:.2f}", 
                 ha='center', va='bottom', bbox=dict(boxstyle='round', facecolor='white', alpha=0.5))
        i = i + 1
        
    finalScore = round(np.nanmean(means), 1)
    plt.text(0.05, 0.95, f"Puntuación general: {finalScore}", transform=plt.gca().transAxes, fontsize=12,
             verticalalignment='top', bbox=dict(boxstyle='round', facecolor='white', alpha=0.5))

    plt.tight_layout()
    plt.savefig(RESULTS_DIR / f"{plot_title}_quality_score.jpg")
    plt.close()


def main():
    if len(sys.argv) < 2:
        print("Falta argumento para el texto de titulo de las graficas")
        return

    plot_title = sys.argv[1]

    check_data_folders()

    database = get_data()

    queryPostEvents = database[database["eventType"] == 4]
    queryRecievedEvents = database[database["eventType"] == 5]
    qualityAnswerScore = database[database["eventType"] == 7]

    analyze_time_between_query_and_response(queryPostEvents, queryRecievedEvents, plot_title)
    analyze_quality_answer_score(qualityAnswerScore, plot_title)
    print(RESULTS_DIR)


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\nForce interrupt")
        sys.exit(0)