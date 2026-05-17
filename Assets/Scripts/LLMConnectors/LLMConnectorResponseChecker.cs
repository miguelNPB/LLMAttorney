using System;
using UnityEngine;

public class LLMConnectorResponseChecker : LLMConnector
{
    [Serializable]
    private class ResponseCheckerResponse
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
        ResponseCheckerResponse jsonResponse = JsonUtility.FromJson<ResponseCheckerResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.answer);
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
        return jsonResponse.answer;
    }

    protected override string deseralizePromptStepResponse(string firstResponse)
    {
        ResponseCheckerResponse jsonResponse = JsonUtility.FromJson<ResponseCheckerResponse>(firstResponse);
        return jsonResponse.answer;
    }
}
