using System;
using UnityEngine;

/// <summary>
/// LLMConnector para obtener la informacion resumen de un caso y los nombres de las partes
/// </summary>
public class LLMConnectorCaseDataGenerator : LLMConnector
{
    [Serializable]
    private class CaseDataResponse
    {
        public string clientName;
        public string rivalName;
        public string caseSummary;
    }

    [TextArea(2, 5)]
    [Tooltip("User prompt para disparar la generacion del contenido del caso.")]
    [SerializeField] private string _promptGenerateCaseDatas = "Genera un caso de responsabilidad civil extracontractual completamente inventado siguiendo la estructura indicada.";

    private Action<string, string, string> _responseCallback;

    /// <summary>
    /// Metodo publico que activa el funcionamiento de este llmconnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="errorCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<string, string, string> responseCallback, Action<string> errorCallback)
    {
        _responseCallback = responseCallback;
        sendPrompt(receiveResponse, errorCallback, _promptGenerateCaseDatas, 0);
    }

    /// <summary>
    /// Recibe la resupesta final con el contenido del caso y los nombres
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void receiveResponse(string finalSerializedResponse)
    {
        CaseDataResponse jsonResponse = JsonUtility.FromJson<CaseDataResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.clientName, jsonResponse.rivalName, jsonResponse.caseSummary);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("clientName", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("rivalName",  new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("caseSummary",new PropertyInfo(JsonDataType.String));
    }
}