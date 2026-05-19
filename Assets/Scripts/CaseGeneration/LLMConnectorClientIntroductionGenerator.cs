using System;
using UnityEngine;

/// <summary>
/// LLMConnector para generar el texto de introduccion que cuenta el cliente a forma de resumen del caso.
/// </summary>
public class LLMConnectorClientIntroductionGenerator : LLMConnector
{
    [Serializable]
    private class CaseIntroduction
    {
        public string introduction;
    }

    Action<string> _responseCallback;

    /// <summary>
    /// Activa el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="errorCallback"></param>
    /// <param name="caseContent"></param>
    public void SendPrompt(Action<string> responseCallback, Action<string> errorCallback, string caseContent)
    {
        _responseCallback = responseCallback;

        sendPrompt(receiveResponse, errorCallback, caseContent, 0);
    }

    /// <summary>
    /// Recibe la respuesta con el contenido del texto de la introduccion del cliente
    /// </summary>
    /// <param name="answer"></param>
    private void receiveResponse(string finalSerializedAnswer)
    {
        CaseIntroduction jsonResponse = JsonUtility.FromJson<CaseIntroduction>(finalSerializedAnswer);
        _responseCallback(finalSerializedAnswer);
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        CaseIntroduction jsonResponse = JsonUtility.FromJson<CaseIntroduction>(serializedResponse);
        return jsonResponse.introduction;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        CaseIntroduction jsonResponse = JsonUtility.FromJson<CaseIntroduction>(serializedResponse);
        return jsonResponse.introduction;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("introduction", new PropertyInfo(JsonDataType.String));
    }
}