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


        docManager.CreateDocument("Perito de daños por humedad", DocumentType.Perito, "Contenido del documento: - Introducción\r\n\r\nEste informe pericial es elaborado por un perito especializado con el objetivo de analizar los hechos relacionados con el caso y determinar las circunstancias técnicas relevantes para su valoración en el ámbito civil. Se ha realizado una evaluación técnica detallada basada en las pruebas presentadas y la inspección judicial.\r\n\r\nDescripción de los Hechos\r\n\r\nEn enero de 2021, el demandante comienza a detectar daños materiales en su vivienda consistentes en humedades en techo y paredes, desprendimiento de pintura, aparición de moho y deterioro progresivo del suelo de parquet. Las primeras inspecciones identificaron una posible fuga de agua procedente del cuarto de baño de la vivienda superior, propiedad de la demandada.\r\n\r\nMetodología de Análisis\r\n\r\nPara la elaboración de este informe se ha realizado un análisis documental, revisión técnica de los elementos afectados y aplicación de criterios periciales basados en la práctica profesional. Se han considerado las facturas de reparaciones previas, fotografías cronológicas y mediciones de humedad. Se realizó una inspección judicial exhaustiva del inmueble para corroborar los hallazgos.\r\n\r\nResultados\r\n\r\nExistencia de Humedad Activa: Se constató la presencia de humedad activa en el inmueble, coincidente con el baño superior de la vivienda de la demandada.\r\nPatrón Descendente: El patrón de humedad es descendente y localizado, lo que sugiere un origen en la instalación privativa del cuarto de baño superior.\r\nCorrespondencia entre Daños y Origen Señalado: Existe una correspondencia clara entre los daños detectados y el origen señalado por el perito del demandante.\r\n\r\nConclusiones\r\n\r\nSe concluye que la fuga de agua procedente del cuarto de baño superior de la vivienda de la demandada es el origen más probable de las humedades y otros daños en la vivienda del demandante. Se recomienda una reparación inmediata de la avería para evitar un deterioro aún mayor.", true, 0, false, false);

        docManager.CreateDocument("Conversación de whatsapp", DocumentType.Report, "Se adjunta una conversación de whatsapp donde Ana ignora las advertencias de que le están formando zonas húmedas en el techo. Pedro advierte varias ocasiones y no recibe respuesta.", true, 0, false, false);

        docManager.CreateDocument("Factura de reparación de daños", DocumentType.ReceiptFacture, "Se adjunta un presupuesto de la reparación de los daños causados por humedad que son 15000 euros.", true, 0, false, false);

        docManager.CreateDocument("Testimonio de vecinos sobre la actitud de Ana", DocumentType.Witness, "Yo, Fran Bernabé, soy vecino del edificio, y rara vez he visto a Ana ser amable o saludar a vecinos. Además hace mucho que no la veo en juntas de comunidad de vecinos, se podría decir que nos tiene olvidados.", true, 0, false, false);

        bool relevant = true;

        docManager.CreateDocument("Factura de reparación de tuberías 2012", DocumentType.ReceiptFacture, "Se adjunta una factura de una reparación integral de todas las tuberías a causa de un reventón por frío de unas tuberías. Se sustituyeron todas las tuberías antiguas por unas nuevas en toda la casa.", relevant, 0, true, true);

        docManager.CreateDocument("Informe del origen de la fuga de agua", DocumentType.Report, "Se ha realizado una investigación y no se puede determinar el origen concreto de la fuga de agua a la casa de Pedro. Dado que la zona afectada es tan grande, pasa por zonas de tuberías de la comunidad como por zonas de tuberías de la casa de Ana, por lo que no hay pruebas concluyentes de que la fuga provenga de una tubería de Ana.", relevant, 0, true, true);

        relevant = false;

        docManager.CreateDocument("Testimonio de Juan Pérez", DocumentType.Witness, "Yo, Juan Pérez, estuve en casa de mi prima Ana el pasado fin de semana. Miré por encima el baño y no vi ninguna fuga. Las tuberías se ven secas. Creo que el problema de abajo es porque el edificio es viejo y las bajantes de la comunidad están mal.", relevant, 0, true, true);

        docManager.CreateDocument("Conversación de whatsapp", DocumentType.Report, "Se adjunta una conversación de whatsapp donde Ana habla con otra vecina donde la vecina se queja de que Pedro es un exagerado y suele decir que las cosas son más grandes de las que son, y que seguramente quiere que Ana le pague los daños pero para pintarse la casa gratis.", relevant, 0, true, true);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        { 
            Destroy(gameObject);
            return;
        }

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
        if(cheatMenu != null) cheatMenu.GetComponentInChildren<TMP_InputField>().onEndEdit.RemoveAllListeners();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loaded scene: " + scene.name);

        

        cheatMenu = GameObject.FindGameObjectWithTag("CheatMenu");

        if (scene.name == "Phase1")
        {
            cheatMenu.GetComponentInChildren<TMP_InputField>().onEndEdit.RemoveAllListeners();
            cheatMenu.GetComponentInChildren<TMP_InputField>().onEndEdit.AddListener(Phase1toPhase2);
        }
        
        if (cheatMenu != null)
            cheatMenu.SetActive(false);
    }

    public void Phase1toPhase2(string text)
    {
        BudgetManager.Instance.SetBudget(text);
        SceneSystem.Instance.LoadPhase2();
    }

    public void AddBudget(string text)
    {
        BudgetManager.Instance.AddBudget(text);

    }
}
