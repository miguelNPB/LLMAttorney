using System;
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

        string configLLM = _config.getContext() + "El historial de busqueda y la información que has dado hasta ahora ha sido la siguiente, "
            + _config.getSafeguard();

        Debug.Log("PROMPT: " + prompt);
        Debug.Log("CONTEXT: " + configLLM);
        Debug.Log("Schema: " + schema);

        foreach(KeyValuePair<string, PropertyInfo> property in schema.properties){
            Debug.Log(property.Key);
        }
        Debug.Log("Schema required: " + schema.required);
        Debug.Log("Schema to string: " + schema.ToString());

        LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, prompt, configLLM, schema, 
            _config.getTemperature(), _config.getRagUse());

        inputField.text = "";

        _uiSearch.AddMessage(prompt);
        _uiSearch.StartPendingMessage();
    }


}
