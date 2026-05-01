using System;
using System.Collections.Generic;
using Telemetry;
using UnityEngine;

public class LLMConnectorTextSentence : LLMConector
{
    [Serializable]
    private class TextSentenceResponse
    {
        public string sentence;
    }

    [SerializeField, TextArea(3, 10)] private string _prompt;
    [SerializeField, TextArea(3, 10)] private string _winPlayerText;
    [SerializeField, TextArea(3, 10)] private string _losePlayerText;

    private Action<string> _onRecievePrompt;
    private string _basePrompt;
    private int _messageID;
    public void PromptTextSentence(bool playerWin, Action<string> onRecievePrompt)
    {
        _onRecievePrompt = onRecievePrompt;

        string lawsuitText = GameSystem.Instance.CaseData.lawsuitText;
        string clientDocs = "";
        string rivalDocs = "";

        string resolutionText = playerWin ? _winPlayerText : _losePlayerText;

        List<Document> clientFinalDocuments = GameSystem.Instance.CaseData.finalPlayerDocuments;
        List<Document> rivalFinalDocuments = GameSystem.Instance.CaseData.finalRivalDocuments;

        for (int i = 0; i < clientFinalDocuments.Count; i++)
        {
            clientDocs += (i + ": " + clientFinalDocuments[i].GetDocName() + ": " + clientFinalDocuments[i].GetContent() + "\n");
        }

        for (int i = 0; i < rivalFinalDocuments.Count; i++)
        {
            rivalDocs += (i + ": " + rivalFinalDocuments[i].GetDocName() + ": " + rivalFinalDocuments[i].GetContent() + "\n");
        }

        _prompt = _basePrompt.Replace("#", lawsuitText);
        _prompt = _prompt.Replace("@", clientDocs);
        _prompt = _prompt.Replace("$", rivalDocs);
        _prompt = _prompt.Replace("%", resolutionText);

        _prompt = _prompt.Replace("!", GameSystem.Instance.CaseData.clientName);
        _prompt = _prompt.Replace("¡", GameSystem.Instance.CaseData.demandedEntityName);


        _messageID = LLMLogManager.Instance.getMessageID();
        Telemetry.TelemetryDispatch.SendQueryPost(_messageID);

        sendContextPrompt(_prompt, 0);
    }


    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("sentence", new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    protected override void receiveResponse(bool success, string answer)
    {
        TelemetryDispatch.SendQueryReceived(_messageID);

        if (success)
        {
            TextSentenceResponse response = JsonUtility.FromJson<TextSentenceResponse>(answer);
            Debug.Log("SentenceText: " + response.sentence);
            _onRecievePrompt?.Invoke(response.sentence);
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _onRecievePrompt?.Invoke("Error en la llamada al LLM: " + answer);
        }
    }

    private void Awake()
    {
        createJsonSchemas();
        _basePrompt = _prompt;
    }
}
