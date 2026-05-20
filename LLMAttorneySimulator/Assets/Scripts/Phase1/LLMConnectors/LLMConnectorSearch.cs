using System;
using UnityEngine;

/// <summary>
/// LLMConnector enfocado en la busqueda y envio de los precios de servicios o productos de un proceso civil enfocando la busqueda en el archivo RAG de 
/// precios
/// </summary>
public class LLMConnectorSearch : LLMConnector
{
    [Serializable]
    private class SearchResponse
    {
        public string answer;
    }

    private Action<string> _responseCallback;

    /// <summary>
    /// Metodo publico para iniciar la llamada al LLM con los parametros y configuracion especificados
    /// </summary>
    /// <param name="responseCallback">Metodo que debe llamarse una vez terminado el envio y recibida la contestación del LLM</param>
    /// <param name="errorCallback">Metodo que debe llamarse si el envio del prompt al LLM es fallido debido a un problema del servidor</param>
    /// <param name="prompt">Prompt escrito por el usuario que se desea enviar al LLM</param>
    public void SendPrompt(Action<string> responseCallback, Action<string> errorCallback, string prompt)
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
        SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(finalSerializedResponse);
     
        _responseCallback?.Invoke(jsonResponse.answer);
    }

    /// <summary>
    /// Metodo que recibe las distintas respuestas de los steps, revisando si la respuesta contiene algun texto de no informacion para devolver 
    /// directamente que no se tiene la informacion
    /// </summary>
    /// <param name="success"> Indica si la respuesta a sido devuelta del servidor sin ningun error o problema</param>
    /// <param name="text"> Atributos rellenados por el LLM</param>
    protected override void recieveStepResponse(bool success, string text)
    {
        if (text.Contains("Sin información"))
        {
            recieveFinalResponse("Información no disponible. Especifique mejor la petición.");
        }
        else
        {
            base.recieveStepResponse(success, text);
        }        
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(serializedResponse);
        return jsonResponse.answer;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        SearchResponse jsonResponse = JsonUtility.FromJson<SearchResponse>(serializedResponse);
        return jsonResponse.answer;
    }

    /// <summary>
    /// Creacion de los esquemas especificos para este conector
    /// </summary>
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
    }

}
