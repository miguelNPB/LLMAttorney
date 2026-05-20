from unitySceneReconstructor import build_event_points_by_level
import seaborn as sns
import matplotlib.pyplot as plt
import pandas as pd
import numpy as np
from pathlib import Path
from matplotlib.colors import ListedColormap
from matplotlib.patches import Patch

RESULTS_DIR = Path(__file__).resolve().parent / "results"
RESULTS_DIR.mkdir(parents=True, exist_ok=True)

DEFAULT_LAYER_COLORS = {
    "KillOnCollide": "#FFCF4B",
}

def _plot_optional_events(axis, level_id, event_points_by_level=None):
    # Dibuja puntos de evento solo si se reciben datos para el nivel.
    if not event_points_by_level:
        return

    level_events = event_points_by_level.get(level_id, [])
    for event in level_events:
        x = event.get("x")
        y = event.get("y")
        if x is None or y is None:
            continue

        marker = event.get("marker", "o")
        edgecolor = event.get("edgecolor", "none")
        scatter_kwargs = {
            "c": event.get("color", "black"),
            "marker": marker,
            "s": event.get("size", 36),
            "alpha": event.get("alpha", 0.9),
            "label": event.get("label", None),
        }

        # Evitamos warning de matplotlib con marcadores no rellenables como 'x'.
        if marker not in {"x", "+", "1", "2", "3", "4", "|", "_"}:
            scatter_kwargs["edgecolors"] = edgecolor

        axis.scatter(x, y, **scatter_kwargs)

def plot_reconstructed_grids(
    scene_data,
    matrix_data,
    max_cols=1,
    include_empty_layers=False,
    margin_cells=1.0,
    event_points_by_level=None,
    show_event_legend=False,
    represented_data_label=None,
    layer_colors=None,
    palette_name="tab10",
    file_output_name=""
    ):
    # Funcion principal de render por nivel.
    level_ids = sorted(scene_data.keys())
    if not level_ids:
        raise ValueError("No hay niveles parseados para visualizar")

    # Combina colores por defecto con overrides opcionales del usuario.
    resolved_layer_colors = dict(DEFAULT_LAYER_COLORS)
    if layer_colors:
        resolved_layer_colors.update(layer_colors)

    max_cols = max(1, max_cols)
    rows = int(np.ceil(len(level_ids) / max_cols))
    axes = plt.subplots(rows, max_cols, figsize=(12 * max_cols, 4.8 * rows), squeeze=False)[1]
    axes_flat = axes.ravel()

    for axis, level_id in zip(axes_flat, level_ids):
        level_data = scene_data[level_id]
        bounds = matrix_data[level_id]["bounds"]
        min_x, max_x, min_y, max_y = bounds

        layer_items = list(level_data["layers"].items())
        palette = plt.get_cmap(palette_name, max(1, len(layer_items)))

        legend_handles = []
        plotted_layers = 0

        # Pintamos cada capa con color configurable por nombre de capa.
        for layer_index, (layer_file_id, layer_info) in enumerate(layer_items):
            matrix = matrix_data[level_id]["layer_matrices"][layer_file_id]
            if matrix.size == 0:
                continue
            if not include_empty_layers and matrix.sum() == 0:
                continue

            layer_label = layer_info["name"] if layer_info["name"] else f"Layer {layer_file_id}"
            layer_color = resolved_layer_colors.get(layer_label, palette(layer_index))
            mask = np.ma.masked_where(matrix == 0, matrix)

            axis.imshow(
                mask,
                origin="lower",
                interpolation="nearest",
                extent=(min_x - 0.5, max_x + 0.5, min_y - 0.5, max_y + 0.5),
                cmap=ListedColormap([layer_color]),
                alpha=0.72,
            )

            legend_handles.append(Patch(facecolor=layer_color, edgecolor="none", label=layer_label))
            plotted_layers += 1

        # Hook para eventos telemetricos (muertes, fin iteracion, etc.).
        _plot_optional_events(axis, level_id, event_points_by_level=event_points_by_level)

        grid_pos_x, grid_pos_y, _ = level_data["grid"]["position"]
        title_parts = [f"Nivel {level_id}", f"Grid Pos: ({grid_pos_x:.2f}, {grid_pos_y:.2f})"]
        if represented_data_label:
            title_parts.append(represented_data_label)
        axis.set_title(" | ".join(title_parts))
        axis.set_xlabel("Grid X")
        axis.set_ylabel("Grid Y")
        axis.set_aspect("equal")
        axis.set_xlim(min_x - 0.5 - margin_cells, max_x + 0.5 + margin_cells)
        axis.set_ylim(min_y - 0.5 - margin_cells, max_y + 0.5 + margin_cells)
        axis.grid(True, color="lightgray", linewidth=0.3, alpha=0.4)

        if legend_handles:
            axis.legend(handles=legend_handles, loc="upper right", fontsize=8, framealpha=0.95)
        if show_event_legend and event_points_by_level:
            axis.legend(loc="upper left", fontsize=8, framealpha=0.95)
        if plotted_layers == 0:
            axis.text(0.5, 0.5, "Sin capas con tiles", ha="center", va="center", transform=axis.transAxes)

    # Ocultamos ejes sobrantes cuando la rejilla de subplots tiene huecos.
    for axis in axes_flat[len(level_ids):]:
        axis.axis("off")

    plt.tight_layout()
    plt.savefig(RESULTS_DIR / f"spatial_{file_output_name}.png")
    plt.close()

