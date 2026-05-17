using System;
using UnityEngine;
using UnityEngine.UI;

public class LLMConnectorClientIntroductionGenerator : LLMConector
{

    private class CaseResponse
    {
        public string CaseContent;
    }

    private class SummaryResponse
    {
        public string Summary;
    }


    [Header("Dependencies")]
    [SerializeField] private LLMCasePdfBuilder _pdfBuilder;

    [Header("Case generation prompts")]
    [TextArea(6, 20)]
    [Tooltip("System prompt para generar el caso completo.")]
    //? Quizás quitar esto? por ahorta se queda para agilizar el asunto xd
    public string caseConfigPrompt =
        "Eres un redactor juridico especializado en derecho civil espanol. " +
        "Tu tarea es inventar un caso ficticio completo de responsabilidad civil extracontractual entre particulares.\n\n" +
        "INVENTA libremente: nombres, fechas, tipo de dano, importes, circunstancias. Cada generacion debe ser diferente.\n\n" +
        "TIPOS DE DANO posibles (elige uno al azar):\n" +
        "- Filtraciones de agua entre viviendas\n" +
        "- Danos por obras en inmueble colindante\n" +
        "- Caida de objetos desde propiedad ajena\n" +
        "- Incendio propagado por negligencia\n" +
        "- Danos por animales domesticos\n" +
        "- Inundacion por rotura de instalacion privativa\n" +
        "- Desprendimiento de elementos constructivos\n\n" +
        "RESTRICCIONES ABSOLUTAS:\n" +
        "- Procedimiento: SIEMPRE juicio ordinario civil (nunca juicio verbal)\n" +
        "- NO puede haber testigos en ninguna seccion\n" +
        "- Prueba limitada a: pericial, documental e inspeccion judicial\n" +
        "- Normativa: solo Codigo Civil y Ley de Enjuiciamiento Civil\n" +
        "- Importes totales: entre 2.000 EUR y 15.000 EUR\n" +
        "- Redaccion en espanol juridico formal\n\n" +
        "ESTRUCTURA OBLIGATORIA - 14 secciones en este orden exacto:\n" +
        "CASO DE RESPONSABILIDAD CIVIL POR DANOS MATERIALES ENTRE PARTICULARES\n" +
        "([subtitulo descriptivo del tipo de dano])\n\n" +
        "1. IDENTIFICACION DEL CASO\n" +
        "2. ANTECEDENTES DE HECHO\n" +
        "3. ACTUACIONES PREVIAS AL PROCESO\n" +
        "   3.1 Reclamacion extrajudicial\n" +
        "   3.2 Informe pericial previo\n" +
        "4. INTERPOSICION DE LA DEMANDA\n" +
        "   4.1 Pretensiones\n" +
        "   4.2 Fundamentacion juridica\n" +
        "5. CONTESTACION A LA DEMANDA\n" +
        "6. AUDIENCIA PREVIA\n" +
        "   6.1 Fijacion de hechos controvertidos\n" +
        "   6.2 Proposicion de prueba\n" +
        "7. JUICIO\n" +
        "   7.1 Prueba pericial\n" +
        "   7.2 Prueba documental\n" +
        "   7.3 Inspeccion judicial\n" +
        "8. FUNDAMENTOS DE DERECHO\n" +
        "9. SENTENCIA DE PRIMERA INSTANCIA\n" +
        "10. RECURSO DE APELACION\n" +
        "11. RESOLUCION DE LA AUDIENCIA PROVINCIAL\n" +
        "12. FALLO\n" +
        "13. CONCLUSIONES JURIDICAS\n" +
        "14. OBSERVACIONES PARA ANALISIS\n\n" +
        "Devuelve SOLO un JSON con la clave \"CaseContent\" conteniendo el documento completo. Sin texto adicional.";

    [TextArea(2, 5)]
    [Tooltip("User prompt para disparar la generacion del caso.")]
    public string caseUserPrompt =
        "Genera un caso de responsabilidad civil extracontractual completamente inventado siguiendo la estructura indicada.";

