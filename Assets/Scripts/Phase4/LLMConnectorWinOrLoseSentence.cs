using System;
using System.Collections.Generic;
using Telemetry;
using UnityEngine;

public class LLMConnectorWinOrLoseSentence : LLMConnector
{
    [Serializable]
    private class WinOrLoseSentenceResponse
    {
        public bool winPlayer;
    }

    [SerializeField, TextArea(3, 10)] private string _promptTemplate = "Analiza el siguiente caso y dicta un texto con la sentencia:\r\n\r\nResumen del caso: ~\r\n\r\nPruebas presentadas por !:\r\n@\r\n\r\nPruebas presentadas por ¡:\r\n$\r\n\r\nDemanda de !:\r\n#\r\n\r\n\r\nGenera la sentencia del tribunal OBJETIVAMENTE comprobando que LA DEMANDA SE CORRESPONDE A LAS PRUEBAS PRESENTADAS. Si no se corresponde, invalidar las pruebas.\r\nAdemás valorar ambas pruebas y contrastarlas con el resumen del caso";
    private Action<bool> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<bool> onRecievePrompt)
    {
        _responseCallback = onRecievePrompt;

        string lawsuitText = GameSystem.Instance.CaseData.lawsuitText;
        string clientDocs = "";
        string rivalDocs = "";

        List<Document> clientFinalDocuments = GameSystem.Instance.CaseData.finalPlayerDocuments;
        List<Document> rivalFinalDocuments = GameSystem.Instance.CaseData.finalRivalDocuments;

        for (int i = 0; i < clientFinalDocuments.Count; i++)
        {
            clientDocs += (i + ": " + clientFinalDocuments[i].GetDocName() + ": " + clientFinalDocuments[i].GetContent() + "\n");
        }

        if (clientFinalDocuments.Count == 0)
            clientDocs = "Ninguna.";

        for (int i = 0; i < rivalFinalDocuments.Count; i++)
        {
            rivalDocs += (i + ": " + rivalFinalDocuments[i].GetDocName() + ": " + rivalFinalDocuments[i].GetContent() + "\n");
        }

        if (rivalFinalDocuments.Count == 0)
            rivalDocs = "Ninguna.";

        _prompt = _promptTemplate.Replace("#", lawsuitText);
        _prompt = _prompt.Replace("@", clientDocs);
        _prompt = _prompt.Replace("$", rivalDocs);

        _prompt = _prompt.Replace("!", GameSystem.Instance.CaseData.clientName);
        _prompt = _prompt.Replace("¡", GameSystem.Instance.CaseData.demandedEntityName);

        _prompt = _prompt.Replace("~", GameSystem.Instance.CaseData.caseDescription);

        sendPrompt(recieveFinalResponse, _prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        WinOrLoseSentenceResponse jsonResponse = JsonUtility.FromJson<WinOrLoseSentenceResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.winPlayer);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("winPlayer", new PropertyInfo(JsonDataType.Boolean));
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        return ""; // no hay steps
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        return ""; // no hay steps
    }
}
