using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LLMConnectorConciliation : LLMConector
{
    private enum CurrentPromptType { Cliente, RivalNormal, RivalRechazar };

    [Serializable]
    private class LLMConciliationResponseContext
    {
        public bool agree;
    }

    [Serializable]
    private class LLMConciliationResponseSteps
    {
        public string answer;
    }



    [SerializeField] private ConciliationPage conciliacionPage;

    private string _clientBooleanBasePrompt;
    private string _rivalBooleanBasePrompt;
    private string _clientTextBasePrompt;
    private string _rivalTextBasePrompt;

    private string answer = "";
    private bool agree = false;
    private string prompt = "";

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
            string nextPrompt = "";

            // deserializamos la respuesta
            if (currentPromptType != CurrentPromptType.RivalRechazar)
            {
                if (_stepCounter == 0)
                {
                    agree = JsonUtility.FromJson<LLMConciliationResponseContext>(answer).agree;

                    nextPrompt = inputFieldText;
                }
                else
                {
                    nextPrompt = JsonUtility.FromJson<LLMConciliationResponseSteps>(answer).answer;
                }
            }
            

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
            {
                if (_stepCounter == 0)
                {
                    // respuesta del cliente
                    if (currentPromptType == CurrentPromptType.Cliente)
                    {
                        nextPrompt = GetTextPromptClientAnswer(agree);
                    }
                    else if (currentPromptType == CurrentPromptType.RivalNormal)
                    {
                        nextPrompt = GetTextPromptRivalAnswer(agree);
                    } 
                    else if (currentPromptType == CurrentPromptType.RivalRechazar)
                    {
                        Debug.Log("RECHAZARINSTANT");
                        nextPrompt = GetTextPromptRivalAnswer(false);
                    }

                    nextPrompt += _historical[0];
                }
                    

                sendSecuritySteps(nextPrompt);
            }
            else
            {

                if (currentPromptType == CurrentPromptType.RivalRechazar)
                    nextPrompt = JsonUtility.FromJson<LLMConciliationResponseSteps>(answer).answer;

                this.answer = nextPrompt;

                _stepCounter = 0;
                _promptSent = false;
            }
        }
        else
        {
            _promptSent = false;
            Debug.LogError("Error en la llamada al LLM: " + answer);
        }
    }


    public IEnumerator SendClientPrompt()
    {
        sendContextPrompt(0);
        currentPromptType = CurrentPromptType.Cliente;
        _promptSent = true;


        while (_promptSent)
            yield return null;
        _promptSent = false;


        Debug.Log("Client answer: " + answer);
        conciliacionPage.SetClientAgrees(agree, answer);

        yield return null;
    }

    public IEnumerator SendRivalPromptNormal()
    {
        sendContextPrompt(1);
        currentPromptType = CurrentPromptType.RivalNormal;
        _promptSent = true;

        while (_promptSent)
            yield return null;
        _promptSent = false;


        conciliacionPage.SetRivalAgrees(agree, answer);


        yield return null;
    }

    public IEnumerator SendRivalPromptRejectionConfirmed()
    {
        currentPromptType = CurrentPromptType.RivalRechazar;
        agree = false;
        receiveResponse(true, "empty");

        _promptSent = true;

        while (_promptSent)
            yield return null;
        _promptSent = false;

        conciliacionPage.SetRivalAgrees(agree, answer);


        yield return null;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("agree", new PropertyInfo(JsonDataType.Boolean));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    private void Start()
    {
        _historical.Add(GameSystem.Instance.CaseData.caseDescription);

        _clientBooleanBasePrompt = _config[0].context;
        _rivalBooleanBasePrompt = _config[1].context;
        _clientTextBasePrompt = _config[2].context;
        _rivalTextBasePrompt = _config[3].context;
    }

    private void Awake()
    {
        createJsonSchemas();

    }
}
