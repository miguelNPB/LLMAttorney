using System;
using UnityEngine;

public class LLMConnectorResponseChecker : LLMConnector
{
    [Serializable]
    private class ResponseCheckerResponse
    {
        public bool isCoherent;
    }
    private Action<bool> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<bool> responseCallback, string prompt)
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
        ResponseCheckerResponse jsonResponse = JsonUtility.FromJson<ResponseCheckerResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.isCoherent);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
    }

    protected override string deseralizePromptFirstResponse(string firstResponse)
    {
        ResponseCheckerResponse jsonResponse = JsonUtility.FromJson<ResponseCheckerResponse>(firstResponse);
        return jsonResponse.isCoherent.ToString();
    }

    protected override string deseralizePromptStepResponse(string firstResponse)
    {
        ResponseCheckerResponse jsonResponse = JsonUtility.FromJson<ResponseCheckerResponse>(firstResponse);
        return jsonResponse.isCoherent.ToString();
    }

    // -- Metodos overrideados para el funcionamiento por bool -- 

    protected override void recieveFirstResponse(bool success, string text)
    {
        if (_useSteps)
        {
            bool isCoherent = bool.Parse(deseralizePromptFirstResponse(text));
            _stepCounter = 0;

            if (isCoherent)
                sendStepPrompt(text);
            else
                respondPrompt(success, text);
        }
        else
            respondPrompt(success, text);
    }

    protected override void recieveStepResponse(bool success, string text)
    {
        bool isCoherent = bool.Parse(deseralizePromptStepResponse(text));
        if (isCoherent || !sendStepPrompt(_prompt))
            respondPrompt(success, text);
    }

    // 
}
