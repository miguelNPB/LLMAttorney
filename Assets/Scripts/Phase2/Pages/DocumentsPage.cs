using Mono.Cecil.Cil;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pagina para gestionar el visualizado y registro de los documentos del caso, tanto del player como del rival
/// </summary>
public class DocumentsPage : IPage
{
    [Header("References")]
    [SerializeField] private GameObject _pageHolder;
    [SerializeField] private GameObject _playerDocumentsContainer;
    [SerializeField] private GameObject _rivalDocumentsContainer;
    [SerializeField] private GameObject _playerDocumentPrefab;

    private DocumentManager _documentManager;
    private List<UIDocument> _playerDocumentsInstanciated = new List<UIDocument>();
    private List<UIDocument> _rivalDocumentsInstanciated = new List<UIDocument>();


    //Padre de los documentos abiertos
    [SerializeField] 
    private Transform _detachedDocParent;

    void Awake()
    {
        _documentManager = GameSystem.Instance.CaseData.documentManager;
    }

    /// <summary>
    /// Instancia un uidocument 
    /// </summary>
    /// <param name="doc"></param>
    private void addUIDocument(Document doc)
    {
        GameObject parent = doc.IsRivalDoc() ? _rivalDocumentsContainer : _playerDocumentsContainer;
        GameObject instance = Instantiate(_playerDocumentPrefab, parent.transform);
        UIDocument uiDoc = instance.GetComponent<UIDocument>();
        uiDoc.SetDocValues(doc.GetDocName(), doc.GetContent(), doc.IsSentToProcurador());

        if (!doc.IsRivalDoc())
        {
            _playerDocumentsInstanciated.Add(uiDoc);
        }
        else
        {
            _rivalDocumentsInstanciated.Add(uiDoc);
        }
        
    }

    /// <summary>
    /// Actualiza un UIDocument ya instanciado
    /// </summary>
    /// <param name="uiDoc"></param>
    /// <param name="doc"></param>
    private void updateUIDocument(UIDocument uiDoc, Document doc)
    {
        uiDoc.SetDocValues(doc.GetDocName(), doc.GetContent(), doc.IsSentToProcurador());
    }

    /// <summary>
    /// Inicializa los valores de los documentos para mostrarlos como ficheros
    /// </summary>
    private void setupUIDocuments()
    {
        // setup la seccion del player
        List<uint> playerDocuments = _documentManager.GetPlayerDocs();
        for (int i = 0; i < playerDocuments.Count; i++)
        {

            if (i < _playerDocumentsInstanciated.Count) {
                updateUIDocument(_playerDocumentsInstanciated[i], _documentManager.GetDocument(playerDocuments[i]));
            }
            else{
                addUIDocument(_documentManager.GetDocument(playerDocuments[i]));
            }
        }

        // setup la seccion del rival
        List<uint> rivalDocuments = _documentManager.GetRivalDocs();
        for (int i = 0; i < rivalDocuments.Count; i++)
        {
            if (i < _rivalDocumentsInstanciated.Count)
            {
                updateUIDocument(_rivalDocumentsInstanciated[i], _documentManager.GetDocument(rivalDocuments[i]));
            }
            else
            {
                addUIDocument(_documentManager.GetDocument(rivalDocuments[i]));
            }
        }
    }

    public override void Open()
    {
        setupUIDocuments();

        _pageHolder.SetActive(true);

        // foreach (UIDocument doc in _detachedDocParent.GetComponentsInChildren<UIDocument>(includeInactive: true))
        //     doc.documentIcon.SetActive(true);
    }

    public override void Close()
    {
        _pageHolder.SetActive(false);
        // Esconder iconos de los documentos abiertos en el detached parent para evitar que sigan visibles al cerrar la pagina

        // foreach (UIDocument doc in _detachedDocParent.GetComponentsInChildren<UIDocument>(includeInactive: true))
        //     doc.documentIcon.SetActive(true);
    }
    
    void Start()
    {
        if (_detachedDocParent == null)
            _detachedDocParent = GameObject.FindWithTag("DetachedDocParent")?.transform;
    }
}