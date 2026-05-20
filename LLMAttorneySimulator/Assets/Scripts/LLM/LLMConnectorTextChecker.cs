using System;
using UnityEngine;

/// <summary>
/// LLMConnector enfocado en la resvisión de la coherencia según el contexto o el formato especifico
/// </summary>
public class LLMConnectorTextChecker : LLMConnector
{
    [Serializable]
    private class TextCheckerResponse
    {
        public bool isCoherent;
    }
    private Action<bool> _responseCallback;

    /// <summary>
    /// Metodo publico para iniciar la llamada al LLM con los parametros y configuracion especificados
    /// </summary>
    /// <param name="responseCallback">Metodo que debe llamarse una vez terminado el envio y recibida la contestación del LLM</param>
    /// <param name="errorCallback">Metodo que debe llamarse si el envio del prompt al LLM es fallido debido a un problema del servidor</param>
    /// <param name="prompt">Prompt escrito por el usuario que se desea enviar al LLM</param>
    /// <param name="indexConfig">Configuración concreta que se debe usar para este envio</param>
    public void SendPrompt(Action<bool> responseCallback, Action<string> errorCallback, string prompt, int indexConfig)
    {
        _responseCallback = responseCallback;
        sendPrompt(recieveFinalResponse, errorCallback, prompt, indexConfig);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse">Texto en formato json devuelto por el servidor que cuenta con los atributos rellenados por el LLM</param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        TextCheckerResponse jsonResponse = JsonUtility.FromJson<TextCheckerResponse>(finalSerializedResponse);

        if (!jsonResponse.isCoherent)
        {
            Telemetry.TelemetryDispatch.SendNotConsistentAnswer(_messageID);
        }

        _responseCallback?.Invoke(jsonResponse.isCoherent);
    }

    /// <summary>
    /// Creacion de los esquemas especificos para este conector
    /// </summary>
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("isCoherent", new PropertyInfo(JsonDataType.Boolean));
    }
}
