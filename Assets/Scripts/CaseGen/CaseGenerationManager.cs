using System.Collections;

using UnityEngine;


public class CaseGenerationSequencer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private LLMCaseGenerator _caseGenerator;
    [SerializeField] private LLMConnectorCaseDataGenerator _dataGenerator;
    [SerializeField] private LLMConnectorClientIntroductionGenerator _introGenerator;

    public string _caseContent;
    public string _clientName;
    public string _rivalName;
    public string _caseSummary;
    public string _caseIntroduction;



    private void Awake()
    {
        if (_caseGenerator == null) 
        {
            Debug.LogError("[CaseGenerationSequencer] LLMCaseGenerator sin asignar.");
        }
        else
        {
            _caseGenerator.OnCaseReceived += OnCaseReceived;
        }
        if (_dataGenerator == null)
        {
            Debug.LogError("[CaseGenerationSequencer] LLMConnectorCaseDataGenerator sin asignar.");
        }
        else
        {
            //_dataGenerator.OnDataReceived += OnDataReceived;
        }
        if (_introGenerator == null)
        {
            Debug.LogError("[CaseGenerationSequencer] LLMConnectorClientIntroductionGenerator sin asignar.");
        }
    }

    private void OnCaseReceived(string caseContent)
    {
        Debug.Log("[CaseGenerationSequencer] Caso recibido, iniciando generación de introducción...");
        _dataGenerator.SendPrompt(OnDataReceived, OnErrorCallback, caseContent);
    }

    private void OnDataReceived(string introduction)
    {
        Debug.Log("[CaseGenerationSequencer] Datos recibidos, iniciando generación de introducción...");
        _introGenerator.SendPrompt(OnCaseReceived, OnErrorCallback, introduction);
    }

    private void OnErrorCallback(string error)
    {
        Debug.LogError($"[CaseGenerationSequencer] Error: {error}");
    }

}