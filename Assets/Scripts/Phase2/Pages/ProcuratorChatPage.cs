using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pagina para gestionar el sistema de mensajes con el procurador
/// </summary>
public class ProcuratorChatPage : ChatPage {

    public GameObject docsUIContainer;
    public GameObject procuradorDocUIPrefab;
    private bool isOpen = false;

    private ProcuratorUIDocument selectedDoc = null;

    public void SelectDocument(ProcuratorUIDocument doc)
    {
        if (selectedDoc != null && selectedDoc != doc)
            selectedDoc.Unselect();

        selectedDoc = doc;

        _sendButton.interactable = selectedDoc != null;
    }
    private void setupUIDocuments()
    {
        for (int i = 0; i < docsUIContainer.transform.childCount; i++)
            Destroy(docsUIContainer.transform.GetChild(i).gameObject);


        List<Document> documents = GameSystem.Instance.myDocumentManager.documents;
        for (int i = 0; i < documents.Count; i++)
        {
            GameObject documentInstanced = Instantiate(procuradorDocUIPrefab, docsUIContainer.transform);

            documentInstanced.GetComponent<ProcuratorUIDocument>().Init(this, documents[i]);
        }
    }

    private void recieveChatMessage(string answer)
    {
        EndPendingMessage(answer);

        if (!isOpen)
            computerSystem.ToggleNotification(Page.ProcuratorChat, true);
    }
  
    private IEnumerator processDocument(string docName)
    {
        _sendButton.interactable = false;

        // simulamos que piensa
        float randomWait = UnityEngine.Random.Range(1f, 3f);
        yield return new WaitForSeconds(randomWait);


        int randomMessageSelection = UnityEngine.Random.Range(0, 2);

        string message = "";
        switch (randomMessageSelection)
        {
            case 0:
                message = "Okay, mando el documento " + docName + " al juzgado y a la parte contraria.";
                break;
            case 1:
                message = "Gracias por pasarmelo, se lo adjunto el documento " + docName + " al juzgado y una copia a la parte contraria.";
                break;
        }
        recieveChatMessage(message);

        _sendButton.interactable = true;
    }

    /// <summary>
    /// Se llama al pulsar el boton de mandar en un doc seleccionado
    /// </summary>
    public void OnPressButton()
    {
        if (!selectedDoc)
            return;

        JsonSchema schema = new JsonSchema();

        schema.properties.Add("answer", new PropertyInfo(JsonDataType.String));


        // mandar doc
        StartCoroutine(processDocument(selectedDoc.documentInfo.GetDocName()));
        selectedDoc.SentToProcurador();

        string message = "Hola! Te adjunto el siguiente documento para que lo incluyas en el proceso: " + selectedDoc.documentInfo.GetDocName();
        addMessage(message, true);
        StartPendingMessage(false);

        selectedDoc.Unselect();
        SelectDocument(null);
    }

    public override void Open()
    {
        computerSystem.ToggleNotification(Page.ProcuratorChat, false);

        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(true);
        
        setupUIDocuments();

        SelectDocument(null);

        ScrollToLastMessage();

        isOpen = true;
    }

    public override void Close()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(false);

        isOpen = false;
    }

    public void Start()
    {
        Open();

        placeMessages(GameSystem.Instance.CaseData.procuratorMessages);
        ScrollToLastMessage();

        Close();



        /*
        Document doc = new Document();
        Document doc1 = new Document();
        Document doc2 = new Document();

        doc.SetDoc("test", PromptType.Informe, "hola", true, 20);
        doc1.SetDoc("test2", PromptType.Informe, "hola", true, 20);
        doc2.SetDoc("test3", PromptType.Informe, "hola", true, 20);

        GameSystem.Instance.myDocumentManager.documents.Add(doc);
        GameSystem.Instance.myDocumentManager.documents.Add(doc1);
        GameSystem.Instance.myDocumentManager.documents.Add(doc2);
        ^*/

        GameSystem.Instance.myDocumentManager.CreateDocument("test", PromptType.Question, "añskdjf", true, 49);
    }


    public void StartPendingOpponentMessage()
    {
        StartPendingMessage(false);

        if (!isOpen)
            computerSystem.ToggleNotification(Page.ProcuratorChat, true);
    }


    public void ReceiveOpponentDocMessage(string summary)
    {
        EndPendingMessage(summary);

        if (!isOpen)
            computerSystem.ToggleNotification(Page.ProcuratorChat, true);
    }

    public void CancelPendingOpponentMessage()
    {
        EndPendingMessage("...");
    }
}
