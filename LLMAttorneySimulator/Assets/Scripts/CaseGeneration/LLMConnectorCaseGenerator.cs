using System;
using UnityEngine;

/// <summary>
/// LLMConnector para sacar el contenido grande de un caso
/// </summary>
public class LLMCaseContentGenerator : LLMConnector
{
    [Serializable]
    private class CaseResponse
    {
        public string CaseContent;
    }

    [TextArea(2, 5)]
    [Tooltip("User prompt para disparar la generacion del contenido del caso.")]
    public string _caseUserPrompt = "Genera un caso de responsabilidad civil extracontractual completamente inventado siguiendo la estructura indicada.";

    private Action<string> _responseCallback;

    /// <summary>
    /// Punto de entrada para empezar la generacion del caso
    /// </summary>
    public void SendPrompt(Action<string> responseCallback, Action<string> errorCallback)
    {
        _responseCallback = responseCallback;
        sendPrompt(receiveResponse, errorCallback, _caseUserPrompt, 0);
    }

    /// <summary>
    /// Llamado al recibir el contenido final del texto
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void receiveResponse(string finalSerializedResponse)
    {
        CaseResponse jsonResponse = JsonUtility.FromJson<CaseResponse>(finalSerializedResponse);
        _responseCallback?.Invoke(jsonResponse.CaseContent);
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        CaseResponse jsonResponse = JsonUtility.FromJson<CaseResponse>(serializedResponse);
        return jsonResponse.CaseContent;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        CaseResponse jsonResponse = JsonUtility.FromJson<CaseResponse>(serializedResponse);
        return jsonResponse.CaseContent;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("CaseContent", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("CaseContent", new PropertyInfo(JsonDataType.String));
    }
}