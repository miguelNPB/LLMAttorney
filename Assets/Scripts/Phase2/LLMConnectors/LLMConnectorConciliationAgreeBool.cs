using System;
using UnityEngine;

public class LLMConnectorConciliationAgreeBool : LLMConnector
{
    [Serializable]
    private class LLMConciliationResponseText
    {
        public bool agree;
    }

    private Action<bool> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<bool> responseCallback, Action<string> errorCallback, string prompt, bool isPlayer)
    {
        _responseCallback = responseCallback;

        sendPrompt(recieveFinalResponse, errorCallback, prompt, isPlayer ? 0 : 1);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        LLMConciliationResponseText jsonResponse = JsonUtility.FromJson<LLMConciliationResponseText>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.agree);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("agree", new PropertyInfo(JsonDataType.Boolean));
    }
}
