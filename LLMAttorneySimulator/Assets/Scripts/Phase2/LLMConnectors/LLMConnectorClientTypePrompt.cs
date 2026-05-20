using System;
using UnityEngine;

/// <summary>
/// Enum que marca los distintos tipos de peticiones que puede hacer el usuario al cliente en la pestaña de chat
/// </summary>
public enum ClientPromptType { Question = 0, Conversation = 1, Perito = 2, Report = 3, Witness = 4, ReceiptFacture = 5}

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
        public int documentQueryType;
    }

    private Action<int> _responseCallback;

    /// <summary>
    /// Metodo publico para iniciar la llamada al LLM con los parametros y configuracion especificados
    /// </summary>
    /// <param name="prompt">Prompt escrito por el usuario que se desea enviar al LLM</param>
    /// <param name="onRecievePrompt">Metodo que debe llamarse una vez terminado el envio y recibida la contestación del LLM</param>
    /// <param name="errorCallback">Metodo que debe llamarse si el envio del prompt al LLM es fallido debido a un problema del servidor</param>
    public void SendPrompt(string prompt, Action<int> onRecievePrompt, Action<string> errorCallback)
    {
        _responseCallback = onRecievePrompt;
        sendPrompt(recieveFinalResponse, errorCallback, prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse">Texto en formato json devuelto por el servidor que cuenta con los atributos rellenados por el LLM</param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        ClientPromptTypeRequest jsonResponse = JsonUtility.FromJson<ClientPromptTypeRequest>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.documentQueryType);
    }

    /// <summary>
    /// Creacion de los esquemas especificos para este conector
    /// </summary>
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("documentQueryType", new PropertyInfo(JsonDataType.Integer));
    }
}
