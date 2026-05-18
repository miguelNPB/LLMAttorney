using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class LLMConnectorClientIntroductionGenerator : LLMConnector
{
    [Serializable]
    public class CaseIntroduction
    {
        public string introduction;
    }

    public CaseIntroduction LastResponse { get; private set; }

    public event Action<CaseIntroduction> OnIntroductionReceived;
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("introduction", new PropertyInfo(JsonDataType.String));
    }

    public void SendPrompt(Action<string> responseCallback, Action<string> errorCallback, string prompt)
    {
        sendPrompt(receiveResponse,errorCallback, prompt, 0);
    }


    private void receiveResponse(string answer)
    {
        LastResponse = JsonUtility.FromJson<CaseIntroduction>(answer);
        OnIntroductionReceived?.Invoke(LastResponse);
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        CaseIntroduction jsonResponse = JsonUtility.FromJson<CaseIntroduction>(serializedResponse);
        return serializedResponse;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        CaseIntroduction jsonResponse = JsonUtility.FromJson<CaseIntroduction>(serializedResponse);
        return serializedResponse;
    }
    private void Awake()
    {
        createJsonSchemas();
    }


}