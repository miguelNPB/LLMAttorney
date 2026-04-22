using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using Mono.Cecil.Cil;

public class Phase3Manager : MonoBehaviour
{
    [Header("Prior hearing settings")]
    [Tooltip("Probabilidad de que el rival recurra a una prueba del player 1=siempre, 0=nunca")]
    [SerializeField][Range(0f, 1f)] private float _rivalObjectionChance = 0.5f;

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
    [Tooltip("Texto que dice el juez despues de la presentacion de pruebas para finalizar la fase 3")]
    [SerializeField] private List<string> _judgeEndingSpeech;
    [Tooltip("Texto que dice el juez para empezar la fase de presentacion de documentos")]
    [SerializeField] private string _judgeStartPresentRivalDocumentsSpeech;
    [Tooltip("Texto que dice el juez para empezar la fase de presentacion de documentos")]
    [SerializeField] private string _judgeStartPresentPlayerDocumentsSpeech;
    [Tooltip("Texto que dice el juez para presentar pruebas. El simbolo $ se sustituira con el nombre del documento. El simbolo @ se sustituira con el tipo de documento")]
    [SerializeField] private string _judgePresentDocumentSpeech;
    [Tooltip("Texto que dice el juez para aceptar una recursion")]
    [SerializeField] private string _judgeRejectObjectionSpeech;
    [Tooltip("Texto que dice el juez para rechazar una recursion")]
    [SerializeField] private string _judgeAcceptObjectionSpeech;

    [SerializeField] private string _playerObjectionSpeech;
    [SerializeField] private string _rivalObjectionSpeech;

    [Header("References")]
    [SerializeField] private Phase3WrittingHandler _writtingHandler;
    [SerializeField] private JudgePatienceSystem _judgePatienceSystem;
    [SerializeField] private GameObject _startPhase3Popup;
    [SerializeField] private GameObject _objectionButton;
    [SerializeField] private Animator _judgeAnimator;
    [SerializeField] private LLMConnectorRivalObjection _llmConnectorRivalObjection;


    // documentos iniciales al entrar a la fase disponbiles de cada parte
    private List<Document> _clientDocuments;
    private List<Document> _rivalDocuments;
    // output de documentos al veredicto final
    private List<Document> _clientOutputDocuments;
    private List<Document> _rivalOutputDocuments;

    private bool _objection = false;
    private bool _acceptedRivalObjection = false;
    private bool _recievedPromptAnswer = false;

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
        _writtingHandler.ToggleSpeakingBubble(true);
        //yield return initialJudgeSpeech();

        _judgePatienceSystem.TogglePatienceVisual(true);
        yield return presentDocumentsPhase();

        _judgePatienceSystem.TogglePatienceVisual(false);
        yield return endingJudgeSpeech();

