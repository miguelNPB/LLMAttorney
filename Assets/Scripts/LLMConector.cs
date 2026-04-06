using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public abstract class LLMConector : MonoBehaviour
{

    [SerializeField]
    protected ConfigLLMInfo[] _config;

    [Header("Search Messages")]
    public TMP_InputField inputField;

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

    /**
     * Metodo encargado de recoger la respuesta del LLM y transmitirla a la clase que muestre el output de este
     * @param success: muestra si ha podido obtenerse una respuesta del LLM
     * @param answer: texto plano que ha sacado el LLM como output
     */
    public abstract void RecieveChatMessage(bool success, string answer);

    /**
     * Metodo que crea los esquemas y propiedades que debe devolver las llamadas al LLM
     * 
     */
    protected abstract void createJsonSchemas();

    /**
     * Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
     */
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

            string prompt = inputField.text;

            string configLLM = _config[_indexConfig].getContext()
                + _config[_indexConfig].getSafeguard();

            configLLM = configLLM + "\n " + _config[_indexConfig].getHistoricalConversation() + "\n Historico: \n";

            if (_useHistoricalInContext)
            {
                foreach (String s in _historical)
                {
                    configLLM = configLLM + s + "\n";
                }
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

    /**
     * Metodo encargado de revisar el texto obtenido como respuesta segun un array de directivas descritas, cambiandolo segun lo especificado
     * @param prompt: texto que debe ser revisado
     */
    protected virtual bool SendSecuritySteps(string prompt)
    {
        
        Debug.Log("PROMPT de security checks: " + prompt);

        string configLLM = "Teniendo el siguiente texto: \n" + prompt + "\n Y teniedo la siguiente directiva de seguridad" +
            _config[_indexConfig].getSafeguardSteps() +
            "\n Quiero que hagas lo siguiente: " + _config[_indexConfig].getStepsChecks()[_stepCounter];

        if (_useHistoricalInSteps)
        {
            foreach (String s in _historical)
            {
                configLLM = configLLM + s + "\n";
            }
        }

        StartCoroutine(CoroutineSendPromptSteps(prompt, configLLM, _stepsSchema));

        inputField.text = "";

        _stepCounter++;

        return true;
    }

    private IEnumerator CoroutineSendPromptSteps(string prompt, string configLLM, JsonSchema schema)
    {

        float timer = 0;

        while (!LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, prompt, configLLM, schema,
            _config[_indexConfig].getTemperature(), false))
        {
            timer += Time.deltaTime;

            yield return null;
        }

    }

    private IEnumerator CoroutineSendPrompt(string prompt, string configLLM, JsonSchema schema)
    {

        float timer = 0;

        while (!LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, prompt, configLLM, schema,
            _config[_indexConfig].getTemperature(), _config[_indexConfig].getRagUse(), (int)_config[_indexConfig].getRagFileType()))
        {
            timer += Time.deltaTime;

            yield return null;
        }

    }
}