# Render de la metrica espacial de muertes del jugador.
def render_spatial_metric_player_deaths(deathDB, scene_grid_data, level_grid_matrices):

    death_points_by_level = build_event_points_by_level(
        events_df=deathDB,
        scene_data=scene_grid_data,
        color="#FF3300",
        marker="x",
        label="Muerte",
        size=50,
        alpha=1.0,
        edgecolor="white",
        )

    plot_reconstructed_grids(
        scene_data=scene_grid_data,
        matrix_data=level_grid_matrices,
        max_cols=1,
        include_empty_layers=False,
        margin_cells=1.0,
        event_points_by_level=death_points_by_level,
        show_event_legend=False,
        represented_data_label="Death Points",
        file_output_name="death_points"
        )
    

def render_spatial_metric_player_iteration_points(endIterationDB, scene_grid_data, level_grid_matrices):
    # Render de la metrica espacial de fin de iteracion.
    end_iteration_points_by_level = build_event_points_by_level(
    events_df=endIterationDB,
    scene_data=scene_grid_data,
    color="#00FF1A",
    marker="o",
    label="Fin iteracion",
    size=50,
    alpha=1.0,
    edgecolor="white",
    )

    plot_reconstructed_grids(
        scene_data=scene_grid_data,
        matrix_data=level_grid_matrices,
        max_cols=1,
        include_empty_layers=False,
        margin_cells=1.0,
        event_points_by_level=end_iteration_points_by_level,
        show_event_legend=False,
        represented_data_label="End Iteration Points",
        file_output_name="end_iteration_points"
        )
    

def render_spatial_metric_failure_points(detFailureDB, scene_grid_data, level_grid_matrices):
    # Render de la metrica espacial de fallos de determinismo.
    det_failure_points_by_level = build_event_points_by_level(
        events_df=detFailureDB,
        scene_data=scene_grid_data,
        color="#FF00D4",
        marker="^",
        label="Fallo determinismo",
        size=50,
        alpha=1.0,
        edgecolor="white",
        )

    plot_reconstructed_grids(
        scene_data=scene_grid_data,
        matrix_data=level_grid_matrices,
        max_cols=1,
        include_empty_layers=False,
        margin_cells=1.0,
        event_points_by_level=det_failure_points_by_level,
        show_event_legend=False,
        represented_data_label="Determinism Failure Points",
        file_output_name="determinism_failure_points"
        )
    
def render_abandonment_rate_by_level(leftLevelDB):
    ax = sns.countplot(x="levelID", data=leftLevelDB, hue="levelID", palette = "crest", order=sorted(leftLevelDB["levelID"].unique()))
    ax.set_title("Tasa de abandono de nivel")
    ax.tick_params(axis='x', rotation=0)
    plt.savefig(RESULTS_DIR / "abandonment_rate_by_level.png")
    plt.close()

def render_abandoment_rate_game(leftGameDB):
    ax = sns.countplot(x="levelID", data=leftGameDB, hue="levelID", palette = "crest", order=sorted(leftGameDB["levelID"].unique()))
    ax.set_title("Tasa de abandono de juego")
    ax.tick_params(axis='x', rotation=0)
    plt.savefig(RESULTS_DIR / "abandonment_rate_game.png")
    plt.close()

