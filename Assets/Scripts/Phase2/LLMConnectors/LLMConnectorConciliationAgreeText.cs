using System;
using System.Collections;
using Telemetry;
using UnityEngine;

/// <summary>
/// LLMConnector para el texto de acuerdo o no acuerdo con la propuesta de conciliacion
/// </summary>
public class LLMConnectorConciliationAgreeText : LLMConnector
{

    [Serializable]
    private class LLMConciliationResponseText
    {
        public string answer;
    }

    [SerializeField] private string _clientAgreeReactionContext = "Dado que ACEPTAS, muestra pragmatismo, alivio por evitar el juicio, o resignación si crees que es 'un mal menor'.";
    [SerializeField] private string _clientDisagreeReactionContext = "Dado que RECHAZAS, muestra que los números no te cuadran, indignación ante una oferta ridícula, y debes pedirle a el abogado que reconsidere y te ofrezca otra oferta de conciliacion.";
    [SerializeField] private string _rivalAgreeReactionContext = "Dado que ACEPTAS, muestra pragmatismo financiero, voluntad de cerrar el conflicto de una vez por todas para ahorrarte costes mayores, y un tono de 'pago esto y nos olvidamos del tema'.";
    [SerializeField] private string _rivalDisagreeReactionContext = "Dado que RECHAZAS, muestra firmeza, hazle ver que su cliente pide una barbaridad, que el descuento no te compensa el riesgo, y que prefieres que decida el juez antes que ceder a esa oferta.";

    private string _baseClientConfig;
    private string _baseRivalConfig;

    private Action<string> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
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
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        LLMConciliationResponseText jsonResponse = JsonUtility.FromJson<LLMConciliationResponseText>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.answer);
    }

    /// <summary>
    /// Genera el texto de configuracion para el prompt de texto del cliente
    /// </summary>
    /// <param name="agree"></param>
    /// <returns></returns>
    private void SetupConfigClientAnswer(bool agree)
    {
        _llmConfigs[0].OverrideContext(_baseClientConfig);

        string decision = agree ? "ACEPTAR" : "RECHAZAR";
        string reaction = agree ? _clientAgreeReactionContext : _clientDisagreeReactionContext;

        string config = _llmConfigs[0].GetContext();
        config = config.Replace("{decision}", decision);
        config = config.Replace("{reaction}", reaction);

        _llmConfigs[0].OverrideContext(config);
    }

    /// <summary>
    /// Genera el texto de configuracion para el prompt de texto del rival
    /// </summary>
    /// <param name="agree"></param>
    /// <returns></returns>
    private void SetupConfigRivalAnswer(bool agree)
    {
        _llmConfigs[1].OverrideContext(_baseRivalConfig);

        string decision = agree ? "ACEPTAR" : "RECHAZAR";
        string reaction = agree ? _rivalAgreeReactionContext : _rivalDisagreeReactionContext;
       
        string config = _llmConfigs[1].GetContext();
        config = config.Replace("{decision}", decision);
        config = config.Replace("{reaction}", reaction);

        _llmConfigs[1].OverrideContext(config);
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
    }

    private void Start()
    {
        _baseClientConfig = _llmConfigs[0].GetContext();
        _baseRivalConfig = _llmConfigs[1].GetContext();

        _llmConfigs[0].AddHistoric(GameSystem.Instance.CaseData.caseDescription);
        _llmConfigs[1].AddHistoric(GameSystem.Instance.CaseData.caseDescription);
    }
}
