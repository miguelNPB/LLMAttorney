using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OpponentDocProcuration : MonoBehaviour
{
    [Serializable]
    private class OpponentProcResponse
    {
        public List<string> validDocuments;
        public List<string> invalidDocuments;
    }

    [Header("Config")]
    [Tooltip("Seconds between automatic opponent docs (min)")]
    public float minIntervalSeconds = 60f;

    [Tooltip("Seconds between automatic opponent docs (max)")]
    public float maxIntervalSeconds = 180f;

    [Tooltip("Numero de documentos generados.")]
    public int maxStartingDocs = 5;

    [Tooltip("Tematicas que puede utilizar para generar documentos validos")]
    public List<string> docValidThemes = new List<string>();

    [Tooltip("Tematicas que puede utilizar para generar documentos NO validos")]
    public List<string> docInvalidThemes = new List<string>();

    [Header("References")]
    [SerializeField] private LLMConnectorOpponentDocuments _llmConnectorOpponentDocGeneration;
    [SerializeField] private LLMConnectorOpponentDocList _llmConnectorOpponentDocList;
    [SerializeField] private ConfigLLMInfo _generateDocumentsListConfig;


    [SerializeField] private ProcuratorChatPage _procuradorPage;


    private float _timer = 0f;
    private float _nextInterval;
    private bool _ready = false;

    private void Start()
    {
        StartCoroutine(Init());
    }

    private void Update()
    {
        if (!_ready || docInvalidThemes.Count == 0 || docInvalidThemes.Count == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= _nextInterval)
        {
            _timer = 0f;
            _nextInterval = UnityEngine.Random.Range(minIntervalSeconds, maxIntervalSeconds);
            //GenerateDocument(BuildTimedDocumentPrompt());
        }
    }

    public void OnDocumentGenerated(Document playerDoc)
    {
        string prompt =
            $"El abogado contrario ha presentado el documento:\n {playerDoc.GetDocName()} \n{playerDoc.GetContent()}" +
            $"Genera un documento que lo contradiga o debilite sus argumentos."+
            $"No menciones el documento del jugador, pero puedes basarte en su contenido para refutarlo";

        GenerateDocument(prompt);
    }

    /*
    /// <summary>
    /// Genera la lista de documentos validos y no validos
    /// </summary>
    /// <returns></returns>
    private IEnumerator GenerateDocumentsList()
    {
        JsonSchema schema = new JsonSchema();
        schema.properties.Add("answer", new PropertyInfo(JsonDataType.Array));

        string caseDesc = GameSystem.Instance.CaseData.caseDescription;
        string prompt = "El resumen del caso es el siguiente: " + caseDesc + "Responde con una lista de 10 documentos validos en el campo \"validDocuments\" y una lista de 10 documentos invalidos en el campo \"invalidDocuments\"";

        _llmConnector.oppPrompt = prompt;
        _llmConnector.CallSendContext();

        yield return LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveDocumentList, prompt, configLLM, schema);
    }

    public void RecieveDocumentList(bool success, string answer)
    {
        OpponentProcResponse wrapper = JsonUtility.FromJson<OpponentProcResponse>(answer);
        config.docInvalidThemes = wrapper.invalidDocuments;
        config.docValidThemes = wrapper.validDocuments;

        _ready = true;
    }
    */

    private string BuildTimedDocumentPrompt(bool valid)
    {
        string themes = "";

        if (valid)
        {
            int aux = UnityEngine.Random.Range(0, docValidThemes.Count);
            themes = docValidThemes[aux];
            docValidThemes.RemoveAt(aux);
        }
        else
        {
            int aux = UnityEngine.Random.Range(0, docInvalidThemes.Count);
            themes = docInvalidThemes[aux];
            docInvalidThemes.RemoveAt(aux);
        }

        string prompt = $"Genera un documento con titulo [{themes}] de parte contraria.";

        if (valid)
            prompt += "El documento debe ser VALIDO, es decir, que en caso de ser recurrido en una audiencia previa, no se puede desestimar y se debe consdierar para el caso";
        else
            prompt += "El documento debe ser INVALIDO, es decir, que en caso de ser recurrido en una audiencia previa, se debe desestimar y no considerarse para el caso";

        return prompt;
    }

    private void GenerateDocument(string prompt)
    {

    }

    private IEnumerator Init()
    {
        yield return new WaitWhile(() => !GameSystem.Instance.CaseData.isDemandaSent);


        bool relevant = true;
        GameSystem.Instance.CaseData.documentManager.CreateDocument("Factura de reparación de tuberías 2012", DocumentType.ReceiptFacture, "Se adjunta una factura de una reparación integral de todas las tuberías a causa de un reventón por frio de unas tuberías. Se sustituyeron todas las tuberías antiguas por unas nuevas en toda la casa.", relevant, 0, true, true);
        GameSystem.Instance.CaseData.documentManager.CreateDocument("Informe del origen de la fuga de agua", DocumentType.Report, "Se ha realizado una investigación y no se puede determinar el origen concreto de la fuga de agua a la casa de Pedro. Dado que la zona afectada es tan grande, pasa por zonas de tuberías de la comunidad como por zonas de tuberías de la casa de Ana, por lo que no hay pruebas concluyentes de que la fuga provenga de una tubería de Ana.", relevant, 0, true, true);
        relevant = false;
        GameSystem.Instance.CaseData.documentManager.CreateDocument("Testimonio de Juan Pérez", DocumentType.Witness, "Yo, Juan Pérez, estuve en casa de mi prima Ana el pasado fin de semana. Miré por encima el baño y no vi ninguna fuga. Las tuberías se ven secas. Creo que el problema de abajo es porque el edificio es viejo y las bajantes de la comunidad están mal.", relevant, 0, true, true);
        GameSystem.Instance.CaseData.documentManager.CreateDocument("Conversación de whatsapp", DocumentType.Report, "Se adjunta una conversación de whatsapp donde Ana habla con otra vecina donde la vecina se queja de que Pedro es un exagerado y suele decir que las cosas son mas grandes de las que son, y que seguramente quiere que Ana le pague los daños pero para pintarse la casa gratis.", relevant, 0, true, true);


        _procuradorPage.StartPendingOpponentMessage();
        _procuradorPage.ReceiveOpponentDocMessage("Has recibido un documento " + "Factura de reparación de tuberías 2012" + " de la parte del damandado.");

        yield return null;
        yield return null;

        _procuradorPage.StartPendingOpponentMessage();
        _procuradorPage.ReceiveOpponentDocMessage("Has recibido un documento " + "Informe del origen de la fuga de agua" + " de la parte del damandado.");
        yield return null;
        yield return null;
        _procuradorPage.StartPendingOpponentMessage();
        _procuradorPage.ReceiveOpponentDocMessage   ("Has recibido un documento " + "Testimonio de Juan Pérez" + " de la parte del damandado.");
        yield return null;
        yield return null;

        _procuradorPage.StartPendingOpponentMessage();
        _procuradorPage.ReceiveOpponentDocMessage("Has recibido un documento " + "Conversación de whatsapp" + " de la parte del damandado.");
        yield return null;
        yield return null;

        //yield return StartCoroutine(GenerateDocumentsList());
        //_nextInterval = UnityEngine.Random.Range(minIntervalSeconds, maxIntervalSeconds);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}