def render_iteration_rate_by_level(endIterationDB, levelIdsValues):
    user_counts = (
                endIterationDB
                .groupby(["sessionID", "levelID"])
                .size()
                .reset_index(name="count")
    )

    mean_counts = (
        user_counts
        .groupby("levelID")["count"]
        .mean()
        .reset_index()
    )

    plt.figure()
    ax = sns.barplot(x="levelID", y="count", data=mean_counts, palette="crest")

    ax.set_title(f"Media de iteraciones por sesion en niveles")
    ax.set_xlabel("levelID")
    ax.set_ylabel("Media de iteraciones")

    plt.savefig(RESULTS_DIR / "iteration_rate_by_level_general.png")
    plt.close()

    endIterationLevelsDS = []
    for level in levelIdsValues:
        endIterationLevelsDS.append(endIterationDB[endIterationDB["levelID"] == level])

    for dataset in endIterationLevelsDS:

        if not dataset.empty:
             #Contamos el numero de usuarios
            user_counts = (
                dataset
                .groupby(["sessionID", "shadowID"])
                .size()
                .reset_index(name="count")
            )

            #Sacamos la media de interaccioones con cada palanca
            mean_counts = (
                user_counts
                .groupby("shadowID")["count"]
                .mean()
                .reset_index()
            )

            #Hacemos el barplot para mostrarlo
            plt.figure()
            ax = sns.barplot(x="shadowID", y="count", data=mean_counts, palette="crest")

            level_id = dataset["levelID"].iloc[0]
            ax.set_title(f"Media de iteraciones por sombras por sesion en nivel {level_id}")
            ax.set_xlabel("shadowID")
            ax.set_ylabel("Media de iteraciones")

            plt.savefig(RESULTS_DIR / f"iteration_rate_shadow_level_{level_id}.png")
            plt.close()


def render_interaction_with_interactables_rate_by_level(buttonPressDB, leverActionDB):
    # Preparamos ambos datasets añadiendo una columna de tipo para diferenciarlos al combinarlos.
    buttonPressDB["type"] = "Boton"
    leverActionDB["type"] = "Palanca"

    combined = pd.concat([buttonPressDB, leverActionDB])

    #Conteo por usuario, nivel y tipo
    user_counts = (
        combined
        .groupby(["sessionID", "levelID", "type"])
        .size()
        .reset_index(name="count")
    )

    #Media por nivel y tipo
    mean_counts = (
        user_counts
        .groupby(["levelID", "type"])["count"]
        .mean()
        .reset_index()
    )

    #Sacamos el grafico comparativo entre ambos
    plt.figure()
    ax = sns.barplot(
        x="levelID",
        y="count",
        hue="type",
        data=mean_counts,
        palette=["#4CA7AF", "#C64444"],
        order=sorted(mean_counts["levelID"].unique())
    )

    ax.set_title("Media de interacciones por jugador en elementos activables por nivel")
    ax.set_xlabel("levelID")
    ax.set_ylabel("Media de interacciones")

    plt.savefig(RESULTS_DIR / "interaction_interactables_rate.png")
    plt.close()


def render_interaction_with_button_by_level(buttonPressDB, levelIdsValues):
    buttonPressLevelsDS = []

    for level in levelIdsValues:
        buttonPressLevelsDS.append(buttonPressDB[buttonPressDB["levelID"] == level])

    for dataset in buttonPressLevelsDS:
        if not dataset.empty:
            #Contamos el numero de usuarios
            user_counts = (
                dataset
                .groupby(["sessionID", "buttonID"])
                .size()
                .reset_index(name="count")
            )

            #Sacamos la media de interaccioones con cada palanca
            mean_counts = (
                user_counts
                .groupby("buttonID")["count"]
                .mean()
                .reset_index()
            )

            #Hacemos el barplot para mostrarlo
            plt.figure()
            ax = sns.barplot(x="buttonID", y="count", data=mean_counts, palette="crest")

            level_id = dataset["levelID"].iloc[0]
            ax.set_title(f"Media de interacciones con botones por sesion en nivel {level_id}")
            ax.set_xlabel("buttonID")
            ax.set_ylabel("Media de interacciones")

            plt.savefig(RESULTS_DIR / f"interaction_button_level_{level_id}.png")
            plt.close()

def render_interaction_with_levers_by_level(leverActionDB, levelIdsValues):
    leverActionLevelsDS = []

    for level in levelIdsValues:
        leverActionLevelsDS.append(leverActionDB[leverActionDB["levelID"] == level])

    for dataset in leverActionLevelsDS:
        if not dataset.empty:

            #Contamos el numero de usuarios
            user_counts = (
                dataset
                .groupby(["sessionID", "leverID"])
                .size()
                .reset_index(name="count")
            )

            #Sacamos la media de interaccioones con cada palanca
            mean_counts = (
                user_counts
                .groupby("leverID")["count"]
                .mean()
                .reset_index()
            )

            #Hacemos el barplot para mostrarlo
            plt.figure()
            ax = sns.barplot(x="leverID", y="count", data=mean_counts, palette="crest")

            level_id = dataset["levelID"].iloc[0]
            ax.set_title(f"Media de interacciones con palancas por sesion en nivel {level_id}")
            ax.set_xlabel("leverID")
            ax.set_ylabel("Media de interacciones")

            plt.savefig(RESULTS_DIR / f"interaction_lever_level_{level_id}.png")
            plt.close()

