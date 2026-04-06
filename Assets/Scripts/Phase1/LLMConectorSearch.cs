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

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
            {
                SendSecuritySteps(jsonResponse.answer);
            }
            else
            {
                Debug.Log("Respuesta final");
                _historical.Add("Respuesta :" + jsonResponse.answer);
                _stepCounter = 0;
                _uiSearch.ShowMessage();
                _promptSent = false;
            }
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _uiSearch.EndPendingMessage("Error al contactar con el modelo.");
        }

    }

    public void CallSendContext(int indexConfig = 0)
    {
        SendContextMessage(indexConfig);
    }

    /**
     * Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
     */
    protected override bool SendContextMessage(int indexConfig = 0)
    {
        _uiSearch.StartPendingMessage();

        bool messageSent = base.SendContextMessage(indexConfig);

        if (!messageSent)
        {
            _uiSearch.EndPendingMessage("Fallo de conexion, escriba de nuevo la pregunta");
        }

        return messageSent;
    }

    protected override bool SendSecuritySteps(string prompt)
    {
        _uiSearch.StartPendingMessage();

        bool securityStepSent = base.SendSecuritySteps(prompt);

        if (!securityStepSent)
        {
            _uiSearch.EndPendingMessage("Fallo de conexion, escriba de nuevo la pregunta");
        }

        return securityStepSent;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    private void Awake()
    {
        createJsonSchemas();
    }

}
