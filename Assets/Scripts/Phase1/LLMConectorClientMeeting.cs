using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LLMConectorClientMeeting : LLMConector
{
    [SerializeField]
    private UIClientMeetingManager _uiMeeting;

    [Serializable]
    private class MeetingResponse
    {
        public string answer;
        public bool contratar_abogado;
    }

    [Header("Search Messages")]
    public TMP_InputField inputField;

    private List<String> _historical = new List<string>();

    private bool _abogadoContratado = false;

    private bool _promptSent = false;

    /**
     * Metodo encargado de recoger la respuesta del LLM y transmitirla a la clase que muestre el output de este
     * @param success: muestra si ha podido obtenerse una respuesta del LLM
     * @param answer: texto plano que ha sacado el LLM como output
     */
    public override void RecieveChatMessage(bool success, string answer)
    {
        
        if (success)
        {
            // deserializamos la respuesta
            MeetingResponse jsonResponse = JsonUtility.FromJson<MeetingResponse>(answer);

            Debug.Log("Respuesta cruda: " + jsonResponse.answer);

            if (_stepCounter == 0)
            {
                Debug.Log("Respuesta contratar abogado: " + jsonResponse.contratar_abogado);
                _abogadoContratado = jsonResponse.contratar_abogado;
            }

            _uiMeeting.EndPendingMessage(jsonResponse.answer);          

            if (_stepCounter < _config.getStepsChecks().Length)
            {
                securityStepsCheck(jsonResponse.answer);
            }
            else
            {
                Debug.Log("Respuesta final");
                _historical.Add("Respuesta :" + jsonResponse.answer);
                _stepCounter = 0;
                _uiMeeting.ShowMessage(_abogadoContratado);
                _promptSent = false;
            }
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _uiMeeting.EndPendingMessage("Error al contactar con el modelo.");
        }

    }

    /**
     * Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
     */
    public override void SendChatMessage()
    {

        if (!_promptSent)
        {
            JsonSchema schema = new JsonSchema();

            schema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
            schema.properties.Add("contratar_abogado", new PropertyInfo(JsonDataType.Boolean));

            string prompt = inputField.text;

            string configLLM = _config.getContext()
                + _config.getSafeguard();

            configLLM = configLLM + "\n " + _config.getHistoricalConversation() + "\n Historico: \n";

            foreach (String s in _historical)
            {
                configLLM = configLLM + s + "\n";
            }

            Debug.Log("PROMPT: " + prompt);
            Debug.Log("CONTEXT: " + configLLM);

            _historical.Add("Pregunta: " + prompt);

            _promptSent = true;

            StartCoroutine(CoroutineSendPrompt(prompt, configLLM, schema));

            inputField.text = "";

            _uiMeeting.StartPendingMessage();
        }
        
    }

    public override void securityStepsCheck(string prompt)
    {
        JsonSchema schema = new JsonSchema();

        schema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        Debug.Log("PROMPT de security checks: " + prompt);

        string configLLM = "Teniendo el siguiente texto: \n" + prompt + "\n Y teniedo la siguiente directiva de seguridad" + _config.getSafeguardSteps() +
            "\n Quiero que hagas lo siguiente: " + _config.getStepsChecks()[_stepCounter];

        StartCoroutine(CoroutineSendPromptSteps(prompt, configLLM, schema));

        inputField.text = "";

        //_uiSearch.AddMessage(prompt);
        _uiMeeting.StartPendingMessage();

        _stepCounter++;
    }

    private IEnumerator CoroutineSendPromptSteps(string prompt, string configLLM, JsonSchema schema)
    {

        float timer = 0;

        while (!LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, prompt, configLLM, schema,
            _config.getTemperature(), false))
        {
            timer += Time.deltaTime;

            yield return null;
        }

    }

    private IEnumerator CoroutineSendPrompt(string prompt, string configLLM, JsonSchema schema)
    {

        float timer = 0;

        while (!LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, prompt, configLLM, schema,
            _config.getTemperature(), _config.getRagUse(), (int)_config.getRagFileType()))
        {
            timer += Time.deltaTime;

            yield return null;
        }

    }
}
