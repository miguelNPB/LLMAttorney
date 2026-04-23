using System;
using UnityEngine;

public class LLMConnectorRivalObjection : LLMConector
{
    [Serializable]
    private class RivalObjectionResponse
    {
        public bool valid;
    }

    private Action<bool> _onRecievePrompt;

    public void SendPrompt(Action<bool> onRecievePrompt)
    {
        _onRecievePrompt = onRecievePrompt;
        sendContextPrompt(0);
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

            _onRecievePrompt?.Invoke(response.valid);
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _onRecievePrompt?.Invoke(false);
        }
    }
}
