using System;
using UnityEngine;

/// <summary>
/// LLMConnector enfocado en la generación de mensajes de respuesta a preguntas o afirmaciones dadas por el usuario en la pestaña de chat.
/// </summary>
public class LLMConnectorClientChat : LLMConnector
{
    /// <summary>
    /// Formato para el LLM de la respuesta del cliente
    /// </summary>
    [Serializable]
    private class ClientChatResponse
    {
        public string answer;
    }

    private Action<string> _responseCallback;

    /// <summary>
    /// Metodo publico para iniciar la llamada al LLM con los parametros y configuracion especificados
    /// </summary>
    /// <param name="onRecievePrompt">Metodo que debe llamarse una vez terminado el envio y recibida la contestación del LLM</param>
    /// <param name="errorCallback">Metodo que debe llamarse si el envio del prompt al LLM es fallido debido a un problema del servidor</param>
    /// <param name="prompt">Prompt escrito por el usuario que se desea enviar al LLM</param>
    /// <param name="context">Configuración concreta que se debe usar para este envio</param>
    public void SendPrompt(Action<string> onRecievePrompt, Action<string> errorCallback, string prompt, int context)
    {
        overrideLLMContext("Te llamas " + GameSystem.Instance.CaseData.clientName + ". " + _llmConfigs[context].GetContext() + "\nResumen del caso: " + GameSystem.Instance.CaseData.caseDescription);
        clearHistoricText();
        string conversation = "";
        foreach (ConversationMessage m in GameSystem.Instance.CaseData.clientMessages)
        {
            conversation += (m.fromPlayer ? "Abogado:" : "Tu:") + m.text;
        }
        appendHistoricText(conversation);

        // mandar prompt
        _responseCallback = onRecievePrompt;
        sendPrompt(recieveFinalResponse, errorCallback, prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse">Texto en formato json devuelto por el servidor que cuenta con los atributos rellenados por el LLM</param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        ClientChatResponse jsonResponse = JsonUtility.FromJson<ClientChatResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.answer);
    }

    /// <summary>
    /// Creacion de los esquemas especificos para este conector
    /// </summary>
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
    }
}
