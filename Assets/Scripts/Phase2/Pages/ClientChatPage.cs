using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum ClientPromptType { Question, Conversation, Perito, Report, Witness, DocAlt }

/// <summary>
/// Pagina para gestionar el sistema de mensajes con el cliente
/// </summary>
public class ClientChatPage : ChatPage
{
    /// <summary>
    /// Formato para el LLM para pedir un promptType
    /// </summary>
    [Serializable]
    private class ClientPromptTypeRequest
    {
        public ClientPromptType documentQueryType;
    }


    [Header("ClientMessages")]
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private LLMConnectorDocuments _llmConnectorDocs;
    [SerializeField] private LLMConnectorClientChat _llmConnectorClientChat;

    private ClientPromptType _lastTypePromptRequest;
    private bool _isOpen = false;


    /// <summary>
    /// 
    /// Callback para saber de que tipo de documento se trata la primera respuesta del LLM
    /// </summary>
    /// <param name="success"></param>
    /// <param name="answer"></param>
    private void getPromptTypeFromPrompt(bool success, string answer)
    {
        if (success)
        {
            ClientPromptTypeRequest typeRequest = JsonUtility.FromJson<ClientPromptTypeRequest>(answer);
            _lastTypePromptRequest = typeRequest.documentQueryType;
        }
        else
            _lastTypePromptRequest = ClientPromptType.Question;
    }
    
    /// <summary>
    /// 
    /// Llamada asincrona a el modelo para clasificar el prompt en una de 6 categorias, dependiendo de la que sea generando un documento o haciendo una pregunta al LLM
    /// </summary>
    /// <returns></returns>
    public IEnumerator sendGetPromptTypePrompt(string prompt)
    {
        JsonSchema schema = new JsonSchema();
        schema.properties.Add("documentQueryType", new PropertyInfo(JsonDataType.Integer));

        string configLLM = @"Clasifica el siguiente texto en una única categoría llamada documentQueryType y responde solo con un número:

                0 = Pregunta (texto cuyo objetivo principal es solicitar informaci�n)
                1 = Diálogo (intercambio conversacional entre dos o más interlocutores)
                2 = Informe pericial (documento técnico elaborado por un experto con conclusiones profesionales)
                3 = Informe (documento descriptivo o informativo sin carácter pericial)
                4 = Declaraci�n de testigo (relato de hechos en primera persona o atribuido a un testigo)
                5 = Peticion de recibo (Factura, ticket)

                Reglas:
                Responde solo con un JSON válido
                No añadas texto fuera del JSON
                No añadas explicación
                Elige la categoría predominante


                Devuelve dicho valor en la variable documentQueryType";

        yield return LLMSystemAPI.Instance.SendPromptCoroutine(API_TYPE.LLAMA, getPromptTypeFromPrompt, prompt, configLLM, schema);

        Debug.Log("Ya se el tipo de documento que es: " + _lastTypePromptRequest);

        switch (_lastTypePromptRequest) {
            case ClientPromptType.Question    : _llmConnectorClientChat.CallSendContext(); break;
            case ClientPromptType.Conversation     : _llmConnectorClientChat.CallSendContext(); break;
            case ClientPromptType.Perito      : sendGenerateDocumentPrompt(); break;
            case ClientPromptType.Report     : sendGenerateDocumentPrompt(); break;
            case ClientPromptType.Witness     : sendGenerateDocumentPrompt(); break;
            case ClientPromptType.DocAlt      : sendGenerateDocumentPrompt(); break;
        }
    }

    /// <summary>
    /// Llamado al pulsar el boton de mandar en la UI
    /// </summary>
    public void OnPressSendButton()
    {
        string prompt = _inputField.text;
        addMessage(prompt, true);
        //inputField.text = "";
        StartPendingMessage(false);

        StartCoroutine(sendGetPromptTypePrompt(prompt));
    }

    /// <summary>
    /// Genera un documento en la categoria dada por el resultado de checkprompt
    /// </summary>
    private void sendGenerateDocumentPrompt()
    {
        _llmConnectorDocs.CallSendContext(_lastTypePromptRequest, (int)_lastTypePromptRequest - 2);
        _inputField.text = "";
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

    public bool IsOpen() { return _isOpen; }

    public void Start()
    {
        // inicializamos los mensajes, se actualiza el mesh y lo cerramos
        Open();
        placeMessages(GameSystem.Instance.CaseData.clientMessages);
        ScrollToLastMessage();
        Close();
    }
}
