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
    

    [SerializeField] private ProcuradorMessagesPage _procuradorPage;


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

    public override void RecieveChatMessage(bool success, string answer)
    {
#if DEBUG
        Debug.Log("[OpponentDoc] " + answer);
#endif

        if (!success)
        {
            Debug.LogError("[OpponentDoc] LLM error: " + answer);
            //_procuradorPage.CancelPendingOpponentMessage();
            _promptSent = false;
            _stepCounter = 0;
            return;
        }

        if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
        {
            SendSecuritySteps(answer);
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
            //_procuradorPage.CancelPendingOpponentMessage();
            return;
        }

        if (response == null || !response.DocumentoValido)
        {
            //_procuradorPage.CancelPendingOpponentMessage();
            return;
        }

        //GameSystem.Instance.myDocumentManager.CreateOpponentDocument(
        //    response.NombreDocumento,
        //    (PromptType)response.TipoDocumento,
        //    response.ContenidoDocumento,
        //    response.DocumentoValido,
        //    response.CosteDocumento
        //);

        //_procuradorPage.ReceiveOpponentDocMessage(response.answer);
    }


    protected override bool SendContextMessage(int indexConfig = 0)
    {
        //_procuradorPage.StartPendingOpponentMessage();

        bool sent = base.SendContextMessage(indexConfig);

        //if (!sent)
        //    _procuradorPage.CancelPendingOpponentMessage();

        return sent;
    }

    protected override bool SendSecuritySteps(string prompt)
    {
        //_procuradorPage.StartPendingOpponentMessage();

        bool sent = base.SendSecuritySteps(prompt);

        if (!sent)
            return false;
            //_procuradorPage.CancelPendingOpponentMessage();

        return sent;
    }


    public void CallSendContext(int indexConfig = 0)
    {
        SendContextMessage(indexConfig);
    }

    private void Awake()
    {
        createJsonSchemas();
    }
}