using System;
using System.Collections;
using System.Collections.Generic;
using Telemetry;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LLMConectorClientMeeting : LLMConector
{
    [SerializeField]
    private UIClientMeetingManager _uiMeeting;

    [SerializeField]
    private GameObject _buttonContinue;

    [Serializable]
    private class MeetingResponse
    {
        public string answer;
        public bool respuesta_valida;
        public bool respuesta_coherente;
        public bool contratar_abogado;
    }

    private bool _abogadoContratado = false;

    private int _messageID = -1;

   
    protected override void receiveResponse(bool success, string answer)
    {
        if (success)
        {
            // deserializamos la respuesta
            MeetingResponse jsonResponse = JsonUtility.FromJson<MeetingResponse>(answer);

            if (_stepCounter == 0)
            {
                _abogadoContratado = jsonResponse.contratar_abogado;
                _messageID = LLMLogManager.Instance.getNumMessageSent();
                LLMLogManager.Instance.addMessageSent();
            }

            if (jsonResponse.respuesta_valida && jsonResponse.respuesta_coherente)
            {
                _uiMeeting.EndPendingMessage(jsonResponse.answer);
            }
            else if (!jsonResponse.respuesta_coherente)
            {
                _uiMeeting.EndPendingMessage("Perdona pero ¿Podriamos centrarnos en mi caso?");
            }
            else
            {
                _uiMeeting.EndPendingMessage("No te he entendido bien, puedes repetirlo");
            }
                       

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length && _useSecuritySteps &&
                (!jsonResponse.respuesta_valida || !jsonResponse.respuesta_coherente))
            {
                sendSecuritySteps(jsonResponse.answer);
            }
            else
            {

                string log =
                $"[Fase: {SceneManager.GetActiveScene().buildIndex}] [Envio: {_messageID}] Respuesta del cliente a pregunta: {jsonResponse.answer}.\n\n" +
                $"Contratar abogado: {jsonResponse.contratar_abogado}\n" +
                $"Respuesta valida: {jsonResponse.respuesta_valida}\n" +
                $"Respuesta coherente: {jsonResponse.respuesta_coherente}";

                LLMLogManager.Instance.LogMessageSent(log, _messageID);

                if(!jsonResponse.respuesta_valida || !jsonResponse.respuesta_coherente)
                {
                    TelemetryDispatch.SendNotConsistentAnswer(_messageID);
                }

                _historical.Add("Respuesta :" + jsonResponse.answer);
                _stepCounter = 0;
                _uiMeeting.ShowMessage(_abogadoContratado);
                _buttonContinue.SetActive(true);
                _promptSent = false;    
            }
        }
        else
        {
            Debug.LogError("Error en la llamada al LLM: " + answer);
            _uiMeeting.EndPendingMessage("Error al contactar con el modelo.");
        }

    }

    public void CallSendContext(int indexConfig = 0)
    {
        sendContextPrompt(indexConfig);
    }

    public void CallSendContext(string text, int indexConfig = 0)
    {
        sendContextPrompt(text, indexConfig);
    }

    protected override bool sendContextPrompt(int indexConfig = 0)
    {
        _uiMeeting.StartPendingMessage();

        _buttonContinue.SetActive(false);

        bool messageSent = base.sendContextPrompt(indexConfig);

        if (!messageSent)
        {
            _uiMeeting.EndPendingMessage("Puedes volver a repetirmelo por favor?");
        }

        return messageSent;
    }

    protected override bool sendContextPrompt(string text, int indexConfig = 0)
    {
        _uiMeeting.StartPendingMessage();

        _buttonContinue.SetActive(false);

        bool messageSent = base.sendContextPrompt(text, indexConfig);

        if (!messageSent)
        {
            _uiMeeting.EndPendingMessage("Puedes volver a repetirmelo por favor?");
        }

        return messageSent;
    }

    protected override bool sendSecuritySteps(string prompt)
    {
        _uiMeeting.StartPendingMessage();

        bool securityStepSent = base.sendSecuritySteps(prompt);

        if (!securityStepSent)
        {
            _uiMeeting.EndPendingMessage("Puedes volver a repetirmelo por favor?");
        }

        return securityStepSent;
    }

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
        _contextSchema.properties.Add("respuesta_valida", new PropertyInfo(JsonDataType.Boolean));
        _contextSchema.properties.Add("respuesta_coherente", new PropertyInfo(JsonDataType.Boolean));
        _contextSchema.properties.Add("contratar_abogado", new PropertyInfo(JsonDataType.Boolean));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
        _stepsSchema.properties.Add("respuesta_valida", new PropertyInfo(JsonDataType.Boolean));
        _stepsSchema.properties.Add("respuesta_coherente", new PropertyInfo(JsonDataType.Boolean));

        _schemasCreated = true;
    }

    private void Awake()
    {
        createJsonSchemas();
    }

}
