using System;
using UnityEngine;

/// <summary>
/// LLMConnector para generar el contenido de los documentos
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
    /// <param name="isPlayer">Si para el cliente el documento o del rival</param>
    /// <param name="isValid">Si el documento es valido en la audiencia previa o no</param>
    public void SendPrompt(Action<string, string> responseCallback, Action<string> errorCallback, string prompt, DocumentType docType, bool isPlayer, bool isValid = true)
    {
        clearHistoricText();
        appendHistoricText(GameSystem.Instance.CaseData.caseDescription);

        _responseCallback = responseCallback;

        int configIndex = 0;
        switch (docType)
        {
            case DocumentType.Perito:
                configIndex = isPlayer ? 0 : (isValid ? 4 : 8);
                break;
            case DocumentType.Report:
                configIndex = isPlayer ? 1 : (isValid ? 5 : 9);
                break;
            case DocumentType.Witness:
                configIndex = isPlayer ? 2 : (isValid ? 6 : 10);
                break;
            case DocumentType.ReceiptFacture:
                configIndex = isPlayer ? 3 : (isValid ? 7 : 11);
                break;
        }
        sendPrompt(recieveFinalResponse, errorCallback, prompt, configIndex);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
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

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("NombreDocumento", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("ContenidoDocumento", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("NombreDocumento", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("ContenidoDocumento", new PropertyInfo(JsonDataType.String));
    }
}
