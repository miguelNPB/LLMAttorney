using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        public bool respuestaValida;
        public bool respuestaCoherente;
        public bool contratarAbogado;
    }

    private bool _abogadoContratado = false;

   
    protected override void receiveResponse(bool success, string answer)
    {
        if (success)
        {
            // deserializamos la respuesta
            MeetingResponse jsonResponse = JsonUtility.FromJson<MeetingResponse>(answer);

            Debug.Log("Respuesta cruda: " + jsonResponse.answer);

            if (_stepCounter == 0)
            {
                Debug.Log("Respuesta contratar abogado: " + jsonResponse.contratarAbogado);
                _abogadoContratado = jsonResponse.contratarAbogado;
            }

            Debug.Log("Respuesta valida: " + jsonResponse.respuestaValida);

            Debug.Log("Respuesta coherente: " + jsonResponse.respuestaCoherente);

            if (jsonResponse.respuestaValida && jsonResponse.respuestaCoherente)
            {
                _uiMeeting.EndPendingMessage(jsonResponse.answer);
            }
            else if (!jsonResponse.respuestaCoherente)
            {
                _uiMeeting.EndPendingMessage("Perdona pero �Podriamos centrarnos en mi caso?");
            }
            else
            {
                _uiMeeting.EndPendingMessage("No te he entendido bien, puedes repetirlo");
            }
                       

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length && _useSecuritySteps &&
                (!jsonResponse.respuestaValida || !jsonResponse.respuestaCoherente))
            {
                sendSecuritySteps(jsonResponse.answer);
            }
            else
            {
                Debug.Log("Respuesta final");
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
