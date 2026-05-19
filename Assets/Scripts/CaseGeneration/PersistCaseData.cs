using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Clase para persistir a un pdf el contenido de un caso generado
/// </summary>
public class PersistCaseData : MonoBehaviour
{
    [Serializable]
    private class CaseDataSerializable
    {
        public string clientName;
        public string rivalName;
        public string caseSummary;
        public string caseClientIntroduction;
    }

    [SerializeField] private CasePdfBuilder _pdfBuilder;

    /// <summary>
    /// Persiste un caso generado con el caseGenerator. Devuelve un log con datos de donde ha persistido.
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="rivalName"></param>
    /// <param name="caseSummary"></param>
    /// <param name="caseContent"></param>
    /// <param name="caseClientIntroduction"></param>
    /// <returns></returns>
    public string PersistCase(string clientName, string rivalName, string caseSummary, string caseContent, string caseClientIntroduction)
    {
        int uniqueID = Guid.NewGuid().GetHashCode() & int.MaxValue;

        string path = WinDirSelect.Open("Selecciona la carpeta donde guardar los ficheros generados del caso", Application.persistentDataPath);

        saveCaseDataJSON(uniqueID, path, clientName, rivalName, caseSummary, caseClientIntroduction);
        saveCaseContentPDF(uniqueID, path, caseContent);

        return "¡Caso generado!\nEl json del caso se encuentra en: " + (path, "\\savedCaseData_" + uniqueID + ".json") + "\nEl pdf del caso se encuentra en: " + (path, "\\savedCaseContent_" + uniqueID + ".pdf"); ;
    }

    /// <summary>
    /// Persiste datos de un caso a un Json situado en Users/(usuario)/Appdata/LocalLow/LLMAttorney
    /// </summary>
    /// <param name="id"></param>
    /// <param name="clientName"></param>
    /// <param name="rivalName"></param>
    /// <param name="caseSummary"></param>
    /// <param name="caseClientIntroduction"></param>
    private void saveCaseDataJSON(int id, string path, string clientName, string rivalName, string caseSummary, string caseClientIntroduction)
    {
        int uniqueID = Guid.NewGuid().GetHashCode() & int.MaxValue;

        CaseDataSerializable serializedData = new CaseDataSerializable();
        serializedData.clientName = clientName;
        serializedData.rivalName = rivalName;
        serializedData.caseSummary = caseSummary;
        serializedData.caseClientIntroduction = caseClientIntroduction;

        path = Path.Combine(path, "savedCaseData_" + uniqueID + ".json");
        File.WriteAllText(path, JsonUtility.ToJson(serializedData));

        LogSystem.Instance.LogString($"[SaveCase] CaseData saved: {path}"); 
    }

    /// <summary>
    /// Persiste el contenido de un caso a un pdf, para poder utilizarlo de rag en el servidor.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="caseContent"></param>
    private void saveCaseContentPDF(int id, string path, string caseContent)
    {
        string pdfPath = _pdfBuilder.Build(id, path, caseContent);

        LogSystem.Instance.LogString($"[SaveCase] PDF saved: {pdfPath}");
    }
}