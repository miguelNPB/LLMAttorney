using System;
using Telemetry;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LLMConectorPreClientMeeting : LLMConector
{
    [SerializeField]
    private UIClientMeetingManager _uiMeeting;

    [SerializeField]
    private GameObject _buttonContinue;

    [SerializeField]
    private LLMConectorClientMeeting _clientMeetingConector;

    [SerializeField]
    private BudgetManager _budgetManager;

    private string _text;
    private int _messageID = -1;

    [Serializable]
    private class PreMeetingResponse
    {
        public bool pregunta_coherente;
        public bool presupuesto_adecuado;
        public int dinero_presupuestado;
    }

    protected override void receiveResponse(bool success, string answer)
    {
        if (success)
        {
            // deserializamos la respuesta
            PreMeetingResponse jsonResponse = JsonUtility.FromJson<PreMeetingResponse>(answer);         

            if (jsonResponse.pregunta_coherente)
            {
                if (jsonResponse.presupuesto_adecuado)
                {

                    string log =
                    $"[Fase: {SceneManager.GetActiveScene().buildIndex}] [Envio: {_messageID}].\n\n" +
                    $"Pregunta coherente: {jsonResponse.pregunta_coherente}\n" +
                    $"Presupuesto adecuado: {jsonResponse.presupuesto_adecuado}\n" +
                    $"Dinero presupuestado: {jsonResponse.dinero_presupuestado}";

                    LLMLogManager.Instance.LogMessageSent(log, _messageID);

                    _budgetManager.SetBudgetFromLLM(_text, jsonResponse.dinero_presupuestado);
                    _clientMeetingConector.CallSendContext(_text, _messageID);

                }
                else
                {
                    TelemetryDispatch.SendDeniedBudget(_messageID, jsonResponse.dinero_presupuestado);
                    _uiMeeting.EndPendingMessage("Me cuesta creer que valga eso. Por favor, se más realista con el precio");
                    _buttonContinue.SetActive(true);           
                }
            }
            else
            {
                _uiMeeting.EndPendingMessage("Perdona pero ¿Podriamos centrarnos en mi caso?");
                _buttonContinue.SetActive(true);
            }

            _promptSent = false;

        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _uiMeeting.EndPendingMessage("Error al contactar con el modelo.");
        }

    }

    public void CallSendContext(int indexConfig = 0)
    {
        _messageID = LLMLogManager.Instance.getNumMessageSent();
        LLMLogManager.Instance.addMessageSent();
        TelemetryDispatch.SendQueryPost(_messageID);

        sendContextPrompt(indexConfig);
    }

    protected override bool sendContextPrompt(int indexConfig = 0)
    {
        _uiMeeting.StartPendingMessage();

        _buttonContinue.SetActive(false);

        _text = inputFieldText;

        bool messageSent = base.sendContextPrompt(indexConfig);

        if (!messageSent)
        {
            _uiMeeting.EndPendingMessage("¿Puedes volver a repetirmelo por favor?");
        }

        return messageSent;
    }

    protected override bool sendSecuritySteps(string prompt)
    {
        return false;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("pregunta_coherente", new PropertyInfo(JsonDataType.Boolean));
        _contextSchema.properties.Add("presupuesto_adecuado", new PropertyInfo(JsonDataType.Boolean));
        _contextSchema.properties.Add("dinero_presupuestado", new PropertyInfo(JsonDataType.Integer));

        _schemasCreated = true;
    }

    private void Awake()
    {
        createJsonSchemas();
    }
}
