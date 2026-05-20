using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Clase para persistir a un pdf el contenido de un caso generado
/// </summary>
public class PersistCaseData : MonoBehaviour
{
    [Serializable]
    public class CaseDataSerializable
    {
        public int id;
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

        bool savedJson = saveCaseDataJSON(uniqueID, path, clientName, rivalName, caseSummary, caseClientIntroduction);
        bool savedCaseContent = saveCaseContentPDF(uniqueID, path, caseContent);

        string returnString = "";
        if (savedJson && savedCaseContent)
            returnString = "¡Caso generado!\nEl json del caso se encuentra en: " + (path, "\\savedCaseData_" + uniqueID + ".json") + "\nEl pdf del caso se encuentra en: " + (path, "\\savedCaseContent_" + uniqueID + ".pdf");
        else
        {
            returnString =  "Fallo guardando caso, no se ha podido generar. Mirar log para ver error.";
        }

        return returnString;
    }

    /// <summary>
    /// Persiste datos de un caso a un Json situado en Users/(usuario)/Appdata/LocalLow/LLMAttorney. Devuelve true si salio bien
    /// </summary>
    /// <param name="id"></param>
    /// <param name="clientName"></param>
    /// <param name="rivalName"></param>
    /// <param name="caseSummary"></param>
    /// <param name="caseClientIntroduction"></param>
    private bool saveCaseDataJSON(int id, string path, string clientName, string rivalName, string caseSummary, string caseClientIntroduction)
    {
        int uniqueID = Guid.NewGuid().GetHashCode() & int.MaxValue;

        CaseDataSerializable serializedData = new CaseDataSerializable();
        serializedData.id = id;
        serializedData.clientName = clientName;
        serializedData.rivalName = rivalName;
        serializedData.caseSummary = caseSummary;
        serializedData.caseClientIntroduction = caseClientIntroduction;

        path = Path.Combine(path, "savedCaseData_" + uniqueID + ".json");
        
        try
        {
            File.WriteAllText(path, JsonUtility.ToJson(serializedData, true));

            LogSystem.Instance.LogString($"[SaveCase] CaseData saved: {path}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error guardando el json con el caso creado: " + e.Message);
            return false;
        }
    }

    /// <summary>
    /// Persiste el contenido de un caso a un pdf, para poder utilizarlo de rag en el servidor. Devuelve true si salio bien
    /// </summary>
    /// <param name="id"></param>
    /// <param name="caseContent"></param>
    private bool saveCaseContentPDF(int id, string path, string caseContent)
    {
        try
        {
            string pdfPath = _pdfBuilder.Build(id, path, caseContent);

            LogSystem.Instance.LogString($"[SaveCase] PDF saved: {pdfPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error guardando el pdf con el caso creado: " + e.Message);
            return false;
        }
    }
}