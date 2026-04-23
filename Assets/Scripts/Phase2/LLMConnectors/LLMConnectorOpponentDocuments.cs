using System;
using System.Collections.Generic;
using UnityEngine;

public class LLMConnectorOpponentDocuments : LLMConector
{
    [Serializable]
    private class OpponentDocResponse
    {
        public string NombreDocumento;
        public int    TipoDocumento;
        public string ContenidoDocumento;
        public bool   DocumentoValido;
        public int    CosteDocumento;
        public string answer;
    }
    

    [SerializeField] private ProcuratorChatPage _procuradorPage;

    [SerializeField] public string oppPrompt;



    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("NombreDocumento",    new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("TipoDocumento",      new PropertyInfo(JsonDataType.Integer));
        _contextSchema.properties.Add("ContenidoDocumento", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("DocumentoValido",    new PropertyInfo(JsonDataType.Boolean));
        _contextSchema.properties.Add("CosteDocumento",     new PropertyInfo(JsonDataType.Integer));
        _contextSchema.properties.Add("answer",             new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("NombreDocumento",    new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("TipoDocumento",      new PropertyInfo(JsonDataType.Integer));
        _stepsSchema.properties.Add("ContenidoDocumento", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("DocumentoValido",    new PropertyInfo(JsonDataType.Boolean));
        _stepsSchema.properties.Add("CosteDocumento",     new PropertyInfo(JsonDataType.Integer));
        _stepsSchema.properties.Add("answer",             new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    protected override void receiveResponse(bool success, string answer)
    {
#if DEBUG
        Debug.Log("[OpponentDoc] " + answer);
#endif

        if (!success)
        {
            Debug.LogError("[OpponentDoc] LLM error: " + answer);
            _procuradorPage.CancelPendingOpponentMessage();
            _promptSent = false;
            _stepCounter = 0;
            return;
        }

        if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
        {
            sendSecuritySteps(answer);
            return;
        }

        _historical.Add("Respuesta: " + answer);
        _stepCounter = 0;
        _promptSent  = false;

        OpponentDocResponse response;
        try
        {
            response = JsonUtility.FromJson<OpponentDocResponse>(answer);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[OpponentDoc] JSON parse failed: " + e.Message);
            _procuradorPage.CancelPendingOpponentMessage();
            return;
        }

        if (response == null || !response.DocumentoValido)
        {
            _procuradorPage.CancelPendingOpponentMessage();
            return;
        }

        GameSystem.Instance.myDocumentManager.CreateDocument(
           response.NombreDocumento,
           (PromptType)response.TipoDocumento,
           response.ContenidoDocumento,
           false,
           0,
            true
        );

        _procuradorPage.ReceiveOpponentDocMessage("Has recibido un documento de la parte del damandado.");
    }

    protected override bool sendContextPrompt(int indexConfig = 0)
    {

        _procuradorPage.StartPendingOpponentMessage();

        if (!_promptSent && _schemasCreated)
        {

            if (_config.Length <= 0)
            {
                Debug.LogError("Ningun Config LLM asignado");
                return false;
            }

            _indexConfig = indexConfig;

            string prompt = oppPrompt;

            string configLLM = _config[_indexConfig].context
                + _config[_indexConfig].safeguard;

            // Grab all documents from the document manager to add to the context
            string documentsContext = "Documentos actuales enviados por el jugador:\n" + GameSystem.Instance.myDocumentManager.getSentDocsInfo() + "\n";

            configLLM = configLLM + "\n " + _config[_indexConfig].historicalConversation + "\n Historico: \n";

            // if (_useHistoricalInContext)
            // {
            //     foreach (String s in _historical)
            //     {
            //         configLLM = configLLM + s + "\n";
            //     }
            // }

            configLLM = configLLM + documentsContext;
            
            Debug.Log("PROMPT: " + prompt);
            Debug.Log("CONTEXT: " + configLLM);

            _historical.Add("Pregunta: " + prompt);

            _promptSent = true;

            StartCoroutine(coroutineSendPrompt(prompt, configLLM, _contextSchema));

            //inputField.text = "";

            return true;
        }

        _procuradorPage.CancelPendingOpponentMessage();

        return false;

    }



    // protected override bool SendContextMessage(int indexConfig = 0)
    // {
    //     _procuradorPage.StartPendingOpponentMessage();

    //     bool sent = base.SendContextMessage(indexConfig);

    //     if (!sent)
    //        _procuradorPage.CancelPendingOpponentMessage();

    //     return sent;
    // }

    // protected override bool SendSecuritySteps(string prompt)
    // {
    //     _procuradorPage.StartPendingOpponentMessage();

    //     bool sent = base.SendSecuritySteps(prompt);

    //     if (!sent)
    //         return false;
    //         _procuradorPage.CancelPendingOpponentMessage();

    //     return sent;
    // }

    protected override bool sendSecuritySteps(string prompt)
    {
        
        Debug.Log("PROMPT de security checks: " + prompt);

        string configLLM = "Teniendo el siguiente texto: \n" + prompt + "\n Y teniedo la siguiente directiva de seguridad" +
            _config[_indexConfig].safeguardSteps +
            "\n Quiero que hagas lo siguiente: " + _config[_indexConfig].getStepsChecks()[_stepCounter];

        if (_useHistoricalInSteps)
        {
            foreach (String s in _historical)
            {
                configLLM = configLLM + s + "\n";
            }
        }

        StartCoroutine(coroutineSendPromptSteps(prompt, configLLM, _stepsSchema));

        //inputField.text = "";

        _stepCounter++;

        return true;
    }


    public void CallSendContext(int indexConfig = 0)
    {
        sendContextPrompt(indexConfig);
    }

    private void Awake()
    {
        createJsonSchemas();
    }
}