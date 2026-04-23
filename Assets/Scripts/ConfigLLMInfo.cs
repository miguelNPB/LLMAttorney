using UnityEngine;

/**
 * Clase usada para configurar la informacion que sera pasada al LLM en un componente en concreto
 */
public class ConfigLLMInfo : MonoBehaviour
{

    public enum RagFiles
    {
        CodigoCivil,
        Precios,
        CasoBase
    }

    //Para mas informacion sobre las variables revisar LLMAttorney_API.cs

    [SerializeField]
    private API_TYPE _apiType;

    [SerializeField, TextArea(3, 10)]
    public string context;

    [SerializeField, TextArea(3, 10)]
    public string safeguard;

    [SerializeField, TextArea(3, 10)]
    public string safeguardSteps;

    [SerializeField, TextArea(3, 10)]
    public string historicalConversation;

    [SerializeField, TextArea(3, 10)]
    private string[] _stepChecks;

    [SerializeField, Range(0f, 1f)]
    private float _temperature;

    [SerializeField]
    private bool _ragUse;

    [SerializeField]
    private RagFiles _ragFile;

    public API_TYPE getApiType()
    {
        return _apiType;
    }

    public string[] getStepsChecks()
    {
        return _stepChecks;
    }

    public float getTemperature()
    {
        return _temperature;
    }

    public bool getRagUse()
    {
        return _ragUse;
    }

    public RagFiles getRagFileType()
    {
        return _ragFile;
    }
}
