using Mono.Cecil.Cil;
using System.Collections.Generic;
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

        GameObject instanciatedUIDoc = Instantiate(_playerDocumentPrefab, parent.transform);
        instanciatedUIDoc.GetComponent<UIDocument>().SetDocValues(doc.GetDocName(), doc.GetContent(), doc.IsSentToProcurador());
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
    }

    public override void Close()
    {
        _pageHolder.SetActive(false);
    }
}