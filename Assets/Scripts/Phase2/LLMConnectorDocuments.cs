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
    }

    [SerializeField]
    private MessagesUIComponent _msgUIComponent;


    public override void RecieveChatMessage(bool success, string answer)
    {
        #if DEBUG
        Debug.Log(answer);
        #endif

        if (success)
        {
            // deserializamos la respuesta
            DocumentResponse jsonResponse = JsonUtility.FromJson<DocumentResponse>(answer);

            GameSystem.Instance.myDocumentManager.CreateDocument(jsonResponse.NombreDocumento, jsonResponse.TipoDocumento, jsonResponse.ContenidoDocumento, jsonResponse.DocumentoValido, jsonResponse.CosteDocumento);

            
            _msgUIComponent.EndPendingMessage("Tu cliente te ha mandado " + jsonResponse.NombreDocumento + ".txt");



            _msgUIComponent.computerSystem.ToggleNotification(Page.ChatCliente, true);

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
            {
                SendSecuritySteps(answer);
            }
            else
            {
                Debug.Log("Respuesta final");
                _historical.Add("Respuesta :" + answer);
                _stepCounter = 0;
                _promptSent = false;
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
        SendContextMessage(indexConfig);
    }

    /**
     * Metodo encargado de enviar un mensaje al LLM con todas las especificaciones obtenidas de ConfigLLMInfo
     */
    protected override bool SendContextMessage(int indexConfig = 0)
    {
        _msgUIComponent.StartPendingMessage(false);

        bool messageSent = base.SendContextMessage(indexConfig);

        if (!messageSent)
        {
            _msgUIComponent.EndPendingMessage("Fallo de conexion, escriba de nuevo la pregunta");
        }

        return messageSent;
    }

    protected override bool SendSecuritySteps(string prompt)
    {
        _msgUIComponent.StartPendingMessage(false);

        bool securityStepSent = base.SendSecuritySteps(prompt);

        if (!securityStepSent)
        {
            _msgUIComponent.EndPendingMessage("Fallo de conexion, escriba de nuevo la pregunta");
        }

        return securityStepSent;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    private void Awake()
    {
        createJsonSchemas();
    }

}
