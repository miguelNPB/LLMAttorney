using System;
using UnityEngine;

/// <summary>
/// LLMConnector enfocado en la resvisión del presupuesto dado por el usuario, comprobando que no supera un umbral sobre el precio real y que 
/// devuelve una media del presupuesto dado por el usuario.
/// </summary>
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
    /// Metodo publico para iniciar la llamada al LLM con los parametros y configuracion especificados
    /// </summary>
    /// <param name="responseCallback">Metodo que debe llamarse una vez terminado el envio y recibida la contestación del LLM</param>
    /// <param name="errorCallback">Metodo que debe llamarse si el envio del prompt al LLM es fallido debido a un problema del servidor</param>
    /// <param name="prompt">Prompt escrito por el usuario que se desea enviar al LLM</param>
    public void SendPrompt(Action<bool, float> responseCallback, Action<string> errorCallback, string prompt)
    {
        _responseCallback = responseCallback;
        sendPrompt(recieveFinalResponse, errorCallback, prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse">Texto en formato json devuelto por el servidor que cuenta con los atributos rellenados por el LLM</param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        BudgetCheckerResponse jsonResponse = JsonUtility.FromJson<BudgetCheckerResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.budgetCoherent, jsonResponse.budget);
    }

    /// <summary>
    /// Creacion de los esquemas especificos para este conector
    /// </summary>
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("budgetCoherent", new PropertyInfo(JsonDataType.Boolean));
        _contextSchema.properties.Add("budget", new PropertyInfo(JsonDataType.Float));

        _stepsSchema = new JsonSchema();
    }

}
