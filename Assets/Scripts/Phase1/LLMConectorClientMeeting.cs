using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LLMConectorClientMeeting : LLMConector
{
    [SerializeField]
    private UIClientMeetingManager _uiMeeting;

    [Serializable]
    private class MeetingResponse
    {
        public string answer;
        public bool contratar_abogado;
    }

    private bool _abogadoContratado = false;

    

    /**
     * Metodo encargado de recoger la respuesta del LLM y transmitirla a la clase que muestre el output de este
     * @param success: muestra si ha podido obtenerse una respuesta del LLM
     * @param answer: texto plano que ha sacado el LLM como output
     */
    public override void RecieveChatMessage(bool success, string answer)
    {
        
        if (success)
        {
            // deserializamos la respuesta
            MeetingResponse jsonResponse = JsonUtility.FromJson<MeetingResponse>(answer);

            Debug.Log("Respuesta cruda: " + jsonResponse.answer);

            if (_stepCounter == 0)
            {
                Debug.Log("Respuesta contratar abogado: " + jsonResponse.contratar_abogado);
                _abogadoContratado = jsonResponse.contratar_abogado;
            }

            _uiMeeting.EndPendingMessage(jsonResponse.answer);          

            if (_stepCounter < _config[_indexConfig].getStepsChecks().Length && _useSecuritySteps)
            {
                SendSecuritySteps(jsonResponse.answer);
            }
            else
            {
                Debug.Log("Respuesta final");
                _historical.Add("Respuesta :" + jsonResponse.answer);
                _stepCounter = 0;
                _uiMeeting.ShowMessage(_abogadoContratado);
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
        SendContextMessage(indexConfig);
    }

    protected override bool SendContextMessage(int indexConfig = 0)
    {
        _uiMeeting.StartPendingMessage();

        bool messageSent = base.SendContextMessage(indexConfig);

        if (!messageSent)
        {
            _uiMeeting.EndPendingMessage("Puedes volver a repetirmelo por favor?");
        }

        return messageSent;
    }

    protected override bool SendSecuritySteps(string prompt)
    {
        _uiMeeting.StartPendingMessage();

        bool securityStepSent = base.SendSecuritySteps(prompt);

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
        _contextSchema.properties.Add("contratar_abogado", new PropertyInfo(JsonDataType.Boolean));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    private void Awake()
    {
        createJsonSchemas();
    }

}
