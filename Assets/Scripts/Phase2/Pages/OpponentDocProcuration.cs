using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OpponentDocConfig
{
    [Tooltip("Seconds between automatic opponent docs (min)")]
    public float minIntervalSeconds = 60f;

    [Tooltip("Seconds between automatic opponent docs (max)")]
    public float maxIntervalSeconds = 180f;

    [Tooltip("Case themes the opponent can use to build counter-documents")]
    public List<string> caseThemes = new();

    [Tooltip("Prompt types the opponent is allowed to generate")]
    public List<PromptType> allowedDocTypes = new()
    {
        PromptType.Perito,
        PromptType.Informe,
        PromptType.Testigo,
    };
}

public class OpponentDocProcuration : MonoBehaviour
{
    [Header("Config")]
    public OpponentDocConfig config = new();

    [Header("References")]
    [SerializeField] private LLMConnectorOpponentDocuments _llmConnector;


    private void Start()
    {
        StartCoroutine(TimedGenerationLoop());
    }

    public void OnDocumentGenerated(Document playerDoc)
    {
        string prompt =
            $"El abogado contrario ha presentado el documento: {playerDoc.GetDocName()}. " +
            $"Genera un documento que lo contradiga o debilite sus argumentos.";

        SetInputAndSend(prompt);
    }


    private IEnumerator TimedGenerationLoop()
    {
        while (true)
        {
            float wait = UnityEngine.Random.Range(
                config.minIntervalSeconds,
                config.maxIntervalSeconds
            );
            yield return new WaitForSeconds(wait);
            SetInputAndSend(BuildTimedPrompt());
        }
    }

    private string BuildTimedPrompt()
    {
        string themes = config.caseThemes.Count > 0
            ? string.Join(", ", config.caseThemes)
            : "los hechos del caso";

        PromptType chosenType = config.allowedDocTypes[
            UnityEngine.Random.Range(0, config.allowedDocTypes.Count)
        ];

        return $"Genera un documento de tipo {chosenType} de parte contraria " +
               $"relacionado con: {themes}. Debilita la posición del abogado defensor.";
    }

    private void SetInputAndSend(string prompt)
    {
        _llmConnector.inputField.text = prompt;
        _llmConnector.CallSendContext();
    }
}