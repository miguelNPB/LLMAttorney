using UnityEngine;

public class CheatsSystem : MonoBehaviour
{
    public static CheatsSystem Instance { get { return instance; } }
    private static CheatsSystem instance = null;

    /// <summary>
    /// Limpiar documentos y usar unos prehechos para evitar softlocks pro alucinaciones del llm
    /// </summary>
    public void UsePremadeDocuments()
    {
        GameSystem.Instance.ResetCaseData();

        DocumentManager docManager = GameSystem.Instance.CaseData.documentManager;
    }

    private void Awake()
    {
        if (CheatsSystem.Instance != null && Instance != this)
            Destroy(this);
    }
}
