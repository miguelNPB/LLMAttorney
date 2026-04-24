using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LLMConnectorConciliation : LLMConector
{
    private enum CurrentPromptType { Cliente, RivalNormal, RivalRechazar };

    [Serializable]
    private class LLMConciliationResponseBool
    {
        public bool agree;
    }

    [Serializable]
    private class LLMConciliationResponseText
    {
        public string answer;
    }



    [SerializeField] private ConciliationPage conciliacionPage;

    private JsonSchema _boolSchema;
    private JsonSchema _stringSchema;

    private string _clientTextBasePrompt;
    private string _rivalTextBasePrompt;

    private string _answer = "";
    private bool _agree = false;
    private bool _agreeBoolRecieved = false;

    private CurrentPromptType currentPromptType;



    /// <summary>
    /// Genera el texto de configuracion para el prompt de texto del cliente
    /// </summary>
    /// <param name="agree"></param>
    /// <returns></returns>
    private string GetTextPromptClientAnswer(bool agree)
    {
        string decision = agree ? "ACEPTAR" : "RECHAZAR";
        string reaction = agree
            ? "Dado que ACEPTAS, muestra pragmatismo, alivio por evitar el juicio, o resignación si crees que es 'un mal menor'."
            : "Dado que RECHAZAS, muestra que los números no te cuadran, indignación ante una oferta ridícula, o determinación para ir a juicio y pelear por lo tuyo.";

        string nextPrompt = _clientTextBasePrompt.Replace("{decision}", decision);
        nextPrompt = nextPrompt.Replace("{reaction}", reaction);

        return nextPrompt;
    }

    /// <summary>
    /// Genera el texto de configuracion para el prompt de texto del rival
    /// </summary>
    /// <param name="agree"></param>
    /// <returns></returns>
    private string GetTextPromptRivalAnswer(bool agree)
    {
        string decision = agree ? "ACEPTAR" : "RECHAZAR";
        string reaction = agree
            ? "Dado que ACEPTAS, muestra pragmatismo financiero, voluntad de cerrar el conflicto de una vez por todas para ahorrarte costes mayores, y un tono de 'pago esto y nos olvidamos del tema'."
            : "Dado que RECHAZAS, muestra firmeza, hazle ver que su cliente pide una barbaridad, que el descuento no te compensa el riesgo, y que prefieres que decida el juez antes que ceder a esa oferta.";

        string nextPrompt = _rivalTextBasePrompt.Replace("{decision}", decision);
        nextPrompt = nextPrompt.Replace("{reaction}", reaction);

        return nextPrompt;
    }


    protected override void receiveResponse(bool success, string answer)
    {
        if (success)
        {
            switch (currentPromptType)
            {
                case CurrentPromptType.Cliente:
                    if (!_agreeBoolRecieved)
                    {
                        _agree = JsonUtility.FromJson<LLMConciliationResponseBool>(answer).agree;
                        _agreeBoolRecieved = true;
                    }
                    else
                    {
                        _answer = JsonUtility.FromJson<LLMConciliationResponseText>(answer).answer;
                    }
                    break;
                case CurrentPromptType.RivalNormal:
                    if (!_agreeBoolRecieved)
                    {
                        _agree = JsonUtility.FromJson<LLMConciliationResponseBool>(answer).agree;
                        _agreeBoolRecieved = true;
                    }
                    else
                    {
                        _answer = JsonUtility.FromJson<LLMConciliationResponseText>(answer).answer;
                    }
                    break;
                case CurrentPromptType.RivalRechazar:
                    _answer = JsonUtility.FromJson<LLMConciliationResponseText>(answer).answer;
                    break;
            }
            _promptSent = false;
        }
    }

    /// <summary>
    /// Coroutina para generar el bool si el cliente acepta o no, y luego un texto para justificarlo
    /// </summary>
    /// <returns></returns>
    public IEnumerator SendClientPrompt()
    {
        currentPromptType = CurrentPromptType.Cliente;
        _agreeBoolRecieved = false;

        // sacar el booleano true o false
        _contextSchema = _boolSchema;
        sendContextPrompt(inputFieldText, 0);
        _promptSent = true;

        while (_promptSent)
            yield return null;

        Debug.Log("Client agree: " + _agree);

        // mandar prompt  de texto
        _contextSchema = _stringSchema;
        _config[2].context = GetTextPromptClientAnswer(_agree);
        sendContextPrompt(inputFieldText,2);
        _promptSent = true;
        while (_promptSent)
            yield return null;



        Debug.Log("Client answer: " + _answer);
        conciliacionPage.SetClientAgrees(_agree, _answer);

        yield return null;
    }

    /// <summary>
    /// Coroutina para generar el bool si el rival acepta o no, y luego un texto para justificarlo
    /// </summary>
    /// <returns></returns>
    public IEnumerator SendRivalPromptNormal()
    {
        currentPromptType = CurrentPromptType.RivalNormal;
        _agreeBoolRecieved = false;

        // mandar prompt para sacar el booleano true o false
        _contextSchema = _boolSchema;
        sendContextPrompt(inputFieldText, 1);
        _promptSent = true;

        while (_promptSent)
            yield return null;

        Debug.Log("Rival agree: " + _agree);

        _contextSchema = _stringSchema;
        _config[3].context = GetTextPromptRivalAnswer(_agree);
        // mandar prompt  de texto
        sendContextPrompt(inputFieldText, 3);
        _promptSent = true;

        while (_promptSent)
            yield return null;


        Debug.Log("Rival answer: " + _answer);
        conciliacionPage.SetRivalAgrees(_agree, _answer);

        yield return null;
    }


    /// <summary>
    /// Prompt para generar el texto del rival con una rejection confirmada
    /// </summary>
    /// <returns></returns>
    public IEnumerator SendRivalPromptRejectionConfirmed()
    {
        currentPromptType = CurrentPromptType.RivalRechazar;
        _agree = false;
        sendContextPrompt(3);

        _promptSent = true;

        while (_promptSent)
            yield return null;
        _promptSent = false;

        conciliacionPage.SetRivalAgrees(_agree, _answer);

        yield return null;
    }

    protected override void createJsonSchemas()
    {
        _boolSchema = new JsonSchema();
        _boolSchema.properties.Add("agree", new PropertyInfo(JsonDataType.Boolean));

        _stringSchema = new JsonSchema();
        _stringSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    private void Start()
    {
        _historical.Add(GameSystem.Instance.CaseData.caseDescription);

        _clientTextBasePrompt = _config[2].context;
        _rivalTextBasePrompt = _config[3].context;
    }

    private void Awake()
    {
        createJsonSchemas();

    }
}
