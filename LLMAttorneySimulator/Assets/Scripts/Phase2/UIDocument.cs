using System;
using TMPro;
using Unity.VisualScripting;
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

    // Para que no dependa del estado activo del padre, se asigna como hijo de otro al abrir y se reasigna al cerrar
    [SerializeField] private Transform _detachedParent;
    private Transform _originalParent;
    [SerializeField]
    private GameObject _documentIcon;


    private bool _open = false;

    private GameObject _placeholder;





    /// <summary>
    /// Llamado al pulsar el documento para abrir la ventana con su contenido. Ademas cambia el sprite,
    /// asigna un nuevo padre para que no dependa del estado activo del padre original y muestra el contenido del documento
    /// </summary>
    public void OnOpenDocument()
    {
        if (!_open)
        {
            _open = true;
            _originalParent = transform.parent;

            RectTransform rt = GetComponent<RectTransform>();
            Vector2 worldPos = rt.position; // screen/world space, stable across parents

            _placeholder = new GameObject("DocPlaceholder", typeof(RectTransform), typeof(LayoutElement));
            _placeholder.transform.SetParent(_originalParent, false);
            _placeholder.transform.SetSiblingIndex(rt.GetSiblingIndex());


            RectTransform placeholderRt = _placeholder.GetComponent<RectTransform>();
            placeholderRt.sizeDelta = rt.sizeDelta;
            
            LayoutElement le = _placeholder.GetComponent<LayoutElement>();
            le.preferredWidth = rt.sizeDelta.x;
            le.preferredHeight = rt.sizeDelta.y;

            transform.SetParent(_detachedParent, true); // worldPositionStays = true
            rt.position = worldPos;

            _documentImageReference.sprite = _openDocumentSprite;
            _placeholder.AddComponent<Image>();
            _placeholder.GetComponent<Image>().sprite = _documentIcon.GetComponent<Image>().sprite;
            _placeholder.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f);
            _documentContentHolder.SetActive(true);
            _documentIcon.SetActive(false);

        }
    }

    /// <summary>
    /// Llamado al pulsar la cruz de la ventana de contenido del documento para cerrarla. Ademas cambia el sprite,
    /// reestablece el padre original y oculta el contenido del documento
    /// </summary>
    public void OnExitDocument()
    {
        if (_open)
        {
            _open = false;

            // Restore to placeholder's position, then destroy it
            if (_placeholder != null)
            {
                transform.SetParent(_originalParent, false);
                transform.SetSiblingIndex(_placeholder.transform.GetSiblingIndex());
                Destroy(_placeholder);
                _placeholder = null;
            }
            else
            {
                transform.SetParent(_originalParent, false);
            }

            _documentImageReference.sprite = _closedDocumentSprite;
            _documentContentHolder.SetActive(false);
            _documentIcon.SetActive(true);
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
        if (_detachedParent == null)
            _detachedParent = GameObject.FindWithTag("DetachedDocParent")?.transform;
        _documentContentHolder.SetActive(false);
    }
}
