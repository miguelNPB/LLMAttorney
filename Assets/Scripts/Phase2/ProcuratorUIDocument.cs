using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Elemento de la UI para la pagina de procuradro para representar documentos
/// </summary>
public class ProcuratorUIDocument : MonoBehaviour
{
    private ProcuratorChatPage _procuradorMessagesPage;

    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _docNameText;
    [SerializeField] private GameObject _tickSent;
    [SerializeField] private GameObject _selected;

    [HideInInspector] private Document _documentInfo; public Document documentInfo { get { return _documentInfo; } }

    private DocumentManager _docManager;
    public void Unselect()
    {
        _selected.SetActive(false);
    }

    public void Select()
    {
        _selected.SetActive(true);

        _procuradorMessagesPage.SelectDocument(this);
    }

    public void Awake()
    {
        _docManager = GameSystem.Instance.myDocumentManager;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SentToProcurador()
    {
        _tickSent.SetActive(true);
        _button.interactable = false;

        int i = 0;
        bool found = false;
        while (!found && i < _docManager.documents.Count)
        {

            if (_documentInfo.GetDocName() == _docManager.documents[i].GetDocName())
            {
                found = true;
                _docManager.documents[i].OnSentToProcurador();
                _docManager.AddSentDocInfo(_documentInfo.GetDocName(), _documentInfo.GetContent());
            }

            i++;
        }
    }
    public void OnDocumentPressed()
    {
        if (!_selected.activeSelf)
            Select();
        else
        {
            Unselect();
            _procuradorMessagesPage.SelectDocument(null);
        }
    }

    public void Init(ProcuratorChatPage pmp, Document document)
    {
        _procuradorMessagesPage = pmp;

        _documentInfo = document;
        _docNameText.text = _documentInfo.GetDocName();

        _button.interactable = !_documentInfo.GetSentToProcurador();
        _tickSent.SetActive(_documentInfo.GetSentToProcurador());
        _selected.SetActive(false);
    }
}
