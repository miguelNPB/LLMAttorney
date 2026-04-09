using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcuradorDocUI : MonoBehaviour
{
    private ProcuradorMessagesPage _procuradorMessagesPage;

    public Button button;
    public TMP_Text docName;
    public GameObject tickSent;
    public GameObject selected;

    [HideInInspector]public Document documentInfo;
    public void Unselect()
    {
        Debug.Log("unselect");
        selected.SetActive(false);

    }

    public void Select()
    {
        Debug.Log("select");
        selected.SetActive(true);

        _procuradorMessagesPage.SelectDocument(this);
    }

    public void OnDocumentPressed()
    {
        if (!selected.activeSelf)
            Select();
        else
        {
            Unselect();
            _procuradorMessagesPage.SelectDocument(null);
        }
    }

    public void Init(ProcuradorMessagesPage pmp, Document document)
    {
        _procuradorMessagesPage = pmp;

        documentInfo = document;
        docName.text = documentInfo.GetDocName();

        button.interactable = !documentInfo.GetSentToProcurador();
        tickSent.SetActive(documentInfo.GetSentToProcurador());
        selected.SetActive(false);
    }
}
