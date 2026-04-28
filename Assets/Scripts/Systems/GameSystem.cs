using UnityEngine;

public class GameSystem : MonoBehaviour
{
    [SerializeField] private Notepad _notepad;

    private static GameSystem instance = null;
    private bool initialized = false;
    public static GameSystem Instance
    {
        get {  return instance; }
    }
    private CaseData _caseData = null; 
    public CaseData CaseData { get { return _caseData; } }


    /// <summary>
    /// Crea caseData con datos ejemplo para placeholder antes de usar la LLM
    /// </summary>
    private void CreateExampleCaseData()
    {
        float chanceOfInstantRejectionConciliacion = Random.Range(0f,0.5f);
        string clientName = "Pedro Muñoz";
        string procuratorName = "Alberto Velazquez";
        string demandedEntityName = "Ana Pérez";
        string caseDescription = "En fecha enero de 2021, el demandante comienza a detectar daños materiales en su vivienda consistentes " +
            "en humedades en techo y paredes, desprendimiento de pintura, aparición de moho y deterioro progresivo del suelo de parquet." +
            "\r\n\r\nTras diversas comprobaciones, se identifica como posible origen de los daños una fuga de agua procedente del cuarto " +
            "de baño de la vivienda superior, propiedad de la demandada.\r\n\r\nEl demandante realiza varios intentos de contacto con la " +
            "demandada a fin de solucionar el problema, sin que se adopten medidas eficaces para la reparación del origen de la filtración.";

        _caseData = new CaseData(chanceOfInstantRejectionConciliacion, clientName, procuratorName, demandedEntityName, caseDescription);

        _caseData.clientMessages.Add(new ConversationMessage("Hola, si tienes alguna duda sobre algo que pueda contarte o cuando sepas que documentos debo conseguir por favor dímelo.", false));
        _caseData.procuratorMessages.Add(new ConversationMessage("Buenas! Mi nombre es " + _caseData.procuratorName + ", seré tu procurador para este caso. Cualquier documento que consideres pertinente adjuntar al proceso, mándamelo y lo registraré.", false));
    }

    /// <summary>
    /// Llamarlo al volver al menu prinicpal
    /// </summary>
    public void ResetCaseData()
    {
        
        CreateExampleCaseData();
    }


    public void DEBUG_ClearPlayerDocs()
    {
        _caseData.documentManager.DEBUG_ClearPlayerDocs();
    }

    /// <summary>
    /// Activa o dessactiva el notepad
    /// </summary>
    /// <param name="on"></param>
    public void ToggleNotepad(bool on)
    {
        _notepad.ToggleNotepad(on);
    }

    public void ResetNotepad()
    {
        _notepad.ResetText();
    }

    private void Init()
    {
        DontDestroyOnLoad(gameObject);

        CreateExampleCaseData();

        initialized = true;
    }

    private void Awake()
    {
        if (GameSystem.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        if (!initialized)
        {
            instance = this;
            Init();
        }
    }
}
