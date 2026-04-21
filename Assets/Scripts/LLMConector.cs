using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class LLMConector : MonoBehaviour
{
    [SerializeField] protected ConfigLLMInfo[] _config;

    [Header("Search Messages")] public TMP_InputField inputField;

    [Header("Parametros de personalizacion")] [SerializeField]
    protected bool _useSecuritySteps = true;

    [SerializeField] protected bool _useHistoricalInContext = true;

    [SerializeField] protected bool _useHistoricalInSteps;

    protected JsonSchema _contextSchema = null;

    protected List<string> _historical = new();

    protected int _indexConfig;

    protected bool _promptSent;

    protected bool _schemasCreated = false;

    protected int _stepCounter;

    protected JsonSchema _stepsSchema = null;

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// Metodo encargado de recoger la respuesta del LLM y transmitirla a la clase que muestre el output de este
    /// </summary>
    /// <param name="success">muestra si ha podido obtenerse una respuesta del LLM</param>
    /// <param name="answer">texto plano que ha sacado el LLM como output</param>
    public abstract void RecieveChatMessage(bool success, string answer);

    /// <summary>
    /// Inicializacion de los schemas que devolvera el LLM
    /// </summary>
    protected abstract void createJsonSchemas();

    /// <summary>
    /// Llamada inicial al LLM para el prompt
    /// </summary>
    /// <param name="indexConfig">El archivo de configuracion a leer de la lista de posibles</param>
    /// <returns></returns>
    protected virtual bool SendContextMessage(int indexConfig = 0)
    {
        if (!_promptSent && _schemasCreated)
        {
            if (_config.Length <= 0)
            {
                Debug.LogError("Ningun Config LLM asignado");
                return false;
            }

            _indexConfig = indexConfig;

            var prompt = inputField.text;

            var configLLM = _config[_indexConfig].getContext()
                            + _config[_indexConfig].getSafeguard();

            if (_useHistoricalInContext)
            {
                configLLM = configLLM + "\n " + _config[_indexConfig].getHistoricalConversation() + "\n Historico: \n";

                foreach (var s in _historical) configLLM = configLLM + s + "\n";
            }

            Debug.Log("PROMPT: " + prompt);
            Debug.Log("CONTEXT: " + configLLM);

            _historical.Add("Pregunta: " + prompt);

            _promptSent = true;

            StartCoroutine(CoroutineSendPrompt(prompt, configLLM, _contextSchema));

            inputField.text = "";

            return true;
        }

        return false;
    }

    /// <summary>
    /// Llamada al LLM para la comprobacion por pasos de la respuesta
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    protected virtual bool SendSecuritySteps(string prompt)
    {
        Debug.Log("PROMPT de security checks: " + prompt);

        var configLLM = "Teniendo el siguiente texto: \n" + prompt +
                        "\n Y teniedo la siguiente directiva de seguridad" +
                        _config[_indexConfig].getSafeguardSteps() +
                        "\n Quiero que hagas lo siguiente: " + _config[_indexConfig].getStepsChecks()[_stepCounter];

        if (_useHistoricalInSteps)
            foreach (var s in _historical)
                configLLM = configLLM + s + "\n";

        StartCoroutine(CoroutineSendPromptSteps(prompt, configLLM, _stepsSchema));

        inputField.text = "";

        _stepCounter++;

        return true;
    }

    protected IEnumerator CoroutineSendPromptSteps(string prompt, string configLLM, JsonSchema schema)
    {
        float timer = 0;

        while (!LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, prompt, configLLM, schema,
                   _config[_indexConfig].getTemperature()))
        {
            timer += Time.deltaTime;

            yield return null;
        }
    }

    protected IEnumerator CoroutineSendPrompt(string prompt, string configLLM, JsonSchema schema)
    {
        float timer = 0;

        while (!LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, prompt, configLLM, schema,
                   _config[_indexConfig].getTemperature(), _config[_indexConfig].getRagUse(),
                   (int)_config[_indexConfig].getRagFileType()))
        {
            timer += Time.deltaTime;

            yield return null;
        }
    }
}