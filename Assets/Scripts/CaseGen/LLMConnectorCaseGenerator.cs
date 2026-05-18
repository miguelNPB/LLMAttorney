using System;
using UnityEngine;

public class LLMCaseGenerator : LLMConnector
{
    [Serializable]
    public class CaseResponse
    {
        public string CaseContent;
    }

    [Header("Case generation prompts")]
    [TextArea(2, 5)]
    [Tooltip("User prompt para disparar la generacion del caso.")]
    public string caseUserPrompt =
        "Genera un caso de responsabilidad civil extracontractual completamente inventado siguiendo la estructura indicada.";

    public CaseResponse GeneratedCase { get; private set; }

    public event Action<string> OnCaseReceived;
    public event Action OnGenerationStarted;
    public event Action<string> OnError;


    private Action<string> _responseCallback;

    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add("CaseContent", new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add("CaseContent", new PropertyInfo(JsonDataType.String));
    }

    public void SendPrompt(Action<string> responseCallback, Action<string> errorCallback, string prompt)
    {
        _responseCallback = responseCallback;
        sendPrompt(receiveResponse, errorCallback, prompt, 0);
    }

    public void GenerateCase()
    {
        if (_llmConfigs == null || _llmConfigs.Length == 0)
        {
            Fail("Ningun LLMConfig asignado en el Inspector.");
            return;
        }

        if (!sendPrompt(receiveResponse, OnErrorCallback, caseUserPrompt, configIndex: 0))
            Fail("No se pudo enviar el prompt (prompt ya en curso o config invalido).");
        
        OnGenerationStarted?.Invoke();
    }

    private void receiveResponse(string answer)
    {
        GeneratedCase = JsonUtility.FromJson<CaseResponse>(answer);

        if (GeneratedCase == null || string.IsNullOrWhiteSpace(GeneratedCase.CaseContent))
        {
            Fail("Respuesta JSON invalida o CaseContent vacio.");
            return;
        }

        OnCaseReceived?.Invoke(GeneratedCase.CaseContent);
        _responseCallback?.Invoke(answer);
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        JsonUtility.FromJson<CaseResponse>(serializedResponse);
        return serializedResponse;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        JsonUtility.FromJson<CaseResponse>(serializedResponse);
        return serializedResponse;
    }

    private void OnErrorCallback(string error) => Fail(error);

    private void Fail(string msg)
    {
        Debug.LogError($"[CaseGenerator] {msg}");
        OnError?.Invoke(msg);
    }

    private void Awake() => createJsonSchemas();
}