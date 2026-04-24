using System;
using UnityEngine;

/// <summary>
/// LLMConnector para promptear un booleano al LLM y dictar si la recursion del rival es valida o no
/// </summary>
public class LLMConnectorRivalObjection : LLMConector
{
    [Serializable]
    private class RivalObjectionResponse
    {
        public bool valid;
    }
    [SerializeField, TextArea(3, 10)] private string _prompt;

    private Action<bool> _onRecievePrompt;
    private string _baseContext;
    public void SendPrompt(string documentContent, Action<bool> onRecievePrompt)
    {
        _onRecievePrompt = onRecievePrompt;

        _config[0].context = _baseContext.Replace("@", GameSystem.Instance.CaseData.caseDescription);
        _config[0].context = _config[0].context.Replace("$", documentContent);

        _promptSent = false;
        sendContextPrompt(_prompt, 0);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("valid", new PropertyInfo(JsonDataType.Boolean));

        _schemasCreated = true;
    }

    protected override void receiveResponse(bool success, string answer)
    {
        if (success)
        {
            RivalObjectionResponse response = JsonUtility.FromJson<RivalObjectionResponse>(answer);
            Debug.Log("Valid: " + response.valid);
            _onRecievePrompt?.Invoke(response.valid);
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _onRecievePrompt?.Invoke(false);
        }
    }

    private void Awake()
    {
        _baseContext = _config[0].context;

        createJsonSchemas();
    }
}
