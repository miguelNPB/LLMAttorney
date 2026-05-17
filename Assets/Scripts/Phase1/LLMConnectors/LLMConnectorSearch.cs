using System;
using Telemetry;
using UnityEngine;

/// <summary>
/// LLMConnector usado en la fase 1 para buscar precios en el RAG de precios
/// </summary>
public class LLMConnectorSearch : LLMConnector
{
    [Serializable]
    private class SearchResponse
    {
        public string answer;
    }

    private Action<string> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<string> responseCallback, string prompt)
    {
        _responseCallback = responseCallback;
        sendPrompt(recieveFinalResponse, prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(finalSerializedResponse);
     
        _responseCallback?.Invoke(jsonResponse.answer);
    }

    protected override void recieveStepResponse(bool success, string text)
    {
        if (text.Contains("Sin información"))
        {
            recieveFinalResponse("Información no disponible. Especifique mejor la petición.");
        }
        else
            base.recieveStepResponse(success, text);
    }
    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(serializedResponse);
        return jsonResponse.answer;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(serializedResponse);
        return jsonResponse.answer;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
    }

    /*
    protected override bool sendContextPrompt(int indexConfig = 0)
    {
        

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



    protected override void receiveResponse(bool success, string answer)
    {

        if (success)
        {
            // deserializamos la respuesta
            SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(answer);

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

                if (!jsonResponse.respuestaValida || !jsonResponse.respuestaCoherente)
                {
                    TelemetryDispatch.SendNotConsistentAnswer(_messageID);
                }

                TelemetryDispatch.SendQueryReceived(_messageID);

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
        _messageID = EventManager.Instance.getMessageID();
        TelemetryDispatch.SendQueryPost(_messageID);

        sendContextPrompt(indexConfig);
    }
    */
}
