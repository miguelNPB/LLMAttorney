using System;
using UnityEngine;

/// <summary>
/// LLMConnector enfocado en justificar la decisión que ha tomado el cliente o rival respecto a la aceptación o rechazo de la propuesta de conciliación
/// </summary>
public class LLMConnectorConciliationAgreeText : LLMConnector
{

    [Serializable]
    private class LLMConciliationResponseText
    {
        public string answer;
    }

    //Introduccion a los contextos segun cada caso posible caso para poder reutilizar el mismo config
    [SerializeField] private string _clientAgreeReactionContext = "Dado que ACEPTAS, muestra pragmatismo, alivio por evitar el juicio, o resignación si crees que es 'un mal menor'.";
    [SerializeField] private string _clientDisagreeReactionContext = "Dado que RECHAZAS, muestra que los números no te cuadran, indignación ante una oferta ridícula, y debes pedirle a el abogado que reconsidere y te ofrezca otra oferta de conciliacion.";
    [SerializeField] private string _rivalAgreeReactionContext = "Dado que ACEPTAS, muestra pragmatismo financiero, voluntad de cerrar el conflicto de una vez por todas para ahorrarte costes mayores, y un tono de 'pago esto y nos olvidamos del tema'.";
    [SerializeField] private string _rivalDisagreeReactionContext = "Dado que RECHAZAS, muestra firmeza, hazle ver que su cliente pide una barbaridad, que el descuento no te compensa el riesgo, y que prefieres que decida el juez antes que ceder a esa oferta.";

    private Action<string> _responseCallback;

    /// <summary>
    /// Metodo publico para iniciar la llamada al LLM con los parametros y configuracion especificados
    /// </summary>
    /// <param name="responseCallback">Metodo que debe llamarse una vez terminado el envio y recibida la contestación del LLM</param>
    /// <param name="errorCallback">Metodo que debe llamarse si el envio del prompt al LLM es fallido debido a un problema del servidor</param>
    /// <param name="prompt">Prompt escrito por el usuario que se desea enviar al LLM</param>
    /// <param name="isPlayer">Configuración concreta que se debe usar para este envio, en este caso si la revision la hace el cliente o 
    /// el rival</param>
    public void SendPrompt(Action<string> responseCallback, Action<string> errorCallback, string prompt, bool agree, bool isPlayer)
    {
        _responseCallback = responseCallback;

        if (isPlayer) {
            SetupConfigClientAnswer(agree);
        }
        else {
            SetupConfigRivalAnswer(agree);
        }

        sendPrompt(recieveFinalResponse, errorCallback, prompt, isPlayer ? 0 : 1);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse">Texto en formato json devuelto por el servidor que cuenta con los atributos rellenados por el LLM</param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        LLMConciliationResponseText jsonResponse = JsonUtility.FromJson<LLMConciliationResponseText>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.answer);
    }

    /// <summary>
    /// Genera el texto de configuracion para el prompt de texto del cliente
    /// </summary>
    /// <param name="agree">Indica si la propuesta a sido aceptada o no para escoger la introducción de config</param>
    private void SetupConfigClientAnswer(bool agree)
    {
        string decision = agree ? "ACEPTAR" : "RECHAZAR";
        string reaction = agree ? _clientAgreeReactionContext : _clientDisagreeReactionContext;

        string config = _llmConfigs[0].GetContext();
        config = config.Replace("{decision}", decision);
        config = config.Replace("{reaction}", reaction);

        overrideLLMContext(config);
    }

    /// <summary>
    /// Genera el texto de configuracion para el prompt de texto del rival
    /// </summary>
    /// <param name="agree">Indica si la propuesta a sido aceptada o no para escoger la introducción de config</param>
    private void SetupConfigRivalAnswer(bool agree)
    {
        string decision = agree ? "ACEPTAR" : "RECHAZAR";
        string reaction = agree ? _rivalAgreeReactionContext : _rivalDisagreeReactionContext;
       
        string config = _llmConfigs[1].GetContext();
        config = config.Replace("{decision}", decision);
        config = config.Replace("{reaction}", reaction);

        overrideLLMContext(config);
    }

    /// <summary>
    /// Creacion de los esquemas especificos para este conector
    /// </summary>
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
    }

    private void Start()
    {
        appendHistoricText(GameSystem.Instance.CaseData.caseDescription);
    }
}
