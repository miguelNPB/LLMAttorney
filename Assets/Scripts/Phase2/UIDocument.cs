using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase para representar visualmente en la pagina de los documentos un docuemnto
/// </summary>
public class UIDocument : MonoBehaviour
{
    [SerializeField] private Sprite _closedDocumentSprite;
    [SerializeField] private Sprite _openDocumentSprite;

    [SerializeField] private GameObject _openDocumentButton;
    [SerializeField] private GameObject _closeDocumentButton;

    [SerializeField] private TMP_Text _textDocName;
    [SerializeField] private TMP_Text _textContent;

    [SerializeField] private GameObject _documentContentHolder;
    [SerializeField] private GameObject _sentToProcuradorVisualFeedback;
    [SerializeField] private Image _documentImageReference;


    private bool _open = false;

    /// <summary>
    /// Llamado al pulsar el documento para abrir la ventana con su contenido. Ademas cambia el sprite
    /// </summary>
    public void OnOpenDocument()
    {
        if (!_open)
        {
            _open = true;
            _documentImageReference.sprite = _openDocumentSprite;
            _documentContentHolder.SetActive(true);
        }
    }
    /// <summary>
    /// Llamado al pulsar la cruz de la ventana de contenido del documento para cerrarla. Ademas cambia el sprite
    /// </summary>
    public void OnExitDocument()
    {
        if (_open)
        {
            _open = false;
            _documentImageReference.sprite = _closedDocumentSprite;
            _documentContentHolder.SetActive(false);
        }
    }

    /// <summary>
    /// Llamar para inicializar los visuales del documento UI
    /// </summary>
    /// <param name="docName"></param>
    /// <param name="docContent"></param>
    /// <param name="sentToProcurador"></param>
    public void SetDocValues(string docName, string docContent, bool sentToProcurador)
    {
        _textDocName.text = docName;
        _textContent.text = docContent;
        _sentToProcuradorVisualFeedback.SetActive(sentToProcurador);
    }

    void Start()
    {
        _documentContentHolder.SetActive(false);
    }
}
