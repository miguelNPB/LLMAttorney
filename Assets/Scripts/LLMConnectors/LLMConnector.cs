using System;
using UnityEngine;

public abstract class LLMConnector : MonoBehaviour
{
    [SerializeField] protected LLMConfig[] _llmConfigs;

    [SerializeField, Tooltip("Si utilizar o no el chainOfThought, es decir, los steps adicionales")]
    protected bool _useSteps = true;
    [SerializeField, Tooltip("Si utilizar o no el historico en el contexto inicial")]
    protected bool _useHistoricalInContext = true;
    [SerializeField, Tooltip("Si utilizar o no el historico en los step checks")]
    protected bool _useHistoricalInSteps = false;

    protected JsonSchema _contextSchema = null;
    protected JsonSchema _stepsSchema = null;

    protected bool _promptSent = false;
    Action<string> _responseCallback = null;
    protected int _configIndex;
    protected int _stepCounter;
    protected string _prompt;

    /// --- Metodos para el json schema

    /// <summary>
    /// En este metodo se debe crear e inicializar los jsonSchemas con los campos a usar
    /// </summary>
    protected abstract void createJsonSchemas();

    /// <summary>
    /// Metodo para obtener el prompt para los steps con el texto json de la primera respuesta
    /// </summary>
    /// <param name="firstResponse"></param>
    /// <returns></returns>
    protected abstract string deseralizePromptFirstResponse(string serializedResponse);
    /// <summary>
    /// Metodo para obtener el prompt para los steps con el texto json de la primera respuesta
    /// </summary>
    /// <param name="firstResponse"></param>
    /// <returns></returns>
    protected abstract string deseralizePromptStepResponse(string serializedResponse);

    /// --- Metodos con la gestion de prompts

    /// <summary>
    /// Metodo interno encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
    /// </summary>
    /// <param name="responseCallback">Metodo al que llamar con la respuesta del LLM</param>
    /// <param name="promptText">Contenido de texto de la petición</param>
    /// <param name="configIndex">Archivo de configuracion a utilizar</param>
    /// <returns>Devuelve true si pudo mandar el prompt</returns>
    protected virtual bool sendPrompt(Action<string> responseCallback, string promptText, int configIndex = 0)
    {
        if (configIndex >= _llmConfigs.Length)
        {
            Debug.LogError("ConfigIndex se sale del array de LLMConfigs");
            return false;
        }

        if (_promptSent)
        {
            Debug.LogError("Prompt ya en curso");
            return false;
        }

        _responseCallback = responseCallback;
        _configIndex = configIndex;

        _prompt = promptText;

        string configLLM = _llmConfigs[_configIndex].GetContext()
            + _llmConfigs[_configIndex].GetSafeguard();

        if (_useHistoricalInContext)
        {
            configLLM = configLLM + "\n" + _llmConfigs[_configIndex].GetHistoric();
        }

        _llmConfigs[_configIndex].AddHistoric("Prompt: " + _prompt);

        bool sent = LLMSystemAPI.Instance.SendPrompt(recieveFirstResponse, _prompt, configLLM, _contextSchema, _llmConfigs[_configIndex].GetTemperature(), _llmConfigs[_configIndex].GetRagUse(), (int)_llmConfigs[_configIndex].GetRagFileType());

        _promptSent = sent;

        return sent;
    }

    /// <summary>
    /// Metodo encargado de recibir la primera respuesta del LLM, y en caso de haber steps, mandarlos, sino mandar el resultado a respondPrompt
    /// </summary>
    /// <param name="success"></param>
    /// <param name="text"></param>
    protected virtual void recieveFirstResponse(bool success, string text)
    {
        if (_useSteps)
        {
            string deserializedPrompt = deseralizePromptFirstResponse(text);
            _stepCounter = 0;
            sendStepPrompt(deserializedPrompt);
        }
        else
            respondPrompt(success, text);
    }

    /// <summary>
    /// Metodo privado para mandar un prompt de step
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    protected bool sendStepPrompt(string prompt)
    {
        if (_stepCounter >= _llmConfigs[_configIndex].GetStepChecks().Length)
            return false;

        bool sent = LLMSystemAPI.Instance.SendPrompt(recieveStepResponse, prompt, _llmConfigs[_configIndex].GetStepChecks()[_stepCounter], _stepsSchema, _llmConfigs[_configIndex].GetTemperature(), _llmConfigs[_configIndex].GetRagUse(), (int)_llmConfigs[_configIndex].GetRagFileType());
        _stepCounter++;

        return sent;
    }

    /// <summary>
    /// Metodo encargado de recibir las respuestas de los steps y en caso de terminarlos, mandar el resultado a respondPrompt
    /// </summary>
    /// <param name="success"></param>
    /// <param name="text"></param>
    protected virtual void recieveStepResponse(bool success, string text)
    {
        string deseralizedPrompt = deseralizePromptStepResponse(text);
        if (!sendStepPrompt(deseralizedPrompt))
            respondPrompt(success, text);
    }

    /// <summary>
    /// Metodo encargado de finalizar el proceso del prompt y devolver la respuesta
    /// </summary>
    /// <param name="success"></param>
    /// <param name="text"></param>
    protected virtual void respondPrompt(bool success, string text)
    {
        _llmConfigs[_configIndex].AddHistoric("Response: " + text);
        _promptSent = false;
        
        _responseCallback?.Invoke(text);
    }

    virtual protected void Awake()
    {
        createJsonSchemas();

        foreach (LLMConfig config in _llmConfigs)
            config.ClearHistoric();
    }
}
