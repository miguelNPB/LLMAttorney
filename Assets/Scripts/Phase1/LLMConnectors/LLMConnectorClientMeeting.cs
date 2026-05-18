using System;
using System.Collections;
using System.Collections.Generic;
using Telemetry;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LLMConnectorClientMeeting : LLMConnector
{

    [Serializable]
    private class ClientMeetingCheckerResponse
    {
        public string answer;
        public bool validResponse;
    }
    private Action<string, bool> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<string, bool> responseCallback, Action<string> errorCallback, string prompt)
    {
        _responseCallback = responseCallback;
        sendPrompt(recieveFinalResponse, errorCallback, prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        ClientMeetingCheckerResponse jsonResponse = JsonUtility.FromJson<ClientMeetingCheckerResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.answer, jsonResponse.validResponse);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("validResponse", new PropertyInfo(JsonDataType.Boolean));
        
        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("validResponse", new PropertyInfo(JsonDataType.Boolean));
    }

    protected override void recieveFirstResponse(bool success, string text)
    {

        if (_useSteps && _llmConfigs[_configIndex].GetStepChecks().Length > 0)
        {
            ClientMeetingCheckerResponse jsonResponse = JsonUtility.FromJson<ClientMeetingCheckerResponse>(text);
            _stepCounter = 0;

            if (!jsonResponse.validResponse)
            {
                sendStepPrompt(_prompt);
            }        
            else
            {
                respondPrompt(success, text);
            }      
        }
        else
        {
            respondPrompt(success, text);
        }
            
    }

    protected override void recieveStepResponse(bool success, string text)
    {

        ClientMeetingCheckerResponse jsonResponse = JsonUtility.FromJson<ClientMeetingCheckerResponse>(text);

        if (jsonResponse.validResponse || !sendStepPrompt(_prompt))
        {
            respondPrompt(success, text);
        }
            
    }
}
