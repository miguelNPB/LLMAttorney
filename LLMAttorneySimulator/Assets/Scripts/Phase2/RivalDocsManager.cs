using UnityEngine;


/// <summary>
/// Genera documentos del rival cada cierto tiempo despues de haber mandado la demanda
/// </summary>
public class RivalDocsManager : MonoBehaviour
{
    [Tooltip("Seconds between automatic opponent docs (min)")]
    [SerializeField] private float _minIntervalSeconds = 60f;

    [Tooltip("Seconds between automatic opponent docs (max)")]
    [SerializeField] private float _maxIntervalSeconds = 120f;

    [Tooltip("Numero de documentos generados.")]
    [SerializeField] private int _maxStartingDocs = 5;
    [SerializeField] private int _minStartingDocs = 5;

    [SerializeField] private Phase2Manager _phase2Manager;
    [SerializeField] private ComputerSystem _computerSystem;
    [SerializeField] private DocumentGenerationManager _documentGenerationManager;
    [SerializeField] private ProcuratorChatPage _procuradorPage;
    [SerializeField] private PriorHearingPage _priorHearingPage;

    // numero alto para que segun se mande la demanda empiece a generar uno
    private float _timer = 999f; 

    private float _nextInterval;

    private int _maxDocsGenerated;
    private int _numDocsGenerated = 0;
    private bool _generating = false;

    /// <summary>
    /// acutaliza el tiempo entre documentos y genera uno cada intervalo hasta el limite de maxDocsGenerated
    /// </summary>
    private void updateRivalDocGeneration()
    {
        if (_generating || _numDocsGenerated >= _maxDocsGenerated)
            return;

        _timer += Time.deltaTime;
        if (_timer >= _nextInterval)
        {
            _timer = 0f;
            _nextInterval = Random.Range(_minIntervalSeconds, _maxIntervalSeconds);
            generateDocument();
        }
    }

    /// <summary>
    /// Genera un documento 
    /// </summary>
    private void generateDocument()
    {
        _generating = true;
        _numDocsGenerated++;

        // 50 / 50 valid o invalid
        bool isValid = 0.5f < Random.Range(0f, 1f);
        DocumentType docType = (DocumentType)Random.Range(0, 5);

        _documentGenerationManager.PromptGenerateDocument(recieveDocument, recieveError, "", docType, true, isValid);
    }

    /// <summary>
    /// Recibe el documento generado del rival
    /// </summary>
    /// <param name="docTitle"></param>
    /// <param name="docContent"></param>
    /// <param name="documentType"></param>
    /// <param name="cost"></param>
    /// <param name="isPlayer"></param>
    /// <param name="isValid"></param>
    private void recieveDocument(string docTitle, string docContent, DocumentType documentType, int cost, bool isOpponent, bool isValid)
    {
        _generating = false;

        GameSystem.Instance.CaseData.documentManager.CreateDocument(docTitle, documentType, docContent, isValid, 0, true, true);

        _procuradorPage.StartPendingOpponentMessage();

        string response = "";
        switch (documentType)
        {
            case DocumentType.Perito:
                response = "Has recibido el siguiente informe pericial: ";
                break;
            case DocumentType.Report:
                response = "Has recibido el siguiente informe: ";
                break;
            case DocumentType.Witness:
                response = "Has recibido el siguiente documento con un testimonio: ";
                break;
            case DocumentType.ReceiptFacture:
                response = "Has recibido el siguiente recibo: ";
                break;
        }

        response += docTitle + " de la parte del demandado.";

        if (_numDocsGenerated >= _maxDocsGenerated)
        {
            response += ". Con este último documento ya están todos los documentos del rival, voy avisando al tribunal para ir agendando la audiencia previa.";
            _computerSystem.ToggleNotification(Page.PriorHearing, true);

            _priorHearingPage.AllRivalDocsRecieved();

            _computerSystem.PingOverlayNotification("¡Ya estas listo para la audiencia previa!");
        }

        _procuradorPage.ReceiveOpponentDocMessage(response);
    }

    /// <summary>
    /// Llamado al recibir un error al contactar con el modelo
    /// </summary>
    /// <param name="text"></param>
    private void recieveError(string text)
    {
        _procuradorPage.StartPendingOpponentMessage();
        _procuradorPage.ReceiveOpponentDocMessage("Error generando documento del rival");
    }

    private void Start()
    {
        _numDocsGenerated = 0;
        _maxDocsGenerated = Random.Range(_minStartingDocs, _maxStartingDocs);
    }

    private void Update()
    {
        if (GameSystem.Instance.CaseData.isDemandaSent && _numDocsGenerated < _maxDocsGenerated)
            updateRivalDocGeneration();
    }
}