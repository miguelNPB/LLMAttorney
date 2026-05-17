using System;
using UnityEngine;

public class LLMConnectorQuestionChecker : LLMConnector
{
    [Serializable]
    private class QuestionCheckerResponse
    {
        public string answer;
    }
    private Action<string> _responseCallback;

    /// <summary>
    /// Metodo publico para activar el funcionamiento de este LLMConnector
    /// </summary>
    /// <param name="responseCallback"></param>
    /// <param name="prompt"></param>
    public void SendPrompt(Action<string> responseCallback, string prompt)
    {
        _responseCallback = responseCallback;
        sendPrompt(recieveFinalResponse, prompt, 0);
    }

    /// <summary>
    /// Metodo final para devolver la respuesta
    /// </summary>
    /// <param name="finalSerializedResponse"></param>
    private void recieveFinalResponse(string finalSerializedResponse)
    {
        QuestionCheckerResponse jsonResponse = JsonUtility.FromJson<QuestionCheckerResponse>(finalSerializedResponse);

        _responseCallback?.Invoke(jsonResponse.answer);
    }
    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("answer", new PropertyInfo(JsonDataType.String));
    }

    protected override string deseralizePromptFirstResponse(string firstResponse)
    {
        QuestionCheckerResponse jsonResponse = JsonUtility.FromJson<QuestionCheckerResponse>(firstResponse);
        return jsonResponse.answer;
    }

    protected override string deseralizePromptStepResponse(string firstResponse)
    {
        QuestionCheckerResponse jsonResponse = JsonUtility.FromJson<QuestionCheckerResponse>(firstResponse);
        return jsonResponse.answer;
    }
}
