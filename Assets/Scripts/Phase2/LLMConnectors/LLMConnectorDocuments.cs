using UnityEngine;

//Para generar documentos a partir de la llm

public class LLMConnectorDocuments : LLMConector
{
    
    private class DocumentResponse
    {
        public string NombreDocumento;
        public PromptType TipoDocumento;
        public string ContenidoDocumento;
        public bool DocumentoValido;
        public int CosteDocumento;
        public bool DocumentoCoherente;
    }

    [SerializeField]
    private ChatPage _msgUIComponent;

    private bool firstTime = true;

    protected override void recieveResponse(bool success, string answer)
    {
        #if DEBUG
        Debug.Log(answer);
#endif

        Debug.Log("Step: " + _stepCounter);

        if (success)
        {
            // deserializamos la respuesta
            DocumentResponse jsonResponse = JsonUtility.FromJson<DocumentResponse>(answer);

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
            {
                sendSecuritySteps(answer);
            }
            else
            {
                Debug.Log("Respuesta final");
        
                _stepCounter = 0;
                _promptSent = false;

                if (jsonResponse.DocumentoCoherente)
                {
                    _historical.Add("Respuesta :" + answer);
                    GameSystem.Instance.myDocumentManager.CreateDocument(jsonResponse.NombreDocumento, jsonResponse.TipoDocumento, jsonResponse.ContenidoDocumento, jsonResponse.DocumentoValido, jsonResponse.CosteDocumento);
                    _msgUIComponent.computerSystem.ToggleNotification(Page.ClientChat, true);
                    _msgUIComponent.EndPendingMessage("Tu cliente te ha mandado " + jsonResponse.NombreDocumento + ".txt");
                }
                else
                {
                    _historical.Add("Respuesta: texto final sin coherencia");
                    _msgUIComponent.computerSystem.ToggleNotification(Page.ClientChat, true);
                    _msgUIComponent.EndPendingMessage("Perdona pero no he podido conseguir el documento, ¿Puedes ser un poco mas especifico?");
                    
                }
                
            }
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _msgUIComponent.EndPendingMessage("Error al contactar con el modelo.");
        }
    }

    public void CallSendContext(int indexConfig = 0)
    {
        sendContextPrompt(indexConfig);
    }

    /**
     * Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
     */
    protected override bool sendContextPrompt(int indexConfig = 0)
    {
        //_msgUIComponent.StartPendingMessage(false);
        if(firstTime)
        {
            _historical.Add(GameSystem.Instance.CaseData.caseDescription);
            firstTime = false;
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
        _contextSchema.properties.Add("TipoDocumento", new PropertyInfo(JsonDataType.Integer));
        _contextSchema.properties.Add("ContenidoDocumento", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("DocumentoValido", new PropertyInfo(JsonDataType.Boolean));
        _contextSchema.properties.Add("CosteDocumento", new PropertyInfo(JsonDataType.Integer));
        _contextSchema.properties.Add("DocumentoCoherente", new PropertyInfo(JsonDataType.Boolean));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("NombreDocumento", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("TipoDocumento", new PropertyInfo(JsonDataType.Integer));
        _stepsSchema.properties.Add("ContenidoDocumento", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("DocumentoValido", new PropertyInfo(JsonDataType.Boolean));
        _stepsSchema.properties.Add("CosteDocumento", new PropertyInfo(JsonDataType.Integer));
        _stepsSchema.properties.Add("DocumentoCoherente", new PropertyInfo(JsonDataType.Boolean));


        _schemasCreated = true;
    }

    private void Awake()
    {
        createJsonSchemas();
    }

}
