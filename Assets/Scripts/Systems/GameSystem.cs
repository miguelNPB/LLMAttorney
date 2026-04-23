using UnityEngine;

public class GameSystem : MonoBehaviour
{
    private static GameSystem instance = null;
    private bool initialized = false;
    public static GameSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<GameSystem>();
                if (instance != null && instance._caseData == null)
                {
                    instance.Init();
                }
            }
            return instance;
        }
    }
    private CaseData _caseData = null; 
    public CaseData CaseData { get { return _caseData; } }

    [SerializeField]
    public DocumentManager myDocumentManager;

    [SerializeField, TextArea(3, 10)]
    private string _caseDescription;


    /// <summary>
    /// Crea caseData con datos ejemplo para placeholder antes de usar la LLM
    /// </summary>
    private void CreateExampleCaseData()
    {
        _caseData = new CaseData();

        _caseData.isDemanda = true;
        _caseData.clientName = "Pedro Muñoz";
        _caseData.procuratorName = "Alberto Velazquez";
        _caseData.demandedEntityName = "Ana Pérez";
        _caseData.clientMessages.Add(new ConversationMessage("Hola, cuando sepas que documentos debo conseguir por favor dimelo", false));

        _caseData.procuratorMessages.Add(new ConversationMessage("Buenas! Mi nombre es " + _caseData.procuratorName + ", seré tu procurador para este caso. Cualquier documento que consideres pertinente adjuntar al proceso, mándamelo y lo registraré.", false));

        _caseData.conciliationRivalInstantRejectProbability = 1;

        _caseData.caseDescription = _caseDescription;
    }

    private void Init()
    {
        DontDestroyOnLoad(gameObject);

        CreateExampleCaseData();

        initialized = true;
    }

    private void Awake()
    {
        if (GameSystem.Instance != null && Instance != this)
            Destroy(this);

        if (!initialized)
        {
            instance = this;
            Init();
        }
    }
}
