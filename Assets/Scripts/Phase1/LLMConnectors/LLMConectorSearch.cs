using System;
using System.Collections;
using System.Collections.Generic;
using Telemetry;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private int _messageID = -1;

    protected override bool sendContextPrompt(int indexConfig = 0)
    {
        _uiSearch.StartPendingMessage();

        bool messageSent = base.sendContextPrompt(indexConfig);

        if (!messageSent)
        {
            _uiSearch.EndPendingMessage("Fallo de conexion, escriba de nuevo la pregunta");
        }

        return messageSent;
    }

    protected override bool sendSecuritySteps(string prompt)
    {
        _uiSearch.StartPendingMessage();

        bool securityStepSent = base.sendSecuritySteps(prompt);

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

    protected override void receiveResponse(bool success, string answer)
    {
        Debug.Log("Respuesta cruda: " + answer);

        if (success)
        {
            // deserializamos la respuesta
            SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(answer);

            if (_stepCounter == 0)
            {
                _messageID = LLMLogManager.Instance.getNumMessageSent();
                LLMLogManager.Instance.addMessageSent();
            }

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
                sendSecuritySteps(jsonResponse.answer);
            }
            else
            {

                string log =
                $"[Fase: {SceneManager.GetActiveScene().buildIndex}] [Envio: {_messageID}] Contestacion buscador: {jsonResponse.answer}.\n\n" +
                $"Respuesta valida: {jsonResponse.respuestaValida}\n" +
                $"Respuesta coherente: {jsonResponse.respuestaCoherente}";

                LLMLogManager.Instance.LogMessageSent(log, _messageID);

                if (!jsonResponse.respuestaValida || !jsonResponse.respuestaCoherente)
                {
                    TelemetryDispatch.SendNotConsistentAnswer(_messageID);
                }

                _historical.Add("Respuesta :" + jsonResponse.answer);
                _stepCounter = 0;
                _promptSent = false;
                _uiSearch.ShowMessage();
                
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
        sendContextPrompt(indexConfig);
    }

    private void Awake()
    {
        createJsonSchemas();
    }

}
