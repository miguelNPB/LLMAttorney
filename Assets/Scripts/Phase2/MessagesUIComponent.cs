using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Heredar de esta clase segun el tipo de conversacion y mandar en el start de la clase el placeMessages con los mensajes correspondientes
/// </summary>
public abstract class MessagesUIComponent : MonoBehaviour
{
    [SerializeField] private GameObject ConversationMessageUIPrefab;
    [SerializeField] private VerticalLayoutGroup layoutGroup;

    [SerializeField] private Color otherPersonColor = Color.blue;
    [SerializeField] private Color playerColor = Color.green;

    [SerializeField] private int scrollSpeed = 1;

    private int totalHeight = 0;
    protected void PlaceMessages(List<ConversationMessage> messages)
    {
        totalHeight = 0;
        foreach (ConversationMessage message in messages)
        {
            GameObject panel = Instantiate(ConversationMessageUIPrefab, layoutGroup.transform);

            Image ImageComp = panel.GetComponent<Image>();
            ImageComp.color = message.fromPlayer ? playerColor : otherPersonColor;

            TMP_Text tmpText = panel.GetComponentInChildren<TMP_Text>();
            tmpText.text = message.text;
             
            tmpText.ForceMeshUpdate();
            int numLineas = tmpText.textInfo.lineCount;
            int height = 25 + (numLineas * 50);

            totalHeight += height;

            RectTransform rectTr = panel.GetComponent<RectTransform>();
            rectTr.sizeDelta = new Vector2(1500, height);
        }
    }

    protected void Scroll(float direction)
    {
        int oldValue = layoutGroup.padding.top;

        layoutGroup.padding.top = Mathf.Clamp((oldValue + (scrollSpeed * (int)direction)), -totalHeight, 20);
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
