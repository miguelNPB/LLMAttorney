using UnityEngine;

/// <summary>
/// Clase hija de ScriptableObject encargada de almacenar las directivas y prompts que configuran la llamada que se hace a los LLM. 
/// La clase se divide en los siguientes apartados los cuales configuran la query enviada al LLM:
/// 
/// * Context: texto encargado de informar sobre el entorno y la situación en la que se encuentra el LLM. Debe indicar en rol que debe asumir, el tono
///     que debe mantener y las limitaciones que pueda llegar a tener su personaje. Tambien hara referencia a que uso tiene el RAG en el 
///     propio contexto.
///     
/// * Safeguard: directiva la cual configura el formato en el que se debe devolver el mensaje. No solo menciona los atributos que se deben de rellenar
///     sino que también bajo que criterio se debe de hacer, estableciendo formatos, limitaciones y especificaciones a la hora de rellenarlos.
///     
/// * HistoryHeader: texto que indica al LLM como se debe de hacer uso del historico, pudiendo establecer que se uso como referencia, que se replique
///     lo que salga en este o que simplemente revise conversaciones pasadas para salir de dudas. La omision de esta directiva no implica no 
///     usar el historico, solo no marca como hacer uso de este.
///     
/// * Steps: pasos de validación, arreglo y reformato del texto generado. Estas directivas permiten llevar a cabo el chain of though, alimentando al LLM
///     con los output que genero anteriormente, enfocando su trabajo a revisar una cosa en especifico. Por ejemplo que compruebe que el texto esta en
///     un idioma concreto o que se hace mención a palabras clave.
///     
/// * StepSafeguard: directiva con el mismo objetivo de formato que el safeguard pero enfocada a los steps. Debido a la diferencia de objetivos entre la
///     llamada inicial y los steps es importante declarar dos prompts distintos.
///     
/// * Temperature: marca el grado de creatividad que se le da al LLM. 0 significa que debe ser muy estricto con las reglas impuestas y 1 que puede ser
///     muy creativo al generar su respuesta.
///     
/// * RagFileUse: marca si se quiere hacer uso de la arquitectura RAG para usar un archivo como base de contexto o no.
/// 
/// * RagFile: archivo el cual se quiere usar siguiendo la arquitectura RAG.
/// 
/// </summary>
[CreateAssetMenu(fileName = "LLMConfigNew", menuName = "LLMAttorney/Crear LLMConfig", order = 1)]
public class LLMConfig : ScriptableObject
{
    public enum RagFiles
    {
        CasoBase, // caso base debe siempre ser el primero = 0
        CodigoCivil,
        Precios,
    }

    [Header("Configuración inicial")]

    [SerializeField, TextArea(3, 10)]
    [Tooltip("Contexto que utilizará el LLM de ayuda para responder el prompt.")]
    private string _context;

    [SerializeField, TextArea(3, 10)]
    [Tooltip("Escribe aquí la de seguridad para el comportamiento del modelo.")]
    private string _safeguard;


    [SerializeField, TextArea(3, 10)]
    [Tooltip("Header para el historico")]
    private string _historicHeader;

    [Header("Chain Of Thought")]

    [SerializeField, TextArea(3, 10)]
    [Tooltip("Contexto para cada paso de la cadena de chain of thought")]
    private string[] _stepChecks;

    [SerializeField, TextArea(3, 10)]
    [Tooltip("Directiva de seguridad para cada paso de la cadena de chain of thought")]
    private string _stepsCommonSafeguard;

    [Header("Parámetros")]

    [SerializeField, Range(0f, 1f)]
    [Tooltip("Controla la creatividad del modelo. 0 es determinista, 1 es muy creativo.")]
    private float _temperature;

    [Header("RAG")]

    [SerializeField]
    [Tooltip("Activa o desactiva el uso de Recuperación Aumentada por Generación (RAG).")]
    private bool _ragUse;

    [SerializeField]
    [Tooltip("Selecciona el archivo o base de conocimiento que utilizará el RAG.")]
    private RagFiles _ragFile;

    //Getters de los distintos atributos
    public string GetContext()
    {
        return _context;
    }

    public string GetSafeguard()
    {
        return _safeguard;
    }

    public string GetSafeguardSteps()
    {
        return _stepsCommonSafeguard;
    }

    public string GetHistoricHeader()
    {
        return _historicHeader;
    }

    public string[] GetStepChecks()
    {
        return _stepChecks;
    }

    public float GetTemperature()
    {
        return _temperature;
    }

    public bool GetRagUse()
    {
        return _ragUse;
    }

    public RagFiles GetRagFileType()
    {
        return _ragFile;
    }
}