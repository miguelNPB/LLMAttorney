using System;
using UnityEngine;

/// <summary>
/// LLMConnector enfocado en escribir la respuesta del cliente dado el mensaje escrito por el usuario. Además se revisa la validez de la respuesta para
/// poder agilizar el proceso del conector
/// </summary>
public class LLMConnectorClientMeeting : LLMConnector
{

    [Serializable]
    private class ClientMeetingCheckerResponse
    {
        public string answer;
        public bool validResponse;
    }
    private Action<string, bool> _responseCallback;

    /// <summary>
    /// Metodo publico para iniciar la llamada al LLM con los parametros y configuracion especificados
    /// </summary>
    /// <param name="responseCallback">Metodo que debe llamarse una vez terminado el envio y recibida la contestación del LLM</param>
    /// <param name="errorCallback">Metodo que debe llamarse si el envio del prompt al LLM es fallido debido a un problema del servidor</param>
    /// <param name="prompt">Prompt escrito por el usuario que se desea enviar al LLM</param>
    public void SendPrompt(Action<string, bool> responseCallback, Action<string> errorCallback, string prompt)
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
        ClientMeetingCheckerResponse jsonResponse = JsonUtility.FromJson<ClientMeetingCheckerResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.answer, jsonResponse.validResponse);
    }

    /// <summary>
    /// Creacion de los esquemas especificos para este conector
    /// </summary>
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("validResponse", new PropertyInfo(JsonDataType.Boolean));
        
        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("validResponse", new PropertyInfo(JsonDataType.Boolean));
    }

    /// <summary>
    /// Metodo que recibe la primera respuesta generada por el LLM. Este revisa si la respuesta ya es valida para saltarse el paso de los steps.
    /// </summary>
    /// <param name="success"> Indica si la respuesta a sido devuelta del servidor sin ningun error o problema</param>
    /// <param name="text"> Atributos rellenados por el LLM</param>
    protected override void recieveFirstResponse(bool success, string text)
    {

        if (_useSteps && _llmConfigs[_configIndex].GetStepChecks().Length > 0)
        {
            ClientMeetingCheckerResponse jsonResponse = JsonUtility.FromJson<ClientMeetingCheckerResponse>(text);
            _stepCounter = 0;

            if (!jsonResponse.validResponse)
            {
                sendStepPrompt(_prompt);
            }        
            else
            {
                respondPrompt(success, text);
            }      
        }
        else
        {
            respondPrompt(success, text);
        }
            
    }

    /// <summary>
    /// Metodo que recibe las distintas respuestas de los steps, revisando si ya se considera el mensaje valido para sacar la respuesta final
    /// </summary>
    /// <param name="success"> Indica si la respuesta a sido devuelta del servidor sin ningun error o problema</param>
    /// <param name="text"> Atributos rellenados por el LLM</param>
    protected override void recieveStepResponse(bool success, string text)
    {

        ClientMeetingCheckerResponse jsonResponse = JsonUtility.FromJson<ClientMeetingCheckerResponse>(text);

        if (jsonResponse.validResponse || !sendStepPrompt(_prompt))
        {
            respondPrompt(success, text);
        }
            
    }
}
