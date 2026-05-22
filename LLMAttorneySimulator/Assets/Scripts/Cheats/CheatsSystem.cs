using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de cheats para poder agilizar el flujo del juego durante las pruebas con usuarios
/// </summary>
public class CheatsSystem : MonoBehaviour
{
    public static CheatsSystem Instance { get { return _instance; } }
    private static CheatsSystem _instance = null;

    private bool initialized = false;

    private GameObject _cheatMenu = null;

    /// <summary>
    /// Limpiar documentos y usar unos prehechos para evitar softlocks pro alucinaciones del llm
    /// </summary>
    public void UsePremadeDocuments()
    {
        GameSystem.Instance.DEBUG_ClearPlayerDocs();

        DocumentManager docManager = GameSystem.Instance.CaseData.documentManager;

        docManager.CreateDocument("Perito de daños por humedad", DocumentType.Perito, "Contenido del documento: - Introducción\r\n\r\nEste informe pericial es elaborado por un perito especializado con el objetivo de analizar los hechos relacionados con el caso y determinar las circunstancias técnicas relevantes para su valoración en el ámbito civil. Se ha realizado una evaluación técnica detallada basada en las pruebas presentadas y la inspección judicial.\r\n\r\nDescripción de los Hechos\r\n\r\nEn enero de 2021, el demandante comienza a detectar daños materiales en su vivienda consistentes en humedades en techo y paredes, desprendimiento de pintura, aparición de moho y deterioro progresivo del suelo de parquet. Las primeras inspecciones identificaron una posible fuga de agua procedente del cuarto de baño de la vivienda superior, propiedad de la demandada.\r\n\r\nMetodología de Análisis\r\n\r\nPara la elaboración de este informe se ha realizado un análisis documental, revisión técnica de los elementos afectados y aplicación de criterios periciales basados en la práctica profesional. Se han considerado las facturas de reparaciones previas, fotografías cronológicas y mediciones de humedad. Se realizó una inspección judicial exhaustiva del inmueble para corroborar los hallazgos.\r\n\r\nResultados\r\n\r\nExistencia de Humedad Activa: Se constató la presencia de humedad activa en el inmueble, coincidente con el baño superior de la vivienda de la demandada.\r\nPatrón Descendente: El patrón de humedad es descendente y localizado, lo que sugiere un origen en la instalación privativa del cuarto de baño superior.\r\nCorrespondencia entre Daños y Origen Señalado: Existe una correspondencia clara entre los daños detectados y el origen señalado por el perito del demandante.\r\n\r\nConclusiones\r\n\r\nSe concluye que la fuga de agua procedente del cuarto de baño superior de la vivienda de la demandada es el origen más probable de las humedades y otros daños en la vivienda del demandante. Se recomienda una reparación inmediata de la avería para evitar un deterioro aún mayor.", true, 500, false, false);

        docManager.CreateDocument("Conversación de whatsapp", DocumentType.Report, "Se adjunta una conversación de whatsapp donde Ana ignora las advertencias de que le están formando zonas húmedas en el techo. Pedro advierte varias ocasiones y no recibe respuesta.", true, 0, false, false);

        docManager.CreateDocument("Factura de reparación de daños", DocumentType.ReceiptFacture, "Se adjunta un presupuesto de la reparación de los daños causados por humedad que son 15000 euros.", true, 0, false, false);

        docManager.CreateDocument("Testimonio de vecinos sobre la actitud de Ana", DocumentType.Witness, "Yo, Fran Bernabé, soy vecino del edificio, y rara vez he visto a Ana ser amable o saludar a vecinos. Además hace mucho que no la veo en juntas de comunidad de vecinos, se podría decir que nos tiene olvidados.", true, 0, false, false);
    }


    /// <summary>
    /// Cheat para pasar a la fase 2 con una cantidad de dinero pasada
    /// </summary>
    /// <param name="text"></param>
    public void Phase1toPhase2(string text)
    {
        BudgetSystem.Instance.SetBudgetFromPhase1(text, int.Parse(text));
        SceneSystem.Instance.LoadPhase2();
    }

    /// <summary>
    /// Para poder tomar la referencia del menu de cheats
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _cheatMenu = GameObject.FindGameObjectWithTag("CheatMenu");

        if (_cheatMenu != null)
        {
            _cheatMenu.SetActive(false);
        }
    }

    /// <summary>
    /// togglea el menu de cheats
    /// </summary>
    private void toggleCheatsMenu()
    {
        if (_cheatMenu != null)
        {
            _cheatMenu.SetActive(!_cheatMenu.activeSelf);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += onSceneLoaded;
        InputSystem.Instance.cheatMenuPerformed += toggleCheatsMenu;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= onSceneLoaded;

        if (InputSystem.Instance != null)
            InputSystem.Instance.cheatMenuPerformed -= toggleCheatsMenu;
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

}
