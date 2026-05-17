using System;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class LLMCaseGenerator : LLMConnector
{
    [Serializable]
    private class CaseResponse
    {
        public string CaseContent;
    }

    [Header("Dependencies")]
    [SerializeField] private CasePdfBuilder _pdfBuilder;
    //! Asignar referencia al LLMConnectorCaseDataGenerator desde el Inspector
    [SerializeField] private LLMConnectorCaseDataGenerator _summaryGenerator;
    [SerializeField] private LLMConnectorClientIntroductionGenerator _clientInitialText;

    [Header("Case generation prompts")]
    [TextArea(2, 5)]
    [Tooltip("User prompt para disparar la generacion del caso.")]
    public string caseUserPrompt =
        "Genera un caso de responsabilidad civil extracontractual completamente inventado siguiendo la estructura indicada.";


    public event Action<string, string> OnCaseGenerated;
    public event Action<string> OnError;

    private const string KEY_CASE = "CaseContent";

    private string _generatedCase;


    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add(KEY_CASE, new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add(KEY_CASE, new PropertyInfo(JsonDataType.String));
    }

    protected override string deseralizePromptFirstResponse(string serializedResponse)
    {
        CaseResponse json = JsonUtility.FromJson<CaseResponse>(serializedResponse);
        return json?.CaseContent ?? string.Empty;
    }

    protected override string deseralizePromptStepResponse(string serializedResponse)
    {
        CaseResponse json = JsonUtility.FromJson<CaseResponse>(serializedResponse);
        return json?.CaseContent ?? string.Empty;
    }

    protected override void respondPrompt(bool success, string text)
    {
        base.respondPrompt(success, text);

        if (!success)
        {
            Fail("Error del servidor: " + text);
            return;
        }

        CaseResponse json = JsonUtility.FromJson<CaseResponse>(text);

        if (json == null || string.IsNullOrWhiteSpace(json.CaseContent))
        {
            Fail("Respuesta JSON invalida o CaseContent vacio.");
            return;
        }

        string pdfPath = _pdfBuilder.Build(json.CaseContent);
        Debug.Log($"[CaseGenerator] PDF guardado: {pdfPath}");
        OnCaseGenerated?.Invoke(pdfPath, json.CaseContent);

        _generatedCase = text;
        //! PLACEHOLDER — llamar al generador de resumen con el contenido del caso
        _summaryGenerator.SendPrompt(OnSummaryExtracted, _generatedCase);
    }



    public void GenerateCase()
    {
        if (_llmConfigs == null || _llmConfigs.Length == 0)
        {
            Fail("Ningun LLMConfig asignado en el Inspector.");
            return;
        }

        bool sent = sendPrompt(OnFinalResponse, caseUserPrompt, configIndex: 0);

        if (!sent)
            Fail("No se pudo enviar el prompt (prompt ya en curso o config invalido).");
    }

    private void OnFinalResponse(string finalText) { }

    private void OnSummaryExtracted(string finalText) {
        _clientInitialText.SendPrompt(OnFinalResponse, _generatedCase);
    }

    protected override void Awake()
    {
        base.Awake();

        if (_pdfBuilder == null)
            _pdfBuilder = GetComponent<CasePdfBuilder>();

        if (_pdfBuilder == null)
            Debug.LogError("[CaseGenerator] CasePdfBuilder no asignado ni encontrado en el GameObject.");

        if (_summaryGenerator == null)
            Debug.LogError("[CaseGenerator] LLMConnectorCaseDataGenerator no asignado.");

        Button but = GetComponent<Button>();
        if (but != null)
            but.onClick.AddListener(GenerateCase);
    }

    private void Fail(string msg)
    {
        Debug.LogError($"[CaseGenerator] {msg}");
        OnError?.Invoke(msg);
    }
}