using System;
using UnityEngine;

public class LLMConnectorQuestionChecker : LLMConnector
{
    [Serializable]
    private class QuestionCheckerResponse
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
        QuestionCheckerResponse jsonResponse = JsonUtility.FromJson<QuestionCheckerResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.isCoherent);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("isCoherent", new PropertyInfo(JsonDataType.Boolean));
    }
}
