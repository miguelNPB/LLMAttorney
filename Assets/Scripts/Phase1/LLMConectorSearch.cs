using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class LLMConectorSearch : LLMConector
{

    [SerializeField]
    private UISearchManager _uiSearch;

    [Serializable]
    private class SearchResponse
    {
        public string answer;
        //public string[] documents;
    }

    [Header("Search Messages")]
    public TMP_InputField inputField;

    /**
     * Metodo encargado de recoger la respuesta del LLM y transmitirla a la clase que muestre el output de este
     * @param success: muestra si ha podido obtenerse una respuesta del LLM
     * @param answer: texto plano que ha sacado el LLM como output
     */
    public override void RecieveChatMessage(bool success, string answer)
    {
        Debug.Log("Respuesta cruda: " + answer);

        if (success)
        {
            // deserializamos la respuesta
            SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(answer);

            _uiSearch.EndPendingMessage(jsonResponse.answer);

            if (_stepCounter < _config.getStepsChecks().Length)
            {
                Debug.Log("Revisa el texto: " + jsonResponse.answer);
                securityStepsCheck(jsonResponse.answer);
            }
            else
            {
                Debug.Log("Respuesta final");
                _uiSearch.ShowMessage();
            }
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _uiSearch.EndPendingMessage("Error al contactar con el modelo.");
        }

    }

    /**
     * Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
     */
    public override void SendChatMessage()
    {
        JsonSchema schema = new JsonSchema();

        schema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        string prompt = inputField.text;

        //string conversation = "";
        //foreach (ConversationMessage m in GameSystem.Instance.CaseData.clientMessages)
        //{
        //    conversation += (m.fromPlayer ? "Abogado:" : "Tu:") + m.text;
        //}

        string configLLM = _config.getContext()
            + _config.getSafeguard();

        Debug.Log("PROMPT: " + prompt);
        Debug.Log("CONTEXT: " + configLLM);
        

        LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, prompt, configLLM, schema, 
            _config.getTemperature(), _config.getRagUse());

        inputField.text = "";

        //_uiSearch.AddMessage(prompt);
        _uiSearch.StartPendingMessage();
    }

    public override void securityStepsCheck(string prompt)
    {
        JsonSchema schema = new JsonSchema();

        schema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        Debug.Log("PROMPT de security checks: " + prompt);

        string configLLM = "Teniendo el siguiente texto: \n" + prompt + "\n Quiero que lo alteres y lo corrijas usando de base la siguiente directiva: " + 
            _config.getStepsChecks()[_stepCounter];

        StartCoroutine(CoroutineSendPrompt(prompt, configLLM, schema)); 

        inputField.text = "";

        //_uiSearch.AddMessage(prompt);
        _uiSearch.StartPendingMessage();

        _stepCounter++;
    }

    private IEnumerator CoroutineSendPrompt(string prompt, string configLLM, JsonSchema schema)
    {

        float timer = 0;

        while (!LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, prompt, configLLM, schema,
            _config.getTemperature(), _config.getRagUse()))
        {
            timer += Time.deltaTime;

            yield return null;
        }



    }


}
