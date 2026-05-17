using System;
using System.Collections.Generic;
using Telemetry;
using UnityEngine;

/// <summary>
/// LLMConnector para el texto de la sentencia de la fase 4.
/// </summary>
public class LLMConnectorTextSentence : LLMConnector
{
    [Serializable]
    private class TextSentenceResponse
    {
        public string sentence;
    }

    [SerializeField, TextArea(3, 10)] private string _promptTemplate = "Analiza el siguiente caso y dicta un texto con la sentencia:\r\n\r\n\r\nDemanda de !:\r\n#\r\n\r\n\r\nPruebas presentadas por !:\r\n@\r\n\r\n\r\nPruebas presentadas por ¡:\r\n$\r\n\r\nSentencia del tribunal: \r\n%\r\n\r\n\r\nSegún la sentencia del tribunal, genera un texto como si fueses el juez dictando la sentencia y explicando el porqué de ella. Rellena el string \"sentence\" con tu respuesta.";
    [SerializeField, TextArea(3, 10)] private string _winPlayerText = "El jurado ha dictado que el player, el demandante, gana el juicio, y por tanto, se aceptan las peticiones de su demanda.\r\nEs importante que menciones en caso la sección de \"petición\" de la demanda, y considerar las peticiones y las costas de dinero al decir tu texto de resolución.";
    [SerializeField, TextArea(3, 10)] private string _losePlayerText = "El jurado ha dictado que el player, el demandante, pierde el juicio, y por tanto, se desestima su demanda, y tiene que compensar economicamente a la parte demandada por las molestias.";

    private Action<string> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<string> responseCallback, bool playerWin)
    {
        _responseCallback = responseCallback;

        string lawsuitText = GameSystem.Instance.CaseData.lawsuitText;
        string clientDocs = "";
        string rivalDocs = "";

        string resolutionText = playerWin ? _winPlayerText : _losePlayerText;

        List<Document> clientFinalDocuments = GameSystem.Instance.CaseData.finalPlayerDocuments;
        List<Document> rivalFinalDocuments = GameSystem.Instance.CaseData.finalRivalDocuments;

        for (int i = 0; i < clientFinalDocuments.Count; i++)
        {
            clientDocs += (i + ": " + clientFinalDocuments[i].GetDocName() + ": " + clientFinalDocuments[i].GetContent() + "\n");
        }

        for (int i = 0; i < rivalFinalDocuments.Count; i++)
        {
            rivalDocs += (i + ": " + rivalFinalDocuments[i].GetDocName() + ": " + rivalFinalDocuments[i].GetContent() + "\n");
        }

        _prompt = _promptTemplate.Replace("#", lawsuitText);
        _prompt = _prompt.Replace("@", clientDocs);
        _prompt = _prompt.Replace("$", rivalDocs);
        _prompt = _prompt.Replace("%", resolutionText);
        _prompt = _prompt.Replace("!", GameSystem.Instance.CaseData.clientName);
        _prompt = _prompt.Replace("¡", GameSystem.Instance.CaseData.demandedEntityName);

        sendPrompt(recieveFinalResponse, _prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        TextSentenceResponse jsonResponse = JsonUtility.FromJson<TextSentenceResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.sentence);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("sentence", new PropertyInfo(JsonDataType.String));
    }
}
