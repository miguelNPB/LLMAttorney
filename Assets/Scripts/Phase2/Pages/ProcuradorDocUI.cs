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

    private DocumentManager myDocManager;
    public void Unselect()
    {
        selected.SetActive(false);

    }

    public void Select()
    {
        selected.SetActive(true);

        _procuradorMessagesPage.SelectDocument(this);
    }

    public void Awake()
    {
        myDocManager = GameSystem.Instance.myDocumentManager;
    }

    public void SentToProcurador()
    {
        tickSent.SetActive(true);
        button.interactable = false;

        int i = 0;
        bool found = false;
        while (!found && i < myDocManager.documents.Count)
        {

            if (documentInfo.GetDocName() == myDocManager.documents[i].GetDocName())
            {
                found = true;
                myDocManager.documents[i].OnSentToProcurador();
                myDocManager.AddSentDocInfo(documentInfo.GetDocName(), documentInfo.GetContent());
            }

            i++;
        }
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
