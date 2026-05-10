using System;
using System.Collections.Generic;
using Telemetry;
using UnityEngine;

public class LLMConnectorWinOrLoseSentence : LLMConector
{
    [Serializable]
    private class WinOrLoseSentenceResponse
    {
        public bool winPlayer;
    }

    [SerializeField, TextArea(3, 10)] private string _prompt;

    private Action<bool> _onRecievePrompt;
    private string _basePrompt;
    private int _messageID;
    public void PromptBoolSentence(Action<bool> onRecievePrompt)
    {
        _onRecievePrompt = onRecievePrompt;

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

        _prompt = _basePrompt.Replace("#", lawsuitText);
        _prompt = _prompt.Replace("@", clientDocs);
        _prompt = _prompt.Replace("$", rivalDocs);

        _prompt = _prompt.Replace("!", GameSystem.Instance.CaseData.clientName);
        _prompt = _prompt.Replace("¡", GameSystem.Instance.CaseData.demandedEntityName);

        _prompt = _prompt.Replace("~", GameSystem.Instance.CaseData.caseDescription);

        _messageID = LLMLogManager.Instance.getMessageID();
        Telemetry.TelemetryDispatch.SendQueryPost(_messageID);

        sendContextPrompt(_prompt, 0);
    }


    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("winPlayer", new PropertyInfo(JsonDataType.Boolean));

        _schemasCreated = true;
    }

    protected override void receiveResponse(bool success, string answer)
    {
        TelemetryDispatch.SendQueryReceived(_messageID);

        if (success)
        {
            WinOrLoseSentenceResponse response = JsonUtility.FromJson<WinOrLoseSentenceResponse>(answer);
            Debug.Log("WinPlayer: " + response.winPlayer);
            _onRecievePrompt?.Invoke(response.winPlayer);
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _onRecievePrompt?.Invoke(false);
        }
    }

    private void Awake()
    {
        createJsonSchemas();
        _basePrompt = _prompt;
    }
}