    [Header("Summary prompts")]
    [TextArea(4, 10)]
    [Tooltip("System prompt para generar el resumen de contexto del caso.")]
    public string summaryConfigPrompt =
        "Eres un asistente juridico. A partir del caso completo que se te proporciona, " +
        "genera un resumen breve (maximo 5 frases) que sirva como descripcion de contexto para el simulador. " +
        "Incluye: tipo de dano, partes implicadas, importe reclamado y estado del procedimiento. " +
        "Redaccion en espanol juridico formal. " +
        "Devuelve SOLO un JSON con la clave \"Summary\" conteniendo el resumen. Sin texto adicional.";

    [TextArea(2, 4)]
    [Tooltip("Prefijo del user prompt para el resumen; el caso completo se concatena automaticamente.")]
    public string summaryUserPromptPrefix =
        "Resume el siguiente caso para usarlo como contexto en el simulador:\n\n";


    public event Action<string, string> OnCaseGenerated;

    public event Action<string> OnSummaryReady;

    public event Action<string> OnError;


    private enum Step { Idle, GeneratingCase, GeneratingSummary }
    private Step _step = Step.Idle;

    private string _rawCaseContent;


    private const string KEY_CASE    = "CaseContent";
    private const string KEY_SUMMARY = "Summary";


    protected override void createJsonSchemas()
    {
        _contextSchema = new JsonSchema();
        _contextSchema.properties.Add(KEY_CASE, new PropertyInfo(JsonDataType.String));

        _stepsSchema = new JsonSchema();
        _stepsSchema.properties.Add(KEY_SUMMARY, new PropertyInfo(JsonDataType.String));

        _schemasCreated = true;
    }

    protected override void receiveResponse(bool success, string answer)
    {
        if (!success)
        {
            Fail("Error del servidor: " + answer);
            _step = Step.Idle;
            return;
        }

        switch (_step)
        {
            case Step.GeneratingCase:     HandleCaseResponse(answer);    break;
            case Step.GeneratingSummary:  HandleSummaryResponse(answer); break;
            default:
                Debug.LogWarning("[CaseGenerator] receiveResponse en Step.Idle inesperado.");
                break;
        }
    }


    private void HandleCaseResponse(string answer)
    {
        if (_stepCounter < _config[_indexConfig].getStepsChecks().Length)
        {
            sendSecuritySteps(answer);
            return;
        }

        _stepCounter = 0;
        _promptSent  = false;

        CaseResponse json = JsonUtility.FromJson<CaseResponse>(answer);

        if (json == null || string.IsNullOrWhiteSpace(json.CaseContent))
        {
            Fail("Respuesta JSON invalida o CaseContent vacio.");
            _step = Step.Idle;
            return;
        }

        _rawCaseContent = json.CaseContent;

        string pdfPath = _pdfBuilder.Build(_rawCaseContent);
        Debug.Log($"[CaseGenerator] PDF guardado: {pdfPath}");
        OnCaseGenerated?.Invoke(pdfPath, _rawCaseContent);

        RequestSummary();
    }

    private void HandleSummaryResponse(string answer)
    {
        _stepCounter = 0;
        _promptSent  = false;
        _step        = Step.Idle;

        SummaryResponse json = JsonUtility.FromJson<SummaryResponse>(answer);

        if (json == null || string.IsNullOrWhiteSpace(json.Summary))
        {
            Fail("Respuesta JSON invalida o Summary vacio.");
            return;
        }

        if (GameSystem.Instance?.CaseData != null)
        {
            GameSystem.Instance.CaseData.SetCaseDescription(json.Summary);
            Debug.Log($"[CaseGenerator] Resumen guardado en CaseData.");
        }
        else
        {
            Debug.LogWarning("[CaseGenerator] GameSystem o CaseData null; resumen descartado.");
        }

        OnSummaryReady?.Invoke(json.Summary);
    }


    public void GenerateCase()
    {
        if (_step != Step.Idle)
        {
            Debug.LogWarning("[CaseGenerator] Generacion ya en curso.");
            return;
        }

        _step = Step.GeneratingCase;
        SendCasePrompt();
    }


