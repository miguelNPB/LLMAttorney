using System;
using UnityEngine;

/// <summary>
/// LLMConnector enfocado en escribir el el contenido de los documentos pedidos segun el formato marcadompor la configuracion
/// </summary>
public class LLMConnectorDocuments : LLMConnector
{
    private class DocumentResponse
    {
        public string documentName;
        public string documentContent;
    }

    Action<string, string> _responseCallback;

    // ORDEN DE LOS LLMCONFIG
    // 0 = client perito
    // 1 = client informe
    // 2 = client testigo
    // 3 = client factura
    // 4 = rival perito valido
    // 5 = rival informe valido
    // 6 = rival testigo valido
    // 7 = rival factura valido
    // 8 = rival perito no valido
    // 9 = rival informe no valido
    // 10 = rival testigo no valido
    // 11 = rival factura no valido

    /// <summary>
    /// Manda el prompt de generar el documento.
    /// </summary>
    /// <param name="prompt">Prompt</param>
    /// <param name="docType">Tipo de documento</param>
    /// <param name="isOpponent">Si para el cliente el documento o del rival</param>
    /// <param name="isValid">Si el documento es valido en la audiencia previa o no</param>
    public void SendPrompt(Action<string, string> responseCallback, Action<string> errorCallback, string prompt, DocumentType docType, bool isOpponent, bool isValid = true)
    {
        clearHistoricText();
        appendHistoricText(GameSystem.Instance.CaseData.caseDescription);

        _responseCallback = responseCallback;

        int configIndex = 0;
        switch (docType)
        {
            case DocumentType.Perito:
                configIndex = isOpponent ? (isValid ? 4 : 8) : 0;
                break;
            case DocumentType.Report:
                configIndex = isOpponent ? (isValid ? 5 : 9) : 1;
                break;
            case DocumentType.Witness:
                configIndex = isOpponent ? (isValid ? 6 : 10) : 2;
                break;
            case DocumentType.ReceiptFacture:
                configIndex = isOpponent ? (isValid ? 7 : 11) : 3;
                break;
        }
        sendPrompt(recieveFinalResponse, errorCallback, prompt, configIndex);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse">Texto en formato json devuelto por el servidor que cuenta con los atributos rellenados por el LLM</param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        DocumentResponse jsonResponse = JsonUtility.FromJson<DocumentResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.documentName, jsonResponse.documentContent);
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        DocumentResponse jsonResponse = JsonUtility.FromJson<DocumentResponse>(serializedResponse);

        return jsonResponse.documentContent;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        DocumentResponse jsonResponse = JsonUtility.FromJson<DocumentResponse>(serializedResponse);

        return jsonResponse.documentContent;
    }

    /// <summary>
    /// Creacion de los esquemas especificos para este conector
    /// </summary>
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("documentName", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("documentContent", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("documentName", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("documentContent", new PropertyInfo(JsonDataType.String));
    }
}
