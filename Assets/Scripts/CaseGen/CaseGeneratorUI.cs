using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaseGenerationUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private LLMCaseGenerator _caseGenerator;
    [SerializeField] private LLMConnectorCaseDataGenerator _dataGenerator;
    [SerializeField] private LLMConnectorClientIntroductionGenerator _introGenerator;
    [SerializeField] private SaveCase _saveCase;

    [Header("UI")]
    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private Button _generateButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _closeButton;

    private bool _waitingPendingMessage;
    private string _pendingMessage;

    private void Awake()
    {
        if (_displayText == null) 
        {
            Debug.LogError("[CaseGenerationUI] Display Text sin asignar.");
        }
        if (_generateButton == null) 
        {
            Debug.LogError("[CaseGenerationUI] Generate Button sin asignar.");
        }
        else
        {
            _generateButton.onClick.AddListener(() => _caseGenerator.GenerateCase());
        }
        if (_saveButton == null) 
        {
            Debug.LogError("[CaseGenerationUI] Save Button sin asignar.");
        }
        else
        {
            _saveButton.onClick.AddListener(() => _saveCase.SaveAll());
        }
        if (_caseGenerator != null) 
        {
            _caseGenerator.OnGenerationStarted += OnGenerationStarted;
        }
        else
        {
            Debug.LogError("[CaseGenerationUI] LLMCaseGenerator sin asignar.");
        }
        if (_dataGenerator != null)
        {
                        _dataGenerator.OnDataReceived += OnDataReceived;
        }

        else
        {
            Debug.LogError("[CaseGenerationUI] LLMConnectorCaseDataGenerator sin asignar.");
        }

        if (_introGenerator != null)
        {            
            _introGenerator.OnIntroductionReceived += OnGenerationComplete;
        }

        else
        {
            Debug.LogError("[CaseGenerationUI] LLMConnectorClientIntroductionGenerator sin asignar.");
        }


        SetSaveButton(false, "Genera un caso primero");
        SetGenerateButton(true);
        if (_closeButton != null)
        {
            _closeButton.interactable = true;
        }
    }

    private void OnDestroy()
    {
        if (_dataGenerator != null)  _dataGenerator.OnDataReceived -= OnDataReceived;
        if (_introGenerator != null) _introGenerator.OnIntroductionReceived -= OnGenerationComplete;
    }

    // Call this when GenerateCase is triggered
    public void OnGenerationStarted()
    {
        SetGenerateButton(false);
        SetSaveButton(false);
        if (_closeButton != null)
        {
            _closeButton.interactable = false;
        }
        StartCoroutine(coroutinePendingMessage());
    }

    private void OnDataReceived(LLMConnectorCaseDataGenerator.CaseDataRetrieval data)
    {
        _pendingMessage = $"Cliente: {data.clientName}\nRival: {data.rivalName}\nResumen: {data.caseSummary}";
        _waitingPendingMessage = false;
    }

    private void OnGenerationComplete(LLMConnectorClientIntroductionGenerator.CaseIntroduction _)
    {
        SetGenerateButton(true, "Generar otro caso");
        SetSaveButton(true);
        if (_closeButton != null)
        {
            _closeButton.interactable = true;
        }
    }

    private IEnumerator coroutinePendingMessage()
    {
        _waitingPendingMessage = true;
        _pendingMessage = null;

        float timer = 0;

        while (_waitingPendingMessage)
        {
            timer += Time.deltaTime;
            _displayText.text = "Generando caso";
            for (int i = 0; i <= (int)(timer % 3); i++)
                _displayText.text += ".";
            yield return null;
        }

        _displayText.text = _pendingMessage;
    }

    private void SetGenerateButton(bool state, string pendingText = "Generar caso")
    {
        if (_generateButton != null) _generateButton.interactable = state;
        _generateButton.GetComponentInChildren<TMP_Text>().text = pendingText;
    }

    private void SetSaveButton(bool state, string pendingText = "Generando datos del caso...")
    {
        if (_saveButton != null) _saveButton.interactable = state;
        _saveButton.GetComponentInChildren<TMP_Text>().text = state ? "Guardar caso" : pendingText;
    }
}