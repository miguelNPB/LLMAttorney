using System;
using System.Collections.Generic;
using Telemetry;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LLMConnectorOpponentDocuments : LLMConector
{
    [Serializable]
    private class OpponentDocResponse
    {
        public string content;
    }
    

    [SerializeField] private ProcuratorChatPage _procuradorPage;

    public string oppPrompt;

    private int _messageID;


    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("content", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("content", new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    protected override void receiveResponse(bool success, string answer)
    {

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

        if (response == null)
        {
            TelemetryDispatch.SendNotConsistentAnswer(_messageID);
            _procuradorPage.CancelPendingOpponentMessage();
            return;
        }
        /*
        string log =
                $"[Fase: {SceneManager.GetActiveScene().buildIndex}] [Envio: {_messageID}] Nombre del documento del rival: {response.NombreDocumento}.\n\n" +
                $"Tipo de documento: {(DocumentType)response.TipoDocumento}\n" +
                $"Contenido del documento: {response.ContenidoDocumento}\n" +
                $"Respuesta coherente: true";

        TelemetryDispatch.SendReceivedDocument(response.TipoDocumento, response.DocumentoValido);

        TelemetryDispatch.SendQueryReceived(_messageID);

        GameSystem.Instance.CaseData.documentManager.CreateDocument(
           response.NombreDocumento,
           (DocumentType)response.TipoDocumento,
           response.ContenidoDocumento,
           false,
           0,
            true,
            true
        );
        */

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

            // sumar al contexto los documentos enviados del player
            DocumentManager docManager = GameSystem.Instance.CaseData.documentManager;
            List<uint> playerDocuments = docManager.GetPlayerDocs();
            string playerSentDocsInfo = "";
            for (int i = 0; i < playerDocuments.Count; i++)
            {
                Document doc = docManager.GetDocument(playerDocuments[i]);

                if (doc.IsSentToProcurador())
                {
                    playerSentDocsInfo += "\n" + doc.GetType() + ", " + doc.GetDocName() + ":\n" + doc.GetContent();
                }
            }

            // Grab all documents from the document manager to add to the context
            string documentsContext = "Documentos actuales enviados por el jugador:\n" + playerSentDocsInfo + "\n";

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
        _messageID = LLMLogManager.Instance.getMessageID();

        TelemetryDispatch.SendQueryPost(_messageID);

        sendContextPrompt(indexConfig);
    }

    private void Awake()
    {
        createJsonSchemas();
    }
}