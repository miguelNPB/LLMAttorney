using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Heredar de esta clase segun el tipo de conversacion y mandar en el start de la clase el placeMessages con los mensajes correspondientes
/// </summary>
public abstract class ChatPage : IPage
{
    [SerializeField] protected GameObject _conversationMessageUIPrefab;
    [SerializeField] protected VerticalLayoutGroup _layoutGroup;
    [SerializeField] protected Button _sendButton;

    [SerializeField] protected Color _otherPersonColor = Color.blue;
    [SerializeField] protected Color _playerColor = Color.green;

    [SerializeField] protected int _scrollSpeed = 1;

    private int _totalHeight = 0;
    private GameObject _lastMessageAdded = null;
    private string _pendingMessage = "";
    private bool _waitingPendingMessage = false;

    /// <summary>
    /// Para poner todos unos mensajes de golpe a la vez
    /// </summary>
    /// <param name="messages"></param>
    protected void placeMessages(List<ConversationMessage> messages)
    {
        _totalHeight = -225;
        foreach (ConversationMessage message in messages)
        {
            addMessage(message.text, message.fromPlayer);
        }
    }

    /// <summary>
    /// Instancia un mensaje y le cambia el color segun si es player o no
    /// </summary>
    /// <param name="text"></param>
    /// <param name="fromPlayer"></param>
    protected void addMessage(string text, bool fromPlayer)
    {
        _lastMessageAdded = Instantiate(_conversationMessageUIPrefab, _layoutGroup.transform);

        Image ImageComp = _lastMessageAdded.GetComponent<Image>();
        ImageComp.color = fromPlayer ? _playerColor : _otherPersonColor;

        TMP_Text tmpText = _lastMessageAdded.GetComponentInChildren<TMP_Text>();
        tmpText.text = text;

        tmpText.ForceMeshUpdate();
        int numLineas = tmpText.textInfo.lineCount;
        int height = 25 + (numLineas * 50);

        _totalHeight += height;

        RectTransform rectTr = _lastMessageAdded.GetComponent<RectTransform>();
        rectTr.sizeDelta = new Vector2(1500, height);
    }

    /// <summary>
    /// Llamar a esto para a�adir un mensaje que tenga una animacion de puntos suspensivos hasta que se llame a EndPendingMessage
    /// </summary>
    /// <param name="fromPlayer"></param>
    public void StartPendingMessage(bool fromPlayer)
    {
        _sendButton.interactable = false;
        addMessage(".", fromPlayer);

        StartCoroutine(coroutinePendingMessage());
    }
    private IEnumerator coroutinePendingMessage()
    {
        _waitingPendingMessage = true;

        float timer = 0;
        TMP_Text tmpText = _lastMessageAdded.GetComponentInChildren<TMP_Text>();

        while (_waitingPendingMessage)
        {
            timer += Time.deltaTime;

            tmpText.text = "";
            for (int i = 0; i < (timer % 3); i++)
                tmpText.text += ".";
            
            yield return null;
        }

        tmpText.text = _pendingMessage;
        // actualizar caja de texto y lineas totales
        tmpText.ForceMeshUpdate();
        int numLineas = tmpText.textInfo.lineCount;
        int height = 25 + (numLineas * 50);

        _totalHeight += height;

        RectTransform rectTr = _lastMessageAdded.GetComponent<RectTransform>();
        rectTr.sizeDelta = new Vector2(1500, height);
    }

    /// <summary>
    /// Llamar esto para detener la animacion de puntos suspensivos y rellenar el mensaje con el contenido del texto
    /// </summary>
    /// <param name="text"></param>
    public void EndPendingMessage(string text)
    {
        _sendButton.interactable = true;

        _pendingMessage = text;

        _waitingPendingMessage = false;
    }

    /// <summary>
    /// Scrollea
    /// </summary>
    /// <param name="direction"></param>
    protected void Scroll(float direction)
    {
        int oldValue = _layoutGroup.padding.top;

        _layoutGroup.padding.top = Mathf.Clamp((oldValue + (_scrollSpeed * (int)direction)), -_totalHeight, Mathf.Max(20, _layoutGroup.padding.top));
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup.GetComponent<RectTransform>());
    }

    /// <summary>
    /// Scrollea instantaneamente abajo del todo
    /// </summary>
    protected void ScrollToLastMessage()
    {
        _layoutGroup.padding.top = -_totalHeight;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup.GetComponent<RectTransform>());
    }

    private void OnEnable()
    {
        //InputSystem.Instance.onScrollPerformed += Scroll;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        //InputSystem.Instance.onScrollPerformed -= Scroll;
    }
}
