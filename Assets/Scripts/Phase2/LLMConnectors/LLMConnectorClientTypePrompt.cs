using System;
using UnityEngine;

public enum ClientPromptType { Question, Conversation, Perito, Report, Witness, ReceiptFacture }

/// <summary>
/// LLMConnector para saber que tipo de peticion se le ha hecho al cliente
/// </summary>
public class LLMConnectorClientTypePrompt : LLMConnector
{
    /// <summary>
    /// Formato para el LLM para pedir un promptType
    /// </summary>
    [Serializable]
    private class ClientPromptTypeRequest
    {
        public ClientPromptType documentQueryType;
    }

    private Action<ClientPromptType> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(string prompt, Action<ClientPromptType> onRecievePrompt, Action<string> errorCallback)
    {
        _responseCallback = onRecievePrompt;
        sendPrompt(recieveFinalResponse, errorCallback, prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        ClientPromptTypeRequest jsonResponse = JsonUtility.FromJson<ClientPromptTypeRequest>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.documentQueryType);
    }

    protected override void createJsonSchemas()
    {
        JsonSchema schema = new JsonSchema();
        schema.properties.Add("documentQueryType", new PropertyInfo(JsonDataType.Integer));
    }
}
