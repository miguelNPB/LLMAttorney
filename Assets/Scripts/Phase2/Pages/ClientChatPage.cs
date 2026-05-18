using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// Pagina para gestionar el sistema de mensajes con el cliente
/// </summary>
public class ClientChatPage : ChatPage
{
    [Header("ClientMessages")]
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private DocumentGenerationManager _documentGenerationManager;
    [SerializeField] private LLMConnectorClientChat _llmConnectorClientChat;
    [SerializeField] private LLMConnectorClientTypePrompt _llmConnectorClientTypePrompt;

    private string _prompt;
    private bool _isOpen = false;

    /// <summary>
    /// Llamado al recibir la respuesta del LLM de cual es el tipo de prompt
    /// </summary>
    /// <param name="promptType"></param>
    private void recieveClientPromptType(ClientPromptType promptType)
    {
        switch (promptType)
        {
            case ClientPromptType.Question: _llmConnectorClientChat.SendPrompt(recieveClientChatResponse, recieveError, _prompt, 0); break;
            case ClientPromptType.Conversation: _llmConnectorClientChat.SendPrompt(recieveClientChatResponse, recieveError, _prompt, 1); break;
            case ClientPromptType.Perito: _documentGenerationManager.PromptGenerateDocument(recieveClientDocumentResponse, recieveError, DocumentType.Perito, true); break;
            case ClientPromptType.Report: _documentGenerationManager.PromptGenerateDocument(recieveClientDocumentResponse, recieveError, DocumentType.Report, true); break;
            case ClientPromptType.Witness: _documentGenerationManager.PromptGenerateDocument(recieveClientDocumentResponse, recieveError, DocumentType.Witness, true); break;
            case ClientPromptType.ReceiptFacture: _documentGenerationManager.PromptGenerateDocument(recieveClientDocumentResponse, recieveError, DocumentType.ReceiptFacture, true); break;
        }
    }

    /// <summary>
    /// Llamado al recibir la respuesta de texto del LLM
    /// </summary>
    /// <param name="response"></param>
    private void recieveClientChatResponse(string response)
    {
        EndPendingMessage(response);

        ConversationMessage conversationMessage;
        conversationMessage.fromPlayer = false;
        conversationMessage.text = response;
        GameSystem.Instance.CaseData.clientMessages.Add(conversationMessage);

        if (!_isOpen)
        {
            _computerSystem.PingOverlayNotification("¡Has recibido un mensaje del cliente!");
            _computerSystem.ToggleNotification(Page.ClientChat, true);
        }
    }

    private void recieveClientDocumentResponse(string docTitle, string docContent, DocumentType documentType, int cost, bool isPlayer, bool isValid)
    {
        string response = "";
        if (isValid)
        {
            GameSystem.Instance.CaseData.documentManager.CreateDocument(docTitle, documentType, docContent, isValid, cost, isPlayer, false);
            switch (documentType)
            {
                case DocumentType.Perito:
                    response = "Te adjunto el informe pericial: ";
                    break;
                case DocumentType.Report:
                    response = "Te adjunto el informe: ";
                    break;
                case DocumentType.Witness:
                    response = "Te adjunto un documento con un testimonio: ";
                    break;
                case DocumentType.ReceiptFacture:
                    response = "Te adjunto el recibo: ";
                    break;
            }

            response += docTitle;
        }
        else
        {
            response = "Perdona, no he entendido el documento que necesitas. Explicamelo mejor o dime otro que pueda conseguir.";
        }

        EndPendingMessage(response);

        ConversationMessage conversationMessage;
        conversationMessage.fromPlayer = false;
        conversationMessage.text = response;
        GameSystem.Instance.CaseData.clientMessages.Add(conversationMessage);

        if (!_isOpen)
        {
            _computerSystem.PingOverlayNotification("¡Has recibido un mensaje del cliente!");
            _computerSystem.ToggleNotification(Page.ClientChat, true);
        }
    }

    /// <summary>
    /// Llamado al recibir un error al contactar con el modelo
    /// </summary>
    /// <param name="text"></param>
    private void recieveError(string text)
    {
        recieveClientChatResponse("Error con el servidor: " + text);
    }

    /// <summary>
    /// Llamado al pulsar el boton de mandar en la UI
    /// </summary>
    public void OnPressSendButton()
    {
        _prompt = _inputField.text;
        if (_prompt.Length > 1)
        {
            addMessage(_prompt, true);
            StartPendingMessage(false);

            _llmConnectorClientTypePrompt.SendPrompt(_prompt, recieveClientPromptType, recieveError);
        }
    }

    public override void Open()
    {
        _computerSystem.ToggleNotification(Page.ClientChat, false);

        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(true);

        ScrollToLastMessage();

        _isOpen = true;
    }

    public override void Close()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(false);

        _isOpen = false;
    }

    public void Start()
    {
        // inicializamos los mensajes, se actualiza el mesh y lo cerramos
        Open();
        placeMessages(GameSystem.Instance.CaseData.clientMessages);
        ScrollToLastMessage();
        Close();
    }
}
