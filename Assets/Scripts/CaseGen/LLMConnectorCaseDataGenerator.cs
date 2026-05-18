using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class LLMConnectorCaseDataGenerator : LLMConnector
{
    [Serializable]
    private class CaseDataRetrieval
    {
        public string clientName;
        public string rivalName;
        public string caseSummary;

    }

    private Action<string> _responseCallback;

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("clientName", new PropertyInfo(JsonDataType.String));

        _contextSchema.properties.Add("rivalName", new PropertyInfo(JsonDataType.String));

        _contextSchema.properties.Add("caseSummary", new PropertyInfo(JsonDataType.String));

    }

    public void SendPrompt(Action<string> responseCallback, Action<string> errorCallback,  string prompt)
    {
        _responseCallback = responseCallback;
        sendPrompt(receiveResponse,errorCallback, prompt, 0);
    }


    private void receiveResponse(string answer)
    {
        CaseDataRetrieval jsonResponse = JsonUtility.FromJson<CaseDataRetrieval>(answer);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "CaseData.json");
        File.WriteAllText(filePath, answer);

        _responseCallback?.Invoke(answer);
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        CaseDataRetrieval jsonResponse = JsonUtility.FromJson<CaseDataRetrieval>(serializedResponse);
        return serializedResponse;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        CaseDataRetrieval jsonResponse = JsonUtility.FromJson<CaseDataRetrieval>(serializedResponse);
        return serializedResponse;
    }
    private void Awake()
    {
        createJsonSchemas();
    }

    
}