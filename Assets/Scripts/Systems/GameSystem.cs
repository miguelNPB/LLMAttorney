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
    private CaseData _caseData = null; public CaseData CaseData { get { return _caseData; } }

    [SerializeField]
    public DocumentManager myDocumentManager;


    /// <summary>
    /// Crea caseData con datos ejemplo para placeholder antes de usar la LLM
    /// </summary>
    private void CreateExampleCaseData()
    {
        _caseData = new CaseData();

        _caseData.isDemanda = true;
        _caseData.clientName = "Carlos Leon";
        _caseData.procuratorName = "Procuratus Maximus";
        _caseData.demandedEntityName = "Guillermo Jimenez";
        _caseData.clientMessages.Add(new ConversationMessage("Hola abogado!", false));
        _caseData.clientMessages.Add(new ConversationMessage("Me llamo Carlos Leon, y me comunico contigo para que me ayudes con una disputa que he tenido con mi vecino, Guillermo Jimenez", false));
        _caseData.clientMessages.Add(new ConversationMessage("Claro! Cuentame!", true));
        _caseData.clientMessages.Add(new ConversationMessage("Sabes que esto es una prueba de un sistema de texto de conversacion estilo whatsap?", false));
        _caseData.clientMessages.Add(new ConversationMessage("Bklajdsñkjfñlsajdfñkljasñkdfjlkasjdfñkaskfjasdfjsdkfjalkdsfklfkljdkfñaldskasdfñsdñkfasdjfkfklñkñdfksfjdfkñsdkflkdflkdfkñakjlkñsfljadkflañksjflkjdfkñ aklajsdkfljasdkñfjaklsdjfñklajsdñkfljañkljdflk aksdjfklñ?", true));
        _caseData.clientMessages.Add(new ConversationMessage("Estas bien?", false));

        _caseData.procuratorMessages.Add(new ConversationMessage("Buenas! Mi nombre es " + _caseData.procuratorName + ", seré tu procurador para este caso. Cualquier documento que consideres pertinente adjuntar al proceso, mándamelo y lo registraré.", false));

        _caseData.conciliationRivalInstantRejectProbability = 1;

        _caseData.caseDescription = "El cliente Carlos Leon quiere demandar a el rival Guillermo porque le ha asesinado a su perro.";
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
