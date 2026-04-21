using UnityEngine;
using TMPro;
using System.Collections;

public class WriteTextSystem : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private TextMeshProUGUI textContainer;
    [SerializeField] private int maxCharactersInContainer;
    [SerializeField] private float typingSpeed = 0.05f;

    private Coroutine _typingCoroutine;
    private bool _isTyping = false;
    private bool _waitingInputToContinue = false;

    public void WriteText(string text)
    {
        if (_isTyping) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeConversation(text));
    }

    public void SkipTyping()
    {
        if (_isTyping && !_waitingInputToContinue)
        {
            textContainer.maxVisibleCharacters = textContainer.text.Length;
        }

        _waitingInputToContinue = false;
    }

    public IEnumerator TypeConversation(string text)
    {
        _isTyping = true;
        int textCount = 0;

        while (textCount < text.Length)
        {
            _waitingInputToContinue = false;

            int remainingCharacters = text.Length - textCount;
            int currentChunkSize = Mathf.Min(maxCharactersInContainer, remainingCharacters);

            string potentialText = text.Substring(textCount, currentChunkSize);

            int lineBreakIndex = potentialText.IndexOf('\n');
            if (lineBreakIndex != -1)
            {
                currentChunkSize = lineBreakIndex + 1;
            }
            else if (remainingCharacters > maxCharactersInContainer)
            {
                int lastSpaceIndex = potentialText.LastIndexOf(' ');
                if (lastSpaceIndex > 0)
                {
                    currentChunkSize = lastSpaceIndex + 1;
                }
            }

            string subText = text.Substring(textCount, currentChunkSize);

            yield return StartCoroutine(TypeText(subText));

            textCount += currentChunkSize;

            if (textCount < text.Length)
            {
                _waitingInputToContinue = true;
                while (_waitingInputToContinue)
                    yield return null;
            }
        }

        _isTyping = false;
    }

    public IEnumerator TypeText(string text)
    {
        textContainer.text = text;
        textContainer.maxVisibleCharacters = 0;
        textContainer.ForceMeshUpdate();

        int totalVisibleCharacters = text.Length;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            // Si el usuario hizo SkipTyping, maxVisibleCharacters ya será igual al length
            if (textContainer.maxVisibleCharacters >= totalVisibleCharacters)
                yield break;

            textContainer.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public bool IsTyping() => _isTyping;
}