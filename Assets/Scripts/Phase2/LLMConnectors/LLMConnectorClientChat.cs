using System;
using System.Collections.Generic;
using UnityEngine;

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
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<string> onRecievePrompt, Action<string> errorCallback, string prompt, int context)
    {
        // setup context y historic
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
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        ClientChatResponse jsonResponse = JsonUtility.FromJson<ClientChatResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.answer);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
    }
}
