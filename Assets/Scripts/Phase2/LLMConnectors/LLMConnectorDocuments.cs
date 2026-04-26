using UnityEngine;

//Para generar documentos a partir de la llm

public class LLMConnectorDocuments : LLMConector
{
    private class DocumentResponse
    {
        public string NombreDocumento;
        public string ContenidoDocumento;
    }

    [SerializeField]
    private ChatPage _msgUIComponent;

    [SerializeField]
    private LLMConnectorDocumentsChecker _checker;

    private bool _firstTime = true;

    private ClientPromptType _type;

    private int _messageID = -1;

    protected override void receiveResponse(bool success, string answer)
    {

        if (success)
        {
            DocumentResponse jsonResponse = JsonUtility.FromJson<DocumentResponse>(answer);

            if (_stepCounter == 0)
            {
                _messageID = LLMLogManager.Instance.getNumMessageSent();
                LLMLogManager.Instance.addMessageSent();
            }

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
            {
                sendSecuritySteps(answer);
            }
            else
            {
                _stepCounter = 0;
                _promptSent = false;

                _historical.Add("Respuesta :" + answer);

                _checker.CallSendContext(_type, jsonResponse.NombreDocumento, jsonResponse.ContenidoDocumento, _messageID, _indexConfig);
                
            }
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _msgUIComponent.EndPendingMessage("Error al contactar con el modelo.");
        }
    }

    public void CallSendContext(ClientPromptType type, int indexConfig = 0)
    {
        _type = type;
        sendContextPrompt(indexConfig);  
    }

    /**
     * Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
     */
    protected override bool sendContextPrompt(int indexConfig = 0)
    {
        //_msgUIComponent.StartPendingMessage(false);
        if(_firstTime)
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
        _contextSchema.properties.Add("NombreDocumento", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("ContenidoDocumento", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("NombreDocumento", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("ContenidoDocumento", new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    private void Awake()
    {
        createJsonSchemas();
    }

}
