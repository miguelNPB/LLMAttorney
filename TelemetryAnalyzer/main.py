import sys
from pathlib import Path
import os
import pandas as pd
import matplotlib.pyplot as plt
import warnings
import matplotlib.ticker as ticker

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



# saca una grafica con dos barras, el numero de respuestas descartadas por no ser coherentes y las respuestas totales recividos
def analyze_not_consistent_questions(notConsistentQuestionEvents, queryRecievedEvents, plot_title):
    totalNotCoherentEvents = len(notConsistentQuestionEvents)
    totalRecievedEvents = len(queryRecievedEvents) + totalNotCoherentEvents
    if totalNotCoherentEvents == 0 or totalRecievedEvents == 0:
        percentOfNotCoherent = 0
    else:   
        percentOfNotCoherent = round((totalNotCoherentEvents * 100) / totalRecievedEvents, 1)

    categories = ['Respuestas descartadas por ser no coherentes', 'Respuestas totales recibidas']
    values = [totalNotCoherentEvents, totalRecievedEvents]
    colors = ['#4e79a7', '#f28e2b']
    plt.figure(figsize=(8, 6))
    plt.title(f"{plot_title} - Detección del LLM de respuestas no coherentes")
    bars = plt.bar(categories, values, color=colors)

    for bar in bars:
        yval = bar.get_height()
        plt.text(bar.get_x() + bar.get_width()/2, yval, f"{yval}", 
                 ha='center', va='bottom')

    plt.text(0.05, 0.95, f"% de no coherentes: {percentOfNotCoherent}", transform=plt.gca().transAxes, fontsize=12,
             verticalalignment='top', bbox=dict(boxstyle='round', facecolor='white', alpha=0.5))

    plt.savefig(RESULTS_DIR / f"{plot_title}_non_coherent_answer.jpg")
    plt.close()

# saca una grafica de puntos con los precios de cada presupuesto rechazados
def analyze_budget_attempts(deniedBudgetEvents, plot_title):
    if len(deniedBudgetEvents) < 1: 
        return

    prices = deniedBudgetEvents["6"].values
    x_axis = range(1, len(prices) + 1)

    plt.figure(figsize=(10, 6))
    
    plt.scatter(x_axis, prices, color='#4e79a7', s=100, alpha=0.7, edgecolors='black')

    for i, price in enumerate(prices):
        plt.text(x_axis[i], price + 0.1, f"{price}", ha='center', fontsize=9)

    plt.title(f"{plot_title} - Distribución de Presupuestos Rechazados")
    plt.xlabel("Número de intento (Evento)")
    plt.ylabel("Valor del presupuesto (Columna 6)")
    
    import matplotlib.ticker as ticker
    plt.gca().xaxis.set_major_locator(ticker.MaxNLocator(integer=True))

    plt.grid(axis='y', linestyle='--', alpha=0.7)
    plt.savefig(RESULTS_DIR / f"{plot_title}_denied_budgets_scatter.jpg")
    plt.close()


# saca una grafica con 4 barras, cada una con el numero de documentos obtenidos de cada tipo posible
def analyze_asked_document_types(askedDocumentEvents, plot_title):
    if len(askedDocumentEvents) < 1:
        return
    
    totalPerito = len(askedDocumentEvents[askedDocumentEvents["7"] == 0])
    totalReport = len(askedDocumentEvents[askedDocumentEvents["7"] == 1])
    totalWitness = len(askedDocumentEvents[askedDocumentEvents["7"] == 2])
    totalReceiptFacture = len(askedDocumentEvents[askedDocumentEvents["7"] == 3])

    categories = ['Peritos', 'Informes', 'Testimonios', 'Facturas']
    values = [totalPerito, totalReport, totalWitness, totalReceiptFacture]
    colors = ['#4e79a7', '#f28e2b', "#2bf25d", "#f22b7e"]
    
    plt.figure(figsize=(8, 6))
    plt.title(f"{plot_title} - Tipos de documentos obtenidos")
    bars = plt.bar(categories, values, color=colors)

    for bar in bars:
        yval = bar.get_height()
        plt.text(bar.get_x() + bar.get_width()/2, yval, f"{yval}", 
                 ha='center', va='bottom')

    plt.gca().yaxis.set_major_locator(ticker.MaxNLocator(integer=True))

    plt.savefig(RESULTS_DIR / f"{plot_title}_asked_document_types.jpg")
    plt.close()


