using UnityEngine;

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