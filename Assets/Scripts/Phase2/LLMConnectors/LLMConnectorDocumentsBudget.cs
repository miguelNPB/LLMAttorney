using System;
using UnityEngine;

/// <summary>
/// LLMConnector para calcular el coste de un documento del player. Los documentos con coste son los peritos y informes (report)
/// </summary>
public class LLMConnectorDocumentsBudget : LLMConnector
{
    private class DocumentBudgetResponse
    {
        public int CosteDocumento;
    }

    private Action<int> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="onRecievePrompt"></param>
    /// <param name="errorCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<int> onRecievePrompt, Action<string> errorCallback, string prompt)
    {
        clearHistoricText();
        appendHistoricText(GameSystem.Instance.CaseData.caseDescription);

        _responseCallback = onRecievePrompt;
        sendPrompt(recieveFinalResponse, errorCallback, prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        DocumentBudgetResponse jsonResponse = JsonUtility.FromJson<DocumentBudgetResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.CosteDocumento);
    }

    protected override void createJsonSchemas()
    {
        JsonSchema schema = new JsonSchema();
        schema.properties.Add("documentQueryType", new PropertyInfo(JsonDataType.Integer));
    }
}
