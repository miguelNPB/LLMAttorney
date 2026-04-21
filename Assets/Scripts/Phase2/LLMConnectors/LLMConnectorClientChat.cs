using System;
using UnityEngine;

public class LLMConnectorClientChat : LLMConector
{
    /// <summary>
    /// Formato para el LLM de la respuesta del cliente
    /// </summary>
    [Serializable]
    private class ClientChatResponse
    {
        public string answer;
    }

    [SerializeField] private ClientChatPage _clientChatPage;
    [SerializeField] private ComputerSystem _computerSystem;
    public void CallSendContext()
    {
        string conversation = "";
        foreach (ConversationMessage m in GameSystem.Instance.CaseData.clientMessages)
        {
            conversation += (m.fromPlayer ? "Abogado:" : "Tu:") + m.text;
        }


        _config[0].historicalConversation = conversation;
        sendContextPrompt(0);
    }
    protected override void receiveResponse(bool success, string answer)
    {
        if (success)
        {
            // deserializamos la respuesta
            ClientChatResponse jsonResponse = JsonUtility.FromJson<ClientChatResponse>(answer);
            string response = jsonResponse.answer;
            Debug.Log("Respuesta cruda: " + response);

            _clientChatPage.EndPendingMessage(response);

            ConversationMessage conversationMessage;
            conversationMessage.fromPlayer = false;
            conversationMessage.text = response;
            GameSystem.Instance.CaseData.clientMessages.Add(conversationMessage);

            if (!_clientChatPage.IsOpen())
                _computerSystem.ToggleNotification(Page.ClientChat, true);


            _promptSent = false;
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _clientChatPage.EndPendingMessage("Error al contactar con el modelo.");
        }
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    private void Start()
    {
        _config[0].context = "Te llamas " + GameSystem.Instance.CaseData.clientName+  ". " + _config[0].context + "\nResumen del caso: " + GameSystem.Instance.CaseData.caseDescription;
    }

    private void Awake()
    {
        createJsonSchemas();
    }
}
