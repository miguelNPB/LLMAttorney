using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class Phase3Manager : MonoBehaviour
{
    [Header("Prior hearing settings")]
    [Tooltip("Tiempo minimo entre que secciones enteras de texto en la caja de texto")]
    [SerializeField] private float _minTimeBetweenTexts;
    [Tooltip("Tiempo maximo entre que secciones enteras de texto en la caja de texto")]
    [SerializeField] private float _maxTimeBetweenTexts;
    [Tooltip("Tiempo minimo entre que se presenta un documento, se espera a recursiones, y se pasa al siguiente")]
    [SerializeField] private float _minTimeBetweenDocuments;
    [Tooltip("Tiempo maximo entre que se presenta un documento, se espera a recursiones, y se pasa al siguiente")]
    [SerializeField] private float _maxTimeBetweenDocuments;

    [Tooltip("Texto que dice el juez al empezar la fase 3")]
    [SerializeField] private List<string> _judgeInitialSpeech;
    [Tooltip("Texto que dice el juez para empezar la fase de presentacion de documentos")]
    [SerializeField] private string _judgeStartPresentDocumentPhaseSpeech;
    [Tooltip("Texto que dice el juez para presentar pruebas. El simbolo $ se sustituira con el nombre del documento. El simbolo @ se sustituira con el tipo de documento")]
    [SerializeField] private string _judgePresentDocumentSpeech;
    [Tooltip("Texto que dice el juez despues de la presentacion de pruebas para finalizar la fase 3")]
    [SerializeField] private string _judgeEndPhase3Speech;

    [Header("References")]
    [SerializeField] private Phase3WrittingHandler _writtingHandler;
    [SerializeField] private GameObject _startPhase3Popup;
    [SerializeField] private GameObject _objectionButton;
    [SerializeField] private JudgePatienceSystem _judgePatienceSystem;


    // documentos iniciales al entrar a la fase disponbiles de cada parte
    private List<Document> _clientDocuments;
    private List<Document> _rivalDocuments;
    // output de documentos al veredicto final
    private List<Document> _clientOutputDocuments;
    private List<Document> _rivalOutputDocuments;

    private bool _objection = false;

    public void Objection()
    {
        _objection = true;
    }


    /// <summary>
    /// Metodo llamado al pulsar el boton de start al llegar a la fase 3
    /// </summary>
    public void StartPhase3()
    {
        _startPhase3Popup.SetActive(false);

        StartCoroutine(phase3Coroutine());
    }



    /// <summary>
    /// Coroutina principal de la phase 3
    /// </summary>
    /// <returns></returns>
    private IEnumerator phase3Coroutine()
    {
        yield return initialJudgeSpeech();

        _judgePatienceSystem.TogglePatienceVisual(true);

        yield return presentDocumentsPhase();
    }

    /// <summary>
    /// Coroutine que escribe el dialogo inicial del juez
    /// </summary>
    /// <returns></returns>
    private IEnumerator initialJudgeSpeech()
    {
        // poner nombres en el texto
        _judgeInitialSpeech[0] += GameSystem.Instance.CaseData.clientName + " y " + GameSystem.Instance.CaseData.demandedEntityName + ".";
        _writtingHandler.ToggleSpeakingBubble(true);

        for (int i = 0; i < _judgeInitialSpeech.Count; i++)
        {
            yield return _writtingHandler.SpeakJudge(_judgeInitialSpeech[i]);
            yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));
        }
    }

    /// <summary>
    /// Coroutine para la presentacion de pruebas de ambas partes
    /// </summary>
    /// <returns></returns>
    private IEnumerator presentDocumentsPhase()
    {
        yield return _writtingHandler.SpeakJudge(_judgeStartPresentDocumentPhaseSpeech);
        yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));

        // parte demandada
        for (int i = 0; i < _rivalDocuments.Count; i++)
        {
            _objectionButton.SetActive(false);

            // presentar prueba
            string oldSpeech = _judgePresentDocumentSpeech;
            _judgePresentDocumentSpeech = _judgePresentDocumentSpeech.Replace("$", _rivalDocuments[i].GetDocName());
            yield return _writtingHandler.SpeakJudge(_judgePresentDocumentSpeech);

            // habilitar recurrimiento
            _objection = false;
            _objectionButton.SetActive(true);
            float timer = 0;
            float waitingObjectionTime = Random.Range(_minTimeBetweenDocuments, _maxTimeBetweenDocuments);
            while (!_objection && timer < waitingObjectionTime)
            {
                timer += Time.deltaTime;
            }
            _objectionButton.SetActive(false);

            if (_objection)
            {
                yield return _writtingHandler.SpeakPlayer("OBJETO!");
            }

            _judgePresentDocumentSpeech = oldSpeech;
            
        }

        yield return null;
    }

    /// <summary>
    /// Inicializa las listas de documentos con los documentos validos en la fase de audiencia previa
    /// </summary>
    private void initDocumentsLists()
    {
        _clientDocuments = new List<Document>();
        _rivalDocuments = new List<Document>();

        // meter solo los documentos que hayan sido mandados al procurador
        foreach (Document doc in GameSystem.Instance.myDocumentManager.documents)
        {
            if (doc.IsOpponentDoc())
                _rivalDocuments.Add(doc);
            else if (!doc.IsOpponentDoc() && doc.IsSentToProcurador())
                _clientDocuments.Add(doc);
        }
    }

    private void Start()
    {
        // TEST DOCUMENTS
        GameSystem.Instance.myDocumentManager.AddDocumentAUX("test1", PromptType.Report, "testcontent", true, 50, true);
        GameSystem.Instance.myDocumentManager.AddDocumentAUX("test2", PromptType.Perito, "testcontent", true, 50, true);
        GameSystem.Instance.myDocumentManager.AddDocumentAUX("test3", PromptType.Report, "testcontent", true, 50, true);
        //

        _writtingHandler.ToggleSpeakingBubble(false);
        _judgePatienceSystem.TogglePatienceVisual(false);
        _objectionButton.SetActive(false);
        initDocumentsLists();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