# saca una grafica con 2 barras, una con la cantidad de documentos mandados al procurador y otra con todos los documentos obtenidos
def analyze_sent_procurator_docs(postDocumentEvents, askedDocumentEvents, plot_title):
    totalSentToProcuratorDocs = len(postDocumentEvents)
    totalAskedDocs = len(askedDocumentEvents)

    categories = ['Documentos mandados al procurador', 'Documentos pedidos']
    values = [totalSentToProcuratorDocs, totalAskedDocs]
    colors = ['#4e79a7', '#f28e2b']
    plt.figure(figsize=(8, 6))
    plt.title(f"{plot_title} - Relación de documentos pedidos y documentos utilizados")
    bars = plt.bar(categories, values, color=colors)

    for bar in bars:
        yval = bar.get_height()
        plt.text(bar.get_x() + bar.get_width()/2, yval, f"{yval}", 
                 ha='center', va='bottom')
        
    plt.gca().yaxis.set_major_locator(ticker.MaxNLocator(integer=True))

    plt.savefig(RESULTS_DIR / f"{plot_title}_sent_procurator_docs.jpg")
    plt.close()
    

# Sacar una grafica con 4 barras con el numero de peticiones por fase
def analyze_num_sent_querys(queryPostEvents, plot_title):
    num_sent_querys_phase1 = len(queryPostEvents[queryPostEvents["5"] == 1])
    num_sent_querys_phase2 = len(queryPostEvents[queryPostEvents["5"] == 2])
    num_sent_querys_phase3 = len(queryPostEvents[queryPostEvents["5"] == 3])
    num_sent_querys_phase4 = len(queryPostEvents[queryPostEvents["5"] == 4])

    categories = ['Fase 1', 'Fase 2', 'Fase 3', 'Fase 4']
    values = [num_sent_querys_phase1, num_sent_querys_phase2, num_sent_querys_phase3, num_sent_querys_phase4]
    colors = ['#4e79a7', '#f28e2b', "#2bf25d", "#f22b7e"]
    plt.figure(figsize=(8, 6))
    plt.title(f"{plot_title} - Número de peticiones enviadas al LLM")
    bars = plt.bar(categories, values, color=colors)

    for bar in bars:
        yval = bar.get_height()
        plt.text(bar.get_x() + bar.get_width()/2, yval, f"{yval}", 
                 ha='center', va='bottom')


    plt.savefig(RESULTS_DIR / f"{plot_title}_sent_querys.jpg")
    plt.close()


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

def main():
    if len(sys.argv) < 2:
        print("Falta argumento para el texto de titulo de las graficas")
        return

    plot_title = sys.argv[1]

    check_data_folders()

    database = get_data()

    ''' Codigos de atributos
            eventType = 0,
            sessionID = 1,
            userID = 2,
            timeStamp = 3,
            messageID = 4,
            phaseID = 5,
            price = 6,
            documentType = 7,
            isValid = 8,
    '''

    notConsistentQuestionEvents = database[database["eventType"] == 0]
    deniedBudgetEvents = database[database["eventType"] == 1]
    postDocumentEvents = database[database["eventType"] == 2]
    askedDocumentEvents = database[database["eventType"] == 3]
    queryPostEvents = database[database["eventType"] == 4]
    queryRecievedEvents = database[database["eventType"] == 5]

    analyze_not_consistent_questions(notConsistentQuestionEvents, queryRecievedEvents, plot_title)
    analyze_budget_attempts(deniedBudgetEvents, plot_title)
    analyze_asked_document_types(askedDocumentEvents, plot_title)
    analyze_sent_procurator_docs(postDocumentEvents, askedDocumentEvents, plot_title)
    analyze_num_sent_querys(queryPostEvents, plot_title)
    analyze_time_between_query_and_response(queryPostEvents, queryRecievedEvents, plot_title)


if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\nForce interrupt")
        sys.exit(0)