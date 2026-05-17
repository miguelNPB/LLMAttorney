using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class LLMConnectorClientIntroductionGenerator : LLMConnector
{
    [Serializable]
    private class CaseIntroduction
    {
        public string introduction;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("introduction", new PropertyInfo(JsonDataType.String));
    }

    public void SendPrompt(Action<string> responseCallback, string prompt)
    {
        sendPrompt(receiveResponse, prompt, 0);
    }


    private void receiveResponse(string answer)
    {
        CaseIntroduction jsonResponse = JsonUtility.FromJson<CaseIntroduction>(answer);
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "CaseSummary.json");
        File.WriteAllText(filePath, answer);
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