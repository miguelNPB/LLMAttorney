using Telemetry;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LLMConnectorDocumentsChecker : LLMConector
{

    private class DocumentCheckerResponse
    {
        public bool DocumentoCoherente;
    }

    [SerializeField]
    private ChatPage _msgUIComponent;

    [SerializeField]
    private LLMConnectorDocumentsBudget _budget;

    private bool _firstTime = true;

    private ClientPromptType _type;
    private string _docName;
    private string _docContent;
    private int _messageID;

    protected override void receiveResponse(bool success, string answer)
    {

        if (success)
        {
            // deserializamos la respuesta
            DocumentCheckerResponse jsonResponse = JsonUtility.FromJson<DocumentCheckerResponse>(answer);

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
            {
                sendSecuritySteps(answer);
            }
            else
            {
                _stepCounter = 0;
                _promptSent = false;

                _historical.Add("Respuesta :" + answer);

                if (jsonResponse.DocumentoCoherente)
                {
                    _budget.CallSendContext(_type, _docName, _docContent, _messageID, 0);
                }
                else
                {
                    _historical.Add("Respuesta: texto final sin coherencia");
                    _msgUIComponent._computerSystem.ToggleNotification(Page.ClientChat, true);
                    _msgUIComponent.EndPendingMessage("Perdona pero no he podido conseguir el documento, ¿Puedes ser un poco mas especifico?");
                    TelemetryDispatch.SendNotConsistentAnswer(_messageID);
                }

            }
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _msgUIComponent.EndPendingMessage("Error al contactar con el modelo.");
        }
    }

    public void CallSendContext(ClientPromptType type, string docName, string docContent, int messageID, int indexConfig = 0)
    {
        _docName = docName;
        _docContent = docContent;
        _type = type;
        _messageID = messageID;

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
        _contextSchema.properties.Add("DocumentoCoherente", new PropertyInfo(JsonDataType.Boolean));

        _stepsSchema = new JsonSchema();

        _schemasCreated = true;
    }

    private void Awake()
    {
        createJsonSchemas();
    }

}
