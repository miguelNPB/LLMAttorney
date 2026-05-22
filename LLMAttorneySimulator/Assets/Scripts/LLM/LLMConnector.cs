using System;
using Telemetry;
using UnityEngine;

/// <summary>
/// Clase enfocada en manejar los envios de peticiones del simulador al servidor y el LLM, recibiendo también las respuestas de estos y devolviendolas
/// para su uso.
/// </summary>
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

    Action<string> _internalFinalResponseCallback = null;
    Action<string> _errorResponseCallback = null;
    protected int _configIndex;
    protected int _stepCounter;
    protected string _prompt;

    protected int _messageID;

    protected bool _overrideContext = false;
    protected string _overrideContextText = "";
    protected string _historicText = "";

    /// --- Metodos para el json schema

    /// <summary>
    /// Metodo enfocado en la creacion de los distintos esquemas que usara el LLM. Estos esquemas marcan los atributos que deben ser rellenados y 
    /// devueltos por el LLM
    /// </summary>
    protected abstract void createJsonSchemas();

    /// <summary>
    /// Metodo enfocado en deserializar los datos devueltos en la primera respuesta por el LLM para la selección de uno o varios de los 
    /// atributos que pasar al step. Por ejemplo si mi step solo revisa uno de los dos strings devueltos solo paso ese a la siguiente llamada.
    /// Solo hace falta hacer override si se usan los steps.
    /// </summary>
    /// <param name="serializedResponse">Respuesta del LLM que se desea deserializar</param>
    /// <returns></returns>
    protected virtual string deseralizePromptFirstResponse(string serializedResponse) { return serializedResponse; }

    /// <summary>
    /// Metodo enfocado en deserializar los datos devueltos las respuestas de los steps por el LLM para la selección de uno o varios de los 
    /// atributos que pasar al step. Por ejemplo si mi step solo revisa uno de los dos strings devueltos solo paso ese a la siguiente llamada.
    /// Solo hace falta hacer override si se usan los steps.
    /// </summary>
    /// <param name="serializedResponse">Respuesta del LLM que se desea deserializar</param>
    /// <returns></returns>
    protected virtual string deseralizePromptStepResponse(string serializedResponse) { return serializedResponse; }

    /// --- Metodos con la gestion de prompts

    /// <summary>
    /// Metodo interno encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLM
    /// </summary>
    /// <param name="responseCallback">Metodo al que llamar con la respuesta del LLM</param>
    /// <param name="errorCallback">Metodo al que llamar si se devuelve un fallo del servidor en la llamada</param>
    /// <param name="promptText">Contenido de texto de la petición</param>
    /// <param name="configIndex">Archivo de configuracion a utilizar</param>
    /// <returns>Devuelve true si pudo mandar el prompt</returns>
    protected virtual bool sendPrompt(Action<string> responseCallback, Action<string> errorCallback, string promptText, int configIndex = 0)
    {
        if (configIndex >= _llmConfigs.Length)
        {
            Debug.LogError("ConfigIndex se sale del array de LLMConfigs. Index intentado: " + configIndex + " Longitud del array: " + _llmConfigs.Length);
            return false;
        }

        _internalFinalResponseCallback = responseCallback;
        _errorResponseCallback = errorCallback;
        _configIndex = configIndex;

        _prompt = promptText;

        string configLLM = (_overrideContext ? _overrideContextText : _llmConfigs[_configIndex].GetContext())
            + _llmConfigs[_configIndex].GetSafeguard();

        if (_useHistoricalInContext)
        {
            configLLM = configLLM + "\n" + _llmConfigs[_configIndex].GetHistoricHeader() + "\n" + _historicText;
        }

        LLMSystemAPI.Instance.SendPrompt(recieveFirstResponse, _prompt, configLLM, _contextSchema, _llmConfigs[_configIndex].GetTemperature(), _llmConfigs[_configIndex].GetRagUse(), (int)_llmConfigs[_configIndex].GetRagFileType());

        _messageID = EventManager.Instance.getMessageID();
        Telemetry.TelemetryDispatch.SendQueryPost(_messageID);

        return true;
    }

    /// <summary>
    /// Metodo encargado de recibir la primera respuesta del LLM, y en caso de haber steps, mandarlos, sino mandar el resultado a respondPrompt
    /// </summary>
    /// <param name="success"> Indica si la respuesta a sido devuelta del servidor sin ningun error o problema</param>
    /// <param name="text"> Atributos rellenados por el LLM</param>
    protected virtual void recieveFirstResponse(bool success, string text)
    {
        Telemetry.TelemetryDispatch.SendQueryReceived(_messageID);

        if (success)
        {
            if (_useSteps)
            {
                string deserializedPrompt = deseralizePromptFirstResponse(text);
                _stepCounter = 0;
                sendStepPrompt(deserializedPrompt);
            }
            else
            {
                respondPrompt(success, text);
            }                
        }
        else
        {
            _errorResponseCallback?.Invoke(text);
        }
        
    }

    /// <summary>
    /// Metodo privado para mandar un prompt de step
    /// </summary>
    /// <param name="prompt"> Prompt que ha sido enviado</param>
    /// <returns> Devuelve true si aun quedan steps que revisar</returns>
    protected bool sendStepPrompt(string prompt)
    {
        if (_stepCounter >= _llmConfigs[_configIndex].GetStepChecks().Length)
        {
            return false;
        }     

        LLMSystemAPI.Instance.SendPrompt(recieveStepResponse, prompt, _llmConfigs[_configIndex].GetStepChecks()[_stepCounter], _stepsSchema, _llmConfigs[_configIndex].GetTemperature(), _llmConfigs[_configIndex].GetRagUse(), (int)_llmConfigs[_configIndex].GetRagFileType());
        _stepCounter++;

        _messageID = EventManager.Instance.getMessageID();
        Telemetry.TelemetryDispatch.SendQueryPost(_messageID);

        return true;
    }

    /// <summary>
    /// Metodo encargado de recibir las respuestas de los steps y en caso de terminarlos, mandar el resultado a respondPrompt
    /// </summary>
    /// <param name="success">Indica si el servidor a podido devolver los atributos sin ningun problema</param>
    /// <param name="text">Conjunto de atributos rellenados por el LLM</param>
    protected virtual void recieveStepResponse(bool success, string text)
    {
        if (success)
        {
            Telemetry.TelemetryDispatch.SendQueryReceived(_messageID);

            string deseralizedPrompt = deseralizePromptStepResponse(text);
            if (!sendStepPrompt(deseralizedPrompt))
                respondPrompt(success, text);
        }
        else
        {
            _errorResponseCallback?.Invoke(text);
        }
        
    }

    /// <summary>
    /// Metodo encargado de finalizar el proceso del prompt y devolver la respuesta
    /// </summary>
    /// <param name="success">Indica si el servidor a podido devolver los atributos sin ningun problema</param>
    /// <param name="text">Conjunto de atributos rellenados por el LLM</param>
    protected virtual void respondPrompt(bool success, string text)
    {
        if (success)
        {
            if (_useHistoricalInContext || _useHistoricalInSteps)
                _historicText += ("Response: " + text);

            _internalFinalResponseCallback?.Invoke(text);
        }
        else
        {
            _errorResponseCallback?.Invoke(text);
        }
    }

    /// <summary>
    /// Metodo que permite alterar el contexto que se le pasa al LLM sin afectar el LLMConfig original. Se usa por ejemplo si se quiere meter 
    /// contexto extra generado durante el proceso a las llamadas de los steps.
    /// </summary>
    /// <param name="newContext">Nuevo contexto que se debe incluir en la llamada al LLM</param>
    protected void overrideLLMContext(string newContext)
    {
        _overrideContext = true;
        _overrideContextText = newContext;
    }

    /// <summary>
    /// Metodo para insertar texto al historico
    /// </summary>
    /// <param name="text">Texto que se desea insertar</param>
    protected void appendHistoricText(string text)
    {
        _historicText += "\n" + text;
    }

    /// <summary>
    /// Metodo para limpiar el historico
    /// </summary>
    protected void clearHistoricText()
    {
        _historicText = "";
    }

    virtual protected void Awake()
    {
        createJsonSchemas();
    }
}
