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
        public bool respuestaValida;
        public bool respuestaCoherente;
    }

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
        _contextSchema.properties.Add("respuestaValida", new PropertyInfo(JsonDataType.Boolean));
        _contextSchema.properties.Add("respuestaCoherente", new PropertyInfo(JsonDataType.Boolean));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("respuestaValida", new PropertyInfo(JsonDataType.Boolean));
        _stepsSchema.properties.Add("respuestaCoherente", new PropertyInfo(JsonDataType.Boolean));

        _schemasCreated = true;
    }

    public override void recieveChatMessage(bool success, string answer)
    {
        Debug.Log("Respuesta cruda: " + answer);

        if (success)
        {
            // deserializamos la respuesta
            SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(answer);

            Debug.Log("Respuesta valida: " + jsonResponse.respuestaValida);

            Debug.Log("Respuesta coherente: " + jsonResponse.respuestaCoherente);

            if (jsonResponse.respuestaValida && jsonResponse.respuestaCoherente)
            {
                _uiSearch.EndPendingMessage(jsonResponse.answer);
            }
            else if (!jsonResponse.respuestaCoherente)
            {
                _uiSearch.EndPendingMessage("Información no disponible. Por favor centrese en cuestiones del ambito del derecho civil");
            }
            else
            {
                _uiSearch.EndPendingMessage("Error de formato. Por favor repita la pregunta");
            }

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length &&
                (!jsonResponse.respuestaValida || !jsonResponse.respuestaCoherente))
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

    private void Awake()
    {
        createJsonSchemas();
    }

}