        // ir a la fase 4
    }

    /// <summary>
    /// Coroutine que escribe el dialogo inicial del juez
    /// </summary>
    /// <returns></returns>
    private IEnumerator initialJudgeSpeech()
    {
        // poner nombres en el texto
        _judgeInitialSpeech[0] += GameSystem.Instance.CaseData.clientName + " y " + GameSystem.Instance.CaseData.demandedEntityName + ".";

        for (int i = 0; i < _judgeInitialSpeech.Count; i++)
        {
            yield return _writtingHandler.SpeakJudge(_judgeInitialSpeech[i]);
            yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));
        }
    }

    /// <summary>
    /// Coroutine que escribe el dialogo final del juez
    /// </summary>
    /// <returns></returns>
    private IEnumerator endingJudgeSpeech()
    {
        // poner nombres en el texto
        _judgeEndingSpeech[0] += GameSystem.Instance.CaseData.clientName + " y " + GameSystem.Instance.CaseData.demandedEntityName + ".";

        for (int i = 0; i < _judgeEndingSpeech.Count; i++)
        {
            yield return _writtingHandler.SpeakJudge(_judgeEndingSpeech[i]);
            yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));
        }
    }


    /// <summary>
    /// Coroutine para la presentacion de pruebas de ambas partes
    /// </summary>
    /// <returns></returns>
    private IEnumerator presentDocumentsPhase()
    {
        yield return _writtingHandler.SpeakJudge(_judgeStartPresentRivalDocumentsSpeech);
        yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));

        // parte demandada (documentos del rival)
        for (int i = 0; i < _rivalDocuments.Count; i++)
        {
            _objectionButton.SetActive(false);

            // presentar prueba
            string oldSpeech = _judgePresentDocumentSpeech;
            _judgePresentDocumentSpeech = _judgePresentDocumentSpeech.Replace("$", _rivalDocuments[i].GetDocName());
            _judgePresentDocumentSpeech = _judgePresentDocumentSpeech.Replace("@", _rivalDocuments[i].GetDocType().ToString());
            yield return _writtingHandler.SpeakJudge(_judgePresentDocumentSpeech);

            // habilitar recurrimiento
            _objection = false;
            _objectionButton.SetActive(true);
            float timer = 0;
            float waitingObjectionTime = Random.Range(_minTimeBetweenDocuments, _maxTimeBetweenDocuments);
            while (!_objection && timer < waitingObjectionTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _objectionButton.SetActive(false);

            if (_objection)
            {
                yield return _writtingHandler.SpeakPlayer(_playerObjectionSpeech);

                // simulamos que el juez piensa
                _judgeAnimator.SetTrigger("Thinking");
                yield return new WaitForSeconds(5f);

                // rollear paciencia
                bool notRejectionDueToLowPatience = _judgePatienceSystem.RollPatience();
                if (!notRejectionDueToLowPatience)
                    _judgePatienceSystem.PlayRejectAnimation();

                // rechazar
                if (!notRejectionDueToLowPatience || _rivalDocuments[i].IsValid())
                {
                    _judgeAnimator.SetTrigger("Reject");
                    _judgePatienceSystem.DecrementPatience();
                    yield return _writtingHandler.SpeakJudge(_judgeRejectObjectionSpeech);
                    yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));

                    _rivalOutputDocuments.Add(_rivalDocuments[i]);
                }
                else // aceptar
                {
                    _judgeAnimator.SetTrigger("Accept");
                    yield return _writtingHandler.SpeakJudge(_judgeAcceptObjectionSpeech);
                    yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));
                }
            }
            else
                _rivalOutputDocuments.Add(_rivalDocuments[i]);

            _judgePresentDocumentSpeech = oldSpeech;
        }



        _objectionButton.SetActive(false);
        yield return _writtingHandler.SpeakJudge(_judgeStartPresentPlayerDocumentsSpeech);
        yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));




        // parte demandadora (documentos del player)
        for (int i = 0; i < _clientDocuments.Count; i++)
        {
            // presentar prueba
            string oldSpeech = _judgePresentDocumentSpeech;
            _judgePresentDocumentSpeech = _judgePresentDocumentSpeech.Replace("$", _rivalDocuments[i].GetDocName());
            _judgePresentDocumentSpeech = _judgePresentDocumentSpeech.Replace("@", _rivalDocuments[i].GetDocType().ToString());
            yield return _writtingHandler.SpeakJudge(_judgePresentDocumentSpeech);
            
            // rollear recurrimiento del rival
            bool _rivalObjects = Random.Range(0f,1f) <= _rivalObjectionChance;

            bool objected = false;
            float timer = 0;
            float waitingObjectionTime = Random.Range(_minTimeBetweenDocuments, _maxTimeBetweenDocuments);
            float rivalTimeWaitingToObject = Random.Range(_minTimeBetweenDocuments, waitingObjectionTime);
            while (!objected && timer < waitingObjectionTime)
            {
                if (_rivalObjects && timer >= rivalTimeWaitingToObject)
                {
                    objected = true;
                    yield return _writtingHandler.SpeakRival(_rivalObjectionSpeech);

                    // prompt si la prueba es valida y si se rechaza por paciencia
                    bool notRejectionDueToLowPatience = _judgePatienceSystem.RollPatience();
                    yield return promptIsObjectionValid();

                    // rechazado
                    if (_acceptedRivalObjection || !notRejectionDueToLowPatience)
                    {
                        _judgeAnimator.SetTrigger("Reject");
                        yield return _writtingHandler.SpeakJudge(_judgeAcceptObjectionSpeech);
                        yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));
                    }
                    else // aceptado
                    {
                        _judgeAnimator.SetTrigger("Accept");
                        yield return _writtingHandler.SpeakJudge(_judgeRejectObjectionSpeech);
                        yield return new WaitForSeconds(Random.Range(_minTimeBetweenTexts, _maxTimeBetweenTexts));
                    }

                    if (!notRejectionDueToLowPatience)
                        _judgePatienceSystem.PlayRejectAnimation();
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (!objected || (objected && !_acceptedRivalObjection))
                _clientOutputDocuments.Add(_clientDocuments[i]);
        }
    }

    /// <summary>
    /// Funcion que recibe la respuesta del LLM sobre si la recursion del rival es valida
    /// </summary>
    /// <param name="valid"></param>
    private void receivePromptAnswer(bool valid)
    {
        _recievedPromptAnswer = true;
        _acceptedRivalObjection = valid;
    }

    /// <summary>
    /// Coroutina para promptear si la recursion del rival es valida
    /// </summary>
    /// <returns></returns>
    private IEnumerator promptIsObjectionValid()
    {
        _acceptedRivalObjection = false;

        _llmConnectorRivalObjection.SendPrompt(receivePromptAnswer);

        while (!_recievedPromptAnswer)
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
        GameSystem.Instance.myDocumentManager.AddDocumentAUX("test2", PromptType.Perito, "testcontent", false, 50, true);
        GameSystem.Instance.myDocumentManager.AddDocumentAUX("test3", PromptType.Report, "testcontent", true, 50, true);
        GameSystem.Instance.myDocumentManager.AddDocumentAUX("PLAYERtest1", PromptType.Report, "testcontent", true, 50, false);
        GameSystem.Instance.myDocumentManager.AddDocumentAUX("PLAYERtest2", PromptType.Perito, "testcontent", false, 50, false);
        GameSystem.Instance.myDocumentManager.AddDocumentAUX("PLAYERtest3", PromptType.Report, "testcontent", true, 50, false);
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
