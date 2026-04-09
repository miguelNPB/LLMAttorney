using System;
using System.Collections;
using TMPro;
using UnityEngine;

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
    private bool isOpen = false;

    private void SetupUIDocuments()
    {
        docsUIContainer.
    }

    public void ReceiveChatMessage(bool success, string answer)
    {
        // deserializamos la respuesta
        ProcuradorMessageResponse jsonResponse = JsonUtility.FromJson<ProcuradorMessageResponse>(answer);

        EndPendingMessage(jsonResponse.answer);

        if (!isOpen)
            computerSystem.ToggleNotification(Page.ChatProcurador, true);
    }
    public void SendChatMessage()
    {
        JsonSchema schema = new JsonSchema();

        schema.properties.Add("answer", new PropertyInfo(JsonDataType.String));

        string prompt = "Hola! Te adjunto el siguiente documento para que lo incluyas en el proceso: ";

        AddMessage(prompt, true);
        StartPendingMessage(false);
    }

    public override void Open()
    {
        computerSystem.ToggleNotification(Page.ChatProcurador, false);

        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(true);

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
    }
}
