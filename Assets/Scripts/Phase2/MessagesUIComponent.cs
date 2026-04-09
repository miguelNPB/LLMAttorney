using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Heredar de esta clase segun el tipo de conversacion y mandar en el start de la clase el placeMessages con los mensajes correspondientes
/// </summary>
public abstract class MessagesUIComponent : PCPage
{
    [SerializeField] private GameObject ConversationMessageUIPrefab;
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    [SerializeField] private Button sendButton;

    [SerializeField] private Color otherPersonColor = Color.blue;
    [SerializeField] private Color playerColor = Color.green;

    [SerializeField] private int scrollSpeed = 1;

    private int totalHeight = 0;
    private GameObject lastMessageAdded = null;
    private string pendingMessage = "";
    private bool waitingPendingMessage = false;

    /// <summary>
    /// Para poner todos unos mensajes de golpe a la vez
    /// </summary>
    /// <param name="messages"></param>
    protected void PlaceMessages(List<ConversationMessage> messages)
    {
        totalHeight = 0;
        foreach (ConversationMessage message in messages)
        {
            AddMessage(message.text, message.fromPlayer);
        }
    }

    /// <summary>
    /// Instancia un mensaje y le cambia el color segun si es player o no
    /// </summary>
    /// <param name="text"></param>
    /// <param name="fromPlayer"></param>
    protected void AddMessage(string text, bool fromPlayer)
    {
        lastMessageAdded = Instantiate(ConversationMessageUIPrefab, layoutGroup.transform);

        Image ImageComp = lastMessageAdded.GetComponent<Image>();
        ImageComp.color = fromPlayer ? playerColor : otherPersonColor;

        TMP_Text tmpText = lastMessageAdded.GetComponentInChildren<TMP_Text>();
        tmpText.text = text;

        tmpText.ForceMeshUpdate();
        int numLineas = tmpText.textInfo.lineCount;
        int height = 25 + (numLineas * 50);

        totalHeight += height;

        RectTransform rectTr = lastMessageAdded.GetComponent<RectTransform>();
        rectTr.sizeDelta = new Vector2(1500, height);
    }

    /// <summary>
    /// Llamar a esto para a�adir un mensaje que tenga una animacion de puntos suspensivos hasta que se llame a EndPendingMessage
    /// </summary>
    /// <param name="fromPlayer"></param>
    public void StartPendingMessage(bool fromPlayer)
    {
        sendButton.interactable = false;
        AddMessage(".", fromPlayer);

        StartCoroutine(CoroutinePendingMessage());
    }
    private IEnumerator CoroutinePendingMessage()
    {
        waitingPendingMessage = true;

        float timer = 0;
        TMP_Text tmpText = lastMessageAdded.GetComponentInChildren<TMP_Text>();

        while (waitingPendingMessage)
        {
            timer += Time.deltaTime;

            tmpText.text = "";
            for (int i = 0; i < (timer % 3); i++)
                tmpText.text += ".";
            
            yield return null;
        }

        tmpText.text = pendingMessage;
        // actualizar caja de texto y lineas totales
        tmpText.ForceMeshUpdate();
        int numLineas = tmpText.textInfo.lineCount;
        int height = 25 + (numLineas * 50);

        totalHeight += height;

        RectTransform rectTr = lastMessageAdded.GetComponent<RectTransform>();
        rectTr.sizeDelta = new Vector2(1500, height);
    }

    /// <summary>
    /// Llamar esto para detener la animacion de puntos suspensivos y rellenar el mensaje con el contenido del texto
    /// </summary>
    /// <param name="text"></param>
    public void EndPendingMessage(string text)
    {
        sendButton.interactable = true;

        pendingMessage = text;

        waitingPendingMessage = false;
    }

    /// <summary>
    /// Scrollea
    /// </summary>
    /// <param name="direction"></param>
    protected void Scroll(float direction)
    {
        int oldValue = layoutGroup.padding.top;

        layoutGroup.padding.top = Mathf.Clamp((oldValue + (scrollSpeed * (int)direction)), -totalHeight, 20);
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
    }

    /// <summary>
    /// Scrollea instantaneamente abajo del todo
    /// </summary>
    protected void ScrollToLastMessage()
    {
        layoutGroup.padding.top = -totalHeight;
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
    }

    private void OnEnable()
    {
        InputSystem.Instance.OnScrollPerformed += Scroll;
    }

    private void OnDisable()
    {
        InputSystem.Instance.OnScrollPerformed -= Scroll;
    }
}
