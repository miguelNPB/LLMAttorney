using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveCase : MonoBehaviour
{
    [SerializeField] private LLMCaseGenerator _caseGenerator;
    [SerializeField] private LLMConnectorCaseDataGenerator _dataGenerator;
    [SerializeField] private LLMConnectorClientIntroductionGenerator _introGenerator;
    [SerializeField] private CasePdfBuilder _pdfBuilder;


    public void SaveAll()
    {
        SaveCasePdf();
        SaveCaseData();
        SaveIntroduction();
    }

    public void SaveCasePdf()
    {
        if (_caseGenerator.GeneratedCase == null ||
            string.IsNullOrWhiteSpace(_caseGenerator.GeneratedCase.CaseContent))
        {
            Debug.LogWarning("[SaveCase] No case generated yet.");
            return;
        }
        string pdfPath = _pdfBuilder.Build(_caseGenerator.GeneratedCase.CaseContent);
        Debug.Log($"[SaveCase] PDF saved: {pdfPath}");
    }

    public void SaveCaseData()
    {
        if (_dataGenerator.LastResponse == null)
        {
            Debug.LogWarning("[SaveCase] No case data received yet.");
            return;
        }
        string path = Path.Combine(Application.persistentDataPath, "CaseData.json");
        File.WriteAllText(path, JsonUtility.ToJson(_dataGenerator.LastResponse));
        Debug.Log($"[SaveCase] CaseData saved: {path}");
    }

    public void SaveIntroduction()
    {
        if (_introGenerator.LastResponse == null ||
            string.IsNullOrEmpty(_introGenerator.LastResponse.introduction))
        {
            Debug.LogWarning("[SaveCase] No introduction received yet.");
            return;
        }
        string path = Path.Combine(Application.persistentDataPath, "CaseSummary.json");
        File.WriteAllText(path, JsonUtility.ToJson(_introGenerator.LastResponse));
        Debug.Log($"[SaveCase] Introduction saved: {path}");
    }
}