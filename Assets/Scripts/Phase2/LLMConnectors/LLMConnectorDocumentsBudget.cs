using UnityEngine;
using UnityEngine.InputSystem;

public class LLMConnectorDocumentsBudget : LLMConector
{
    private class DocumentBudgetResponse
    {
        public int CosteDocumento;
    }

    [SerializeField]
    private ChatPage _msgUIComponent;

    private bool _firstTime = true;

    private ClientPromptType _type;
    private string _docName;
    private string _docContent;

    protected override void receiveResponse(bool success, string answer)
    {
        #if DEBUG
            Debug.Log(answer);
        #endif

        Debug.Log("Step: " + _stepCounter);

        if (success)
        {
            // deserializamos la respuesta
            DocumentBudgetResponse jsonResponse = JsonUtility.FromJson<DocumentBudgetResponse>(answer);

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
            {
                sendSecuritySteps(answer);
            }
            else
            {
                Debug.Log("Respuesta final");

                _stepCounter = 0;
                _promptSent = false;


                _historical.Add("Respuesta :" + answer);

                DocumentType docType = fromClientDocumentToDocumentType(_type);

                GameSystem.Instance.CaseData.documentManager.CreateDocument(_docName, docType, _docContent, true, jsonResponse.CosteDocumento);
                _msgUIComponent._computerSystem.ToggleNotification(Page.ClientChat, true);
                _msgUIComponent.EndPendingMessage("Tu cliente te ha mandado " + _docName + ".txt");

            }
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _msgUIComponent.EndPendingMessage("Error al contactar con el modelo.");
        }
    }

    public void CallSendContext(ClientPromptType type, string docName, string docContent, int indexConfig = 0)
    {
        _docName = docName;
        _docContent = docContent;
        _type = type;

        sendContextPrompt(indexConfig);
    }

    /**
     * Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
     */
    protected override bool sendContextPrompt(int indexConfig = 0)
    {
        //_msgUIComponent.StartPendingMessage(false);
        if (_firstTime)
        {
            _historical.Add(GameSystem.Instance.CaseData.caseDescription);
            _firstTime = false;
        }
        bool messageSent = base.sendContextPrompt(indexConfig);

        if (!messageSent)
        {
            _msgUIComponent.EndPendingMessage("Fallo de conexion, escriba de nuevo la pregunta");
        }

        return messageSent;
    }

    protected override bool sendSecuritySteps(string prompt)
    {
        //_msgUIComponent.StartPendingMessage(false);

        bool securityStepSent = base.sendSecuritySteps(prompt);

        if (!securityStepSent)
        {
            _msgUIComponent.EndPendingMessage("Fallo de conexion, escriba de nuevo la pregunta");
        }

        return securityStepSent;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("DocumentBudgetResponse", new PropertyInfo(JsonDataType.Integer));

        _stepsSchema = new JsonSchema();

        _schemasCreated = true;
    }

    private DocumentType fromClientDocumentToDocumentType(ClientPromptType type)
    {
        switch (type)
        {
            case ClientPromptType.Perito:

                return DocumentType.Perito;
            case ClientPromptType.Report:

                return DocumentType.Report;
            case ClientPromptType.Witness:

                return DocumentType.Witness;
            case ClientPromptType.DocAlt:

                return DocumentType.ReceiptFacture;

            default:
                return DocumentType.Report;

        }
    }

    private void Awake()
    {
        createJsonSchemas();
    }
}
