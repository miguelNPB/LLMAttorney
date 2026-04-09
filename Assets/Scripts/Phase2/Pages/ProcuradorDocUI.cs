using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcuradorDocUI : MonoBehaviour
{
    public ProcuradorMessagesPage procuradorMessagesPage;
    public Button button;
    public TMP_Text docName;
    public GameObject tickSent;
    public GameObject selected;


    public void Unselect()
    {
        selected.SetActive(false);
    }

    public void Select()
    {
        selected.SetActive(true);
    }

    public void OnDocumentPressed()
    {
        if (selected.activeSelf)
            Select();
        else
            Unselect();
    }

    public void Init(string DocName, bool sent)
    {
        docName.text = DocName;

        button.interactable = !sent;
        tickSent.SetActive(sent);
        selected.SetActive(false);
    }
}
