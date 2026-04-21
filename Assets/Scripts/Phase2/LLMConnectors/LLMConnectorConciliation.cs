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

    private string answer = "";
    private bool agree = false;
    private string prompt = "";

    private CurrentPromptType currentPromptType;

    private string GetTextPromptClientAnswer(bool agree)
    {
        string decision = agree ? "ACEPTAR" : "RECHAZAR";
        string reaccion = agree
            ? "Dado que ACEPTAS, muestra pragmatismo, alivio por evitar el juicio, o resignación si crees que es 'un mal menor'."
            : "Dado que RECHAZAS, muestra que los números no te cuadran, indignación ante una oferta ridícula, o determinación para ir a juicio y pelear por lo tuyo.";

        string nextPrompt = $@"
                        Actúa como mi cliente en un caso judicial en España. Yo soy tu abogado (el jugador). 
                        Me dirijo a ti para presentarte una propuesta de conciliación de la parte contraria, 
                        un paso que a menudo es obligatorio intentar antes de ir a juicio en nuestra jurisdicción.

                        Basándote en los antecedentes de tu caso (en el apartado 'Historico') y en la oferta que 
                        te acabo de transmitir (el prompt), has tomado una decisión irrevocable: 
                        has decidido {decision} la oferta.

                        Tu tarea es escribirme un mensaje de respuesta justificando esta decisión.

                        Reglas para tu respuesta:
                        1. Háblame en primera persona y dirígete a mí directamente como tu abogado (ej. 'He leído lo que me mandas...', 'Mira, abogado...').
                        2. Tono realista: Eres el cliente, no el abogado. No uses jerga procesal compleja. Habla de tu dinero, tu tiempo, tu tranquilidad o tu sentido de la justicia.
                        3. Reacción emocional coherente: {reaccion}
                        4. Fundamenta tu decisión mencionando detalles específicos del 'Histórico' y contrastándolos con la 'Propuesta'.
                        5. Limítate a darme tu justificación, no me pidas que haga trámites adicionales, la decisión ya está tomada.";

        return nextPrompt;
    }
    private string GetTextPromptRivalAnswer(bool agree)
    {
        string decision = agree ? "ACEPTAR" : "RECHAZAR";
        string reaccion = agree
            ? "Dado que ACEPTAS, muestra pragmatismo financiero, voluntad de cerrar el conflicto de una vez por todas para ahorrarte costes mayores, y un tono de 'pago esto y nos olvidamos del tema'."
            : "Dado que RECHAZAS, muestra firmeza, hazle ver que su cliente pide una barbaridad, que el descuento no te compensa el riesgo, y que prefieres que decida el juez antes que ceder a esa oferta.";

        string nextPrompt = $@"
                        Actúa como la parte demandada (el rival) en un caso judicial en España. Yo soy el abogado de la parte contraria (el jugador), es decir, represento a la persona o empresa que te ha demandado.
                        Te acabo de hacer llegar una propuesta de conciliación para intentar evitar el juicio.

                        Basándote en los antecedentes del caso (en el apartado 'Historico') y en la oferta que 
                        te acabo de plantear, has tomado una decisión irrevocable: 
                        has decidido {decision} mi propuesta.

                        Tu tarea es escribirme un mensaje de respuesta justificando esta decisión.

                        Reglas para tu respuesta:
                        1. Háblame en primera persona y dirígete a mí directamente como el abogado contrario (ej. 'Estimado abogado de la parte demandante...', 'Mire, letrado...').
                        2. Tono realista: Eres el demandado (una persona o empresa), no un abogado. No uses jerga procesal compleja. Habla desde tu perspectiva sobre lo que te ahorras, lo que te costaría el juicio, o por qué crees que la cifra es justa o absurda.
                        3. Reacción emocional coherente: {reaccion}
                        4. Fundamenta tu decisión mencionando detalles específicos del 'Histórico' y contrastándolos con la 'Propuesta' que te he hecho.
                        5. Limítate a darme tu respuesta definitiva y directa, no dejes la puerta abierta a más negociaciones. La decisión ya está tomada.";

        return nextPrompt;
    }


    protected override void recieveResponse(bool success, string answer)
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

                    nextPrompt = inputField.text;
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
        recieveResponse(true, "empty");

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
    }

    private void Awake()
    {
        createJsonSchemas();

    }
}
