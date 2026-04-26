using System;
using System.Collections;
using System.Collections.Generic;
using Telemetry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UI;
using UnityEngine.WSA;

/// <summary>
/// Pagina para gestionar el sistema de mensajes con el procurador
/// </summary>
public class ProcuratorChatPage : ChatPage {

    [SerializeField] private GameObject _holder;
    [SerializeField] private GameObject _sendLawsuitFirstText;
    [SerializeField] private GameObject _docsUIContainer;
    [SerializeField] private GameObject _procuradorDocUIPrefab;

    private bool _isOpen = false;
    private ProcuratorUIDocument _selectedDoc = null;
    private DocumentManager _docManager;
    public void SelectDocument(ProcuratorUIDocument doc)
    {
        if (_selectedDoc != null && _selectedDoc != doc)
            _selectedDoc.Unselect();

        _selectedDoc = doc;

        _sendButton.interactable = _selectedDoc != null;
    }
    private void setupUIDocuments()
    {
        for (int i = 0; i < _docsUIContainer.transform.childCount; i++)
            Destroy(_docsUIContainer.transform.GetChild(i).gameObject);

        // coger documentos cliente
        List<Document> documents = new List<Document>();
        foreach (uint docId in _docManager.GetPlayerDocs())
        {
            Document doc = _docManager.GetDocument(docId);
            if (!doc.IsRivalDoc())
                documents.Add(doc);
        }


        Debug.Log("Documentos que mostrar en el apartado del procurador: " + documents.Count);

        // instanciar prefabs ui
        for (int i = 0; i < documents.Count; i++)
        {
            GameObject documentInstanced = Instantiate(_procuradorDocUIPrefab, _docsUIContainer.transform);

            documentInstanced.GetComponent<ProcuratorUIDocument>().Init(this, documents[i]);

            Debug.Log("Documento " + i + " instanciado en la posicion " + documentInstanced.transform.position);
        }
    }

    /// <summary>
    /// Comprueba si la demanda ha sdio mandada y activa o desactiva unos gameobject en funcion de eso
    /// </summary>
    private void checkIfLawsuitSent()
    {
        _sendButton.interactable = GameSystem.Instance.CaseData.isDemandaSent;
        _docsUIContainer.SetActive(GameSystem.Instance.CaseData.isDemandaSent);
        _sendLawsuitFirstText.SetActive(!GameSystem.Instance.CaseData.isDemandaSent);
    }

    private void recieveChatMessage(string answer)
    {
        EndPendingMessage(answer);

        if (!_isOpen)
            _computerSystem.ToggleNotification(Page.ProcuratorChat, true);
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
        if (!_selectedDoc)
            return;

        JsonSchema schema = new JsonSchema();

        schema.properties.Add("answer", new PropertyInfo(JsonDataType.String));


        // mandar doc
        TelemetryDispatch.SendPostDocument((int)_selectedDoc.documentInfo.GetDocType());

        StartCoroutine(processDocument(_selectedDoc.documentInfo.GetDocName()));
        _selectedDoc.SentToProcurador();

        string message = "Hola! Te adjunto el siguiente documento para que lo incluyas en el proceso: " + _selectedDoc.documentInfo.GetDocName();
        addMessage(message, true);
        StartPendingMessage(false);

        _selectedDoc.Unselect();
        SelectDocument(null);
    }

    public override void Open()
    {
        _computerSystem.ToggleNotification(Page.ProcuratorChat, false);

        _holder.SetActive(true);

        checkIfLawsuitSent();

        setupUIDocuments();

        SelectDocument(null);

        ScrollToLastMessage();

        _isOpen = true;
    }

    public override void Close()
    {
        _holder.SetActive(false);

        _isOpen = false;
    }

    public void Start()
    {
        _docManager = GameSystem.Instance.CaseData.documentManager;
        Open();

        placeMessages(GameSystem.Instance.CaseData.procuratorMessages);
        ScrollToLastMessage();

        Close();
    }


    public void StartPendingOpponentMessage()
    {
        StartPendingMessage(false);

        if (!_isOpen)
            _computerSystem.ToggleNotification(Page.ProcuratorChat, true);
    }


    public void ReceiveOpponentDocMessage(string summary)
    {
        EndPendingMessage(summary);

        if (!_isOpen)
            _computerSystem.ToggleNotification(Page.ProcuratorChat, true);
    }

    public void CancelPendingOpponentMessage()
    {
        EndPendingMessage("...");
    }
}
