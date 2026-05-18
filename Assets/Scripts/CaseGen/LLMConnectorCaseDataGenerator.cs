using System;
using System.IO;
using UnityEngine;

public class LLMConnectorCaseDataGenerator : LLMConnector
{
    [Serializable]
    public class CaseDataRetrieval          // public so UI can reference the type
    {
        public string clientName;
        public string rivalName;
        public string caseSummary;
    }

    public CaseDataRetrieval LastResponse { get; private set; }

    public event Action<CaseDataRetrieval> OnDataReceived;

    private Action<string> _responseCallback;

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("clientName", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("rivalName",  new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("caseSummary",new PropertyInfo(JsonDataType.String));
    }

    public void SendPrompt(Action<string> responseCallback, Action<string> errorCallback, string prompt)
    {
        _responseCallback = responseCallback;
        sendPrompt(receiveResponse, errorCallback, prompt, 0);
    }

    private void receiveResponse(string answer)
    {
        LastResponse = JsonUtility.FromJson<CaseDataRetrieval>(answer);

        if (LastResponse == null ||
            string.IsNullOrEmpty(LastResponse.clientName) ||
            string.IsNullOrEmpty(LastResponse.rivalName)  ||
            string.IsNullOrEmpty(LastResponse.caseSummary))
        {
            Debug.LogWarning("[CaseDataGenerator] Invalid or incomplete response.");
            return;
        }

        OnDataReceived?.Invoke(LastResponse);
        _responseCallback?.Invoke(answer);
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        JsonUtility.FromJson<CaseDataRetrieval>(serializedResponse);
        return serializedResponse;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        JsonUtility.FromJson<CaseDataRetrieval>(serializedResponse);
        return serializedResponse;
    }

    private void Awake() => createJsonSchemas();
}