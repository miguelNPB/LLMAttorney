using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcuradorMessagesPage : MessagesUIComponent {

    public GameObject docsUIContainer;
    public GameObject procuradorDocUIPrefab;
    public Button sendDocumentButton;
    private bool isOpen = false;

    private ProcuradorDocUI selectedDoc = null;

    public void SelectDocument(ProcuradorDocUI doc)
    {
        if (selectedDoc != null && selectedDoc != doc)
            selectedDoc.Unselect();

        selectedDoc = doc;

        sendDocumentButton.interactable = selectedDoc != null;
    }
    private void SetupUIDocuments()
    {
        for (int i = 0; i < docsUIContainer.transform.childCount; i++)
            Destroy(docsUIContainer.transform.GetChild(i).gameObject);


        List<Document> documents = GameSystem.Instance.myDocumentManager.documents;
        for (int i = 0; i < documents.Count; i++)
        {
            GameObject documentInstanced = Instantiate(procuradorDocUIPrefab, docsUIContainer.transform);

            documentInstanced.GetComponent<ProcuradorDocUI>().Init(this, documents[i]);
        }
    }

    public void ReceiveChatMessage(string answer)
    {
        EndPendingMessage(answer);

        if (!isOpen)
            computerSystem.ToggleNotification(Page.ChatProcurador, true);
    }
  
    private IEnumerator ProcessDocument(string docName)
    {
        sendDocumentButton.interactable = false;

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
        ReceiveChatMessage(message);

        sendDocumentButton.interactable = true;
    }

    /// <summary>
    /// Se llama al pulsar el boton de mandar en un doc seleccionado
    /// </summary>
    public void OnSendDocument()
    {
        if (!selectedDoc)
            return;

        JsonSchema schema = new JsonSchema();

        schema.properties.Add("answer", new PropertyInfo(JsonDataType.String));


        // mandar doc
        StartCoroutine(ProcessDocument(selectedDoc.documentInfo.GetDocName()));
        selectedDoc.SentToProcurador();

        string message = "Hola! Te adjunto el siguiente documento para que lo incluyas en el proceso: " + selectedDoc.documentInfo.GetDocName();
        AddMessage(message, true);
        StartPendingMessage(false);

        selectedDoc.Unselect();
        SelectDocument(null);
    }

    public override void Open()
    {
        computerSystem.ToggleNotification(Page.ChatProcurador, false);

        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(true);
        
        SetupUIDocuments();

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

        PlaceMessages(GameSystem.Instance.CaseData.procuradorMessages);
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

        GameSystem.Instance.myDocumentManager.CreateDocument("test", PromptType.Pregunta, "añskdjf", true, 49);
    }


    public void StartPendingOpponentMessage()
    {
        StartPendingMessage(false);

        if (!isOpen)
            computerSystem.ToggleNotification(Page.ChatProcurador, true);
    }


    public void ReceiveOpponentDocMessage(string summary)
    {
        EndPendingMessage(summary);

        if (!isOpen)
            computerSystem.ToggleNotification(Page.ChatProcurador, true);
    }

    public void CancelPendingOpponentMessage()
    {
        EndPendingMessage("...");
    }
}