    private void SendCasePrompt()
    {
        if (_promptSent || !_schemasCreated) return;
        if (_config.Length <= 0) { Fail("Ningun Config LLM asignado"); return; }

        string configLLM = BuildConfigLLM(caseConfigPrompt);
        _historical.Add("Pregunta: " + caseUserPrompt);
        _promptSent = true;

        StartCoroutine(coroutineSendPrompt(caseUserPrompt, configLLM, _contextSchema));
    }

    private void RequestSummary()
    {
        _step = Step.GeneratingSummary;

        string userMsg   = summaryUserPromptPrefix + _rawCaseContent;
        string configLLM = BuildConfigLLM(summaryConfigPrompt);

        _historical.Add("Pregunta (resumen): " + summaryUserPromptPrefix + "[caso completo]");
        _promptSent = true;

        StartCoroutine(coroutineSendPrompt(userMsg, configLLM, _stepsSchema));
    }

    private string BuildConfigLLM(string systemPrompt)
    {
        string configLLM = systemPrompt + _config[_indexConfig].safeguard;

        if (_useHistoricalInContext)
        {
            configLLM += "\n " + _config[_indexConfig].historicalConversation + "\n Historico: \n";
            foreach (string s in _historical)
                configLLM += s + "\n";
        }

        return configLLM;
    }


    private void Awake()
    {
        createJsonSchemas();

        if (_pdfBuilder == null)
            _pdfBuilder = GetComponent<LLMCasePdfBuilder>();

        if (_pdfBuilder == null)
            Debug.LogError("[CaseGenerator] LLMCasePdfBuilder no asignado ni encontrado en el GameObject.");

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


//! --- CODIGO PARA USAR CONFIGLLMINFO DESP ---

    // private const int INDEX_CASE    = 0;
    // private const int INDEX_SUMMARY = 1;






    // private void HandleCaseResponse(string answer)
    // {
    //     if (_stepCounter < _config[INDEX_CASE].getStepsChecks().Length)
    //     {
    //         sendSecuritySteps(answer);
    //         return;
    //     }

    //     _stepCounter = 0;
    //     _promptSent  = false;

    //     CaseResponse json = JsonUtility.FromJson<CaseResponse>(answer);

    //     if (json == null || string.IsNullOrWhiteSpace(json.CaseContent))
    //     {
    //         Fail("Respuesta JSON invalida o CaseContent vacio.");
    //         _step = Step.Idle;
    //         return;
    //     }

    //     _rawCaseContent = json.CaseContent;

    //     string pdfPath = _pdfBuilder.Build(_rawCaseContent);
    //     Debug.Log($"[CaseGenerator] PDF guardado: {pdfPath}");
    //     OnCaseGenerated?.Invoke(pdfPath, _rawCaseContent);

    //     RequestSummary();
    // }



    // public void GenerateCase()
    // {
    //     if (_step != Step.Idle)
    //     {
    //         Debug.LogWarning("[CaseGenerator] Generacion ya en curso.");
    //         return;
    //     }

    //     if (_config.Length <= INDEX_SUMMARY)
    //     {
    //         Fail($"Se necesitan al menos 2 ConfigLLMInfo en _config (indice {INDEX_CASE}=caso, {INDEX_SUMMARY}=resumen).");
    //         return;
    //     }

    //     _step = Step.GeneratingCase;
    //     SendCasePrompt();
    // }


    // private void SendCasePrompt()
    // {
    //     if (_promptSent || !_schemasCreated) return;
    //     sendContextPrompt(caseUserPrompt, INDEX_CASE);
    // }

    // private void RequestSummary()
    // {
    //     _step = Step.GeneratingSummary;

    //     _contextSchema = new JsonSchema();
    //     _contextSchema.properties.Add(KEY_SUMMARY, new PropertyInfo(JsonDataType.String));

    //     _stepsSchema = new JsonSchema();
    //     _stepsSchema.properties.Add(KEY_SUMMARY, new PropertyInfo(JsonDataType.String));

    //     _promptSent = false; // reset guard so sendContextPrompt fires

    //     string userMsg = summaryUserPromptPrefix + _rawCaseContent;
    //     sendContextPrompt(userMsg, INDEX_SUMMARY);
    // }
