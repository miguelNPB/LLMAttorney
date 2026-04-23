using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class LLMConector : MonoBehaviour
{

    [SerializeField]
    protected ConfigLLMInfo[] _config;

    [Header("Search Messages")]
    [SerializeField] private TMP_InputField _inputField; 
    public string inputFieldText { get { return _inputField.text; } }
    

    [Header("Parametros de personalizacion")]

    [SerializeField]
    protected bool _useSecuritySteps = true;

    [SerializeField]
    protected bool _useHistoricalInContext = true;

    [SerializeField]
    protected bool _useHistoricalInSteps = false;

    protected List<String> _historical = new List<string>();

    protected int _stepCounter;

    protected bool _promptSent = false;

    protected int _indexConfig = 0;

    protected JsonSchema _contextSchema = null;

    protected JsonSchema _stepsSchema = null;

    protected bool _schemasCreated = false;

    /// <summary>
    /// Metodo encargado de recoger la respuesta del LLM y transmitirla a la clase que muestre el output de este
    /// </summary>
    /// <param name="success">muestra si ha podido obtenerse una respuesta del LLM</param>
    /// <param name="answer">texto plano que ha sacado el LLM como output</param>
    protected abstract void receiveResponse(bool success, string answer);

    /// <summary>
    /// Metodo que crea los esquemas y propiedades que debe devolver las llamadas al LLM
    /// </summary>
    protected abstract void createJsonSchemas();

    /// <summary>
    /// Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
    /// </summary>
    /// <param name="indexConfig">Archivo de configuracion a utilizar</param>
    /// <returns></returns>
    protected virtual bool sendContextPrompt(int indexConfig = 0)
    {

        if (!_promptSent && _schemasCreated)
        {

            if (_config.Length <= 0)
            {
                Debug.LogError("Ningun Config LLM asignado");
                return false;
            }

            _indexConfig = indexConfig;

            string prompt = _inputField.text;

            string configLLM = _config[_indexConfig].context
                + _config[_indexConfig].safeguard;

            if (_useHistoricalInContext)
            {
                configLLM = configLLM + "\n " + _config[_indexConfig].historicalConversation + "\n Historico: \n";

                foreach (String s in _historical)
                {
                    configLLM = configLLM + s + "\n";
                }
            }
            
            Debug.Log("PROMPT: " + prompt);
            Debug.Log("CONTEXT: " + configLLM);

            _historical.Add("Pregunta: " + prompt);

            _promptSent = true;

            StartCoroutine(coroutineSendPrompt(prompt, configLLM, _contextSchema));

            _inputField.text = "";

            return true;
        }

        return false;

    }

    /// <summary>
    /// Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
    /// </summary>
    /// <param name="indexConfig">Archivo de configuracion a utilizar</param>
    /// <returns></returns>
    protected virtual bool sendContextPrompt(string text, int indexConfig = 0)
    {

        if (!_promptSent && _schemasCreated)
        {

            if (_config.Length <= 0)
            {
                Debug.LogError("Ningun Config LLM asignado");
                return false;
            }

            _indexConfig = indexConfig;

            string prompt = text;

            string configLLM = _config[_indexConfig].context
                + _config[_indexConfig].safeguard;

            if (_useHistoricalInContext)
            {
                configLLM = configLLM + "\n " + _config[_indexConfig].historicalConversation + "\n Historico: \n";

                foreach (String s in _historical)
                {
                    configLLM = configLLM + s + "\n";
                }
            }

            Debug.Log("PROMPT: " + prompt);
            Debug.Log("CONTEXT: " + configLLM);

            _historical.Add("Pregunta: " + prompt);

            _promptSent = true;

            StartCoroutine(coroutineSendPrompt(prompt, configLLM, _contextSchema));

            return true;
        }

        return false;

    }

    /// <summary>
    /// Metodo encargado de revisar el texto obtenido como respuesta segun un array de directivas descritas, cambiandolo segun lo especificado
    /// </summary>
    /// <param name="prompt">texto que debe ser revisado</param>
    /// <returns></returns>
    protected virtual bool sendSecuritySteps(string prompt)
    {
        
        Debug.Log("PROMPT de security checks: " + prompt);

        string configLLM = "Teniendo el siguiente texto: \n" + prompt + "\n Y teniedo la siguiente directiva de seguridad" +
            _config[_indexConfig].safeguardSteps+
            "\n Quiero que hagas lo siguiente: " + _config[_indexConfig].getStepsChecks()[_stepCounter];

        if (_useHistoricalInSteps)
        {
            foreach (String s in _historical)
            {
                configLLM = configLLM + s + "\n";
            }
        }

        StartCoroutine(coroutineSendPromptSteps(prompt, configLLM, _stepsSchema));

        _inputField.text = "";

        _stepCounter++;

        return true;
    }

    protected IEnumerator coroutineSendPromptSteps(string prompt, string configLLM, JsonSchema schema)
    {

        float timer = 0;

        while (!LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, receiveResponse, prompt, configLLM, schema,
            _config[_indexConfig].getTemperature(), false))
        {
            timer += Time.deltaTime;

            yield return null;
        }

    }

    protected IEnumerator coroutineSendPrompt(string prompt, string configLLM, JsonSchema schema)
    {

        float timer = 0;

        while (!LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, receiveResponse, prompt, configLLM, schema,
            _config[_indexConfig].getTemperature(), _config[_indexConfig].getRagUse(), (int)_config[_indexConfig].getRagFileType()))
        {
            timer += Time.deltaTime;

            yield return null;
        }

    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
