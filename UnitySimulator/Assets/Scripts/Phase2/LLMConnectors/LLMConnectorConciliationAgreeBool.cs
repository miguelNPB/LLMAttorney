using System;
using UnityEngine;

/// <summary>
/// LLMConnector enfocado en la revisión del acuerdo de conciliacion proporcionado por el usuario con el objetivo de dado un contexto y una 
/// configuracion escoger entre aceptarlo o no.
/// </summary>
public class LLMConnectorConciliationAgreeBool : LLMConnector
{
    [Serializable]
    private class LLMConciliationResponseText
    {
        public bool agree;
    }

    private Action<bool> _responseCallback;

    /// <summary>
    /// Metodo publico para iniciar la llamada al LLM con los parametros y configuracion especificados
    /// </summary>
    /// <param name="responseCallback">Metodo que debe llamarse una vez terminado el envio y recibida la contestación del LLM</param>
    /// <param name="errorCallback">Metodo que debe llamarse si el envio del prompt al LLM es fallido debido a un problema del servidor</param>
    /// <param name="prompt">Prompt escrito por el usuario que se desea enviar al LLM</param>
    /// <param name="isPlayer">Configuración concreta que se debe usar para este envio, en este caso si la revision la hace el cliente o 
    /// el rival</param>
    public void SendPrompt(Action<bool> responseCallback, Action<string> errorCallback, string prompt, bool isPlayer)
    {
        _responseCallback = responseCallback;

        sendPrompt(recieveFinalResponse, errorCallback, prompt, isPlayer ? 0 : 1);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse">Texto en formato json devuelto por el servidor que cuenta con los atributos rellenados por el LLM</param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        LLMConciliationResponseText jsonResponse = JsonUtility.FromJson<LLMConciliationResponseText>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.agree);
    }

    /// <summary>
    /// Creacion de los esquemas especificos para este conector
    /// </summary>
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("agree", new PropertyInfo(JsonDataType.Boolean));
    }
}
