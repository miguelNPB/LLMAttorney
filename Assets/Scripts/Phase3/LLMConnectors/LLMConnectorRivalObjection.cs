using System;
using Telemetry;
using UnityEngine;

/// <summary>
/// LLMConnector para promptear un booleano al LLM y dictar si la recursion del rival es valida o no
/// Debe rellenar el context con valores del documento a dictar si es valido o no y informacion del caso
/// </summary>
public class LLMConnectorRivalObjection : LLMConnector
{
    [Serializable]
    private class RivalObjectionResponse
    {
        public bool valid;
    }

    private Action<bool> _responseCallback;
    private string _baseContext;

    private void Start()
    {
        _baseContext = _llmConfigs[_configIndex].GetContext();
    }

    public void SendPrompt(string documentContent, Action<bool> onRecievePrompt, Action<string> errorCallback)
    {
        string newContext = _baseContext.Replace("@", GameSystem.Instance.CaseData.caseDescription);
        newContext = newContext.Replace("$", documentContent);
        _llmConfigs[_configIndex].OverrideContext(newContext);

        _responseCallback = onRecievePrompt;
        sendPrompt(recieveFinalResponse, errorCallback, documentContent, 0);

        _messageID = EventManager.Instance.getMessageID();
        Telemetry.TelemetryDispatch.SendQueryPost(_messageID);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        RivalObjectionResponse jsonResponse = JsonUtility.FromJson<RivalObjectionResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.valid);
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        RivalObjectionResponse jsonResponse = JsonUtility.FromJson<RivalObjectionResponse>(serializedResponse);
        return jsonResponse.valid.ToString();
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        return ""; // no se usan steps
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("valid", new PropertyInfo(JsonDataType.Boolean));
    }
}