def render_time_to_complete_level(levelStartDB, levelEndDB):

    # Sacamos los tiempos de inicio y fin, renombrando la columna de tiempo para diferenciarlas 
    start = levelStartDB[["sessionID", "levelID", "timestamp"]].rename(columns={"timestamp": "start_time"})
    end = levelEndDB[["sessionID", "levelID", "timestamp"]].rename(columns={"timestamp": "end_time"})

    #Combinamos ambos datasets por usuario y nivel para calcular el tiempo de completado
    merged = pd.merge(start, end, on=["sessionID", "levelID"])

    # Calculamos el tiempo de completado restando el tiempo de inicio al tiempo de fin
    merged["completion_time"] = merged["end_time"] - merged["start_time"]
    # Nos aseguramos que no hay niveles donde el usuario se haya salido sin completarlo
    merged = merged[merged["completion_time"].dt.total_seconds() >= 0]

    #Pasamos el tiempo a segundos
    merged["completion_time_sec"] = merged["completion_time"].dt.total_seconds()

    #Calculamos la media de tiempo de completado por nivel
    mean_times = (
        merged
        .groupby("levelID")["completion_time_sec"]
        .mean()
        .reset_index()
    )

    #Hacemos la grafica
    plt.figure()
    ax = sns.barplot(
        x="levelID",
        y="completion_time_sec",
        data=mean_times,
        palette="crest"
    )

    # Añadimos las etiquetas de tiempo encima de cada barra
    for container in ax.containers:
        ax.bar_label(container, fmt="%.2f s", padding=3)

    #Dejamos espacio para la barra mas alta
    max_val = mean_times["completion_time_sec"].max()
    if (np.isfinite(max_val)):
        ax.set_ylim(0, max_val * 1.15)

        ax.set_title("Tiempo medio de completar nivel")
        ax.set_xlabel("levelID")
        ax.set_ylabel("Tiempo (segundos)")

    plt.savefig(RESULTS_DIR / "level_completion_time.png")
    plt.close()

def render_time_to_complete_level_compare_to_ideal_time(levelStartDB, levelEndDB):

    # Sacamos los tiempos de inicio y fin, renombrando la columna de tiempo para diferenciarlas 
    start = levelStartDB[["sessionID", "levelID", "timestamp"]].rename(columns={"timestamp": "start_time"})
    end = levelEndDB[["sessionID", "levelID", "timestamp"]].rename(columns={"timestamp": "end_time"})

    #Combinamos ambos datasets por usuario y nivel para calcular el tiempo de completado
    merged = pd.merge(start, end, on=["sessionID", "levelID"])

    # Calculamos el tiempo de completado restando el tiempo de inicio al tiempo de fin
    merged["completion_time"] = merged["end_time"] - merged["start_time"]
    # Nos aseguramos que no hay niveles donde el usuario se haya salido sin completarlo
    merged = merged[merged["completion_time"].dt.total_seconds() >= 0]

    #Pasamos el tiempo a segundos
    merged["completion_time_sec"] = merged["completion_time"].dt.total_seconds()

    #Calculamos la media de tiempo de completado por nivel
    mean_times = (
        merged
        .groupby("levelID")["completion_time_sec"]
        .mean()
        .reset_index()
    )

    ideal_times = pd.DataFrame({
        "levelID": list(range(1, 11)),
        "completion_time_sec": [10, 16, 21, 16, 19, 29, 23, 57, 46, 37]
    })

    mean_times["tipo"] = "Mean"
    ideal_times["tipo"] = "Ideal"

    combined = pd.concat([mean_times, ideal_times])

    #Hacemos la grafica
    plt.figure()
    ax = sns.barplot(
        x="levelID",
        y="completion_time_sec",
        hue="tipo",
        data=combined,
        palette="crest"
    )

    # Añadimos las etiquetas de tiempo encima de cada barra
    for container in ax.containers:
        ax.bar_label(container, fmt="%.2f s", padding=3)

    #Dejamos espacio para la barra mas alta
    max_val = combined["completion_time_sec"].max()
    ax.set_ylim(0, max_val * 1.15)

    ax.set_title("Tiempo medio de completar nivel comparado con tiempo ideal")
    ax.set_xlabel("levelID")
    ax.set_ylabel("Tiempo (segundos)")

    plt.savefig(RESULTS_DIR / "level_completion_time_compare_to_ideal.png")
    plt.close()
