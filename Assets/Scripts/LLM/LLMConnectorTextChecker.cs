using System;
using UnityEngine;

/// <summary>
/// LLMConnector para comprobar si un texto es coherente. No tiene steps
/// </summary>
public class LLMConnectorTextChecker : LLMConnector
{
    [Serializable]
    private class TextCheckerResponse
    {
        public bool isCoherent;
    }
    private Action<bool> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<bool> responseCallback, Action<string> errorCallback, string prompt, int indexConfig)
    {
        Debug.Log("Este es el index del comprobador de puto texto " + indexConfig);
        _responseCallback = responseCallback;
        sendPrompt(recieveFinalResponse, errorCallback, prompt, indexConfig);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        TextCheckerResponse jsonResponse = JsonUtility.FromJson<TextCheckerResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.isCoherent);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("isCoherent", new PropertyInfo(JsonDataType.Boolean));
    }
}
