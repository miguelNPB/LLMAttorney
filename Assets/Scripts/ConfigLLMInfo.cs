using UnityEngine;

/**
 * Clase usada para configurar la informacion que sera pasada al LLM en un componente en concreto
 */
public class ConfigLLMInfo : MonoBehaviour
{
    //Para mas informacion sobre las variables revisar LLMAttorney_API.cs

    [SerializeField]
    private API_TYPE _apiType;

    [SerializeField, TextArea(3, 10)]
    private string _context;

    [SerializeField, TextArea(3, 10)]
    private string _safeguard;

    [SerializeField, TextArea(3, 10)]
    private string _safeguardSteps;

    [SerializeField, TextArea(3, 10)]
    private string _historicalConversation;

    [SerializeField, TextArea(3, 10)]
    private string[] _stepChecks;

    [SerializeField, Range(0f, 1f)]
    private float _temperature;

    [SerializeField]
    private bool _ragUse;

    public API_TYPE getApiType()
    {
        return _apiType;
    }

    public string getContext()
    {
        return _context;
    }

    public string getSafeguard()
    {
        return _safeguard;
    }

    public string getSafeguardSteps()
    {
        return _safeguardSteps;
    }

    public string getHistoricalConversation()
    {
        return _historicalConversation;
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
}
