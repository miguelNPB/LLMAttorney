using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcuradorMessagesPage : MessagesUIComponent {
    /// <summary>
    /// Formato para el LLM de la respuesta del cliente
    /// </summary>
    [Serializable]
    private class ProcuradorMessageResponse
    {
        public string answer;
    }


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
            Destroy(docsUIContainer.transform.GetChild(i));


        List<Document> documents = GameSystem.Instance.myDocumentManager.documents;
        for (int i = 0; i < documents.Count; i++)
        {
            GameObject documentInstanced = Instantiate(procuradorDocUIPrefab, docsUIContainer.transform);

            documentInstanced.GetComponent<ProcuradorDocUI>().Init(this, documents[i]);
        }
    }

    public void ReceiveChatMessage(bool success, string answer)
    {
        // deserializamos la respuesta
        ProcuradorMessageResponse jsonResponse = JsonUtility.FromJson<ProcuradorMessageResponse>(answer);

        EndPendingMessage(jsonResponse.answer);

        if (!isOpen)
            computerSystem.ToggleNotification(Page.ChatProcurador, true);
    }

    /// <summary>
    /// Se llama al pulsar el boton de mandar en un doc seleccionado
    /// </summary>
    public void OnSendDocument()
    {
        JsonSchema schema = new JsonSchema();

        schema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        string prompt = "Hola! Te adjunto el siguiente documento para que lo incluyas en el proceso: " + selectedDoc.documentInfo.GetDocName();

        // TODO mandar prompt 

        selectedDoc.Unselect();
        SelectDocument(null);

        AddMessage(prompt, true);
        StartPendingMessage(false);
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

        Document doc = new Document();
        Document doc1 = new Document();
        Document doc2 = new Document();

        doc.SetDoc("test", PromptType.Informe, "hola", true, 20);
        doc1.SetDoc("test2", PromptType.Informe, "hola", true, 20);
        doc2.SetDoc("test3", PromptType.Informe, "hola", true, 20);

        GameSystem.Instance.myDocumentManager.documents.Add(doc);
        GameSystem.Instance.myDocumentManager.documents.Add(doc1);
        GameSystem.Instance.myDocumentManager.documents.Add(doc2);
    }
}
