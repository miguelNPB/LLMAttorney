using System;
using Telemetry;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LLMConnectorBudgetChecker : LLMConnector
{
    [Serializable]
    private class BudgetCheckerResponse
    {
        public bool budgetCoherent;
        public float budget;
    }
    private Action<bool, float> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<bool, float> responseCallback, string prompt)
    {
        _responseCallback = responseCallback;
        sendPrompt(recieveFinalResponse, prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        BudgetCheckerResponse jsonResponse = JsonUtility.FromJson<BudgetCheckerResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.budgetCoherent, jsonResponse.budget);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("budgetCoherent", new PropertyInfo(JsonDataType.Boolean));
        _contextSchema.properties.Add("budget", new PropertyInfo(JsonDataType.Float));

        _stepsSchema = new JsonSchema();
    }

}
