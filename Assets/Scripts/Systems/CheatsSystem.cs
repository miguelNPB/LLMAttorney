using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatsSystem : MonoBehaviour
{
    public static CheatsSystem Instance { get { return _instance; } }
    private static CheatsSystem _instance = null;

    private bool initialized = false;

    public GameObject cheatMenu = null;

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

        if (!initialized)
        {
            _instance = this;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loaded scene: " + scene.name);

        cheatMenu = GameObject.FindGameObjectWithTag("CheatMenu");

        if (scene.name == "Phase1")
        {
            cheatMenu.GetComponentInChildren<TMP_InputField>().onEndEdit.AddListener(Phase1toPhase2);
        }
        cheatMenu.SetActive(false);
    }

    public void Phase1toPhase2(string text)
    {
        Debug.Log(text);
        GameSystem.Instance.GetComponentInChildren<BudgetManager>().SetBudget(text);
        GameSystem.Instance.GetComponentInChildren<SceneSystem>().LoadPhase2();
    }
}
