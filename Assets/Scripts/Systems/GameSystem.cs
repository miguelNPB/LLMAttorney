using UnityEngine;

public class GameSystem : MonoBehaviour
{
    private static GameSystem instance = null; public static GameSystem Instance { get { return instance; } }
    private CaseData _caseData = null; public CaseData CaseData { get { return _caseData; } }


    /// <summary>
    /// Crea caseData con datos ejemplo para placeholder antes de usar la LLM
    /// </summary>
    private void CreateExampleCaseData()
    {
        _caseData = new CaseData();

        _caseData.clientMessages.Add(new ConversationMessage("Hola abogado!", false));
        _caseData.clientMessages.Add(new ConversationMessage("Me llamo Carlos Leon, y me comunico contigo para que me ayudes con una disputa que he tenido con mi vecino, Guillermo Jimenez", false));
        _caseData.clientMessages.Add(new ConversationMessage("Claro! Cuentame!", true));
        _caseData.clientMessages.Add(new ConversationMessage("asdf", false));
    }

    private void Awake()
    {
        if (GameSystem.Instance != null)
            Destroy(this);

        instance = this;

        DontDestroyOnLoad(gameObject);

        CreateExampleCaseData();
    }


}
