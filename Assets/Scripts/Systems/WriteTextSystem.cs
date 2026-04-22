using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class WriteTextSystem : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private TextMeshProUGUI _textContainer;
    [SerializeField] private int _maxCharactersInContainer;
    [SerializeField] private float _typingSpeed = 0.05f;

    private Coroutine _typingCoroutine;
    private bool _isTyping = false;
    private bool _waitingInputToContinue = false;

    public Action onFinishTyping;

    public bool IsTyping() => _isTyping;
    public void WriteText(string text)
    {
        if (_isTyping) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeConversation(text));
    }

    public void SkipTyping()
    {
        if (_isTyping && !_waitingInputToContinue)
        {
            _textContainer.maxVisibleCharacters = _textContainer.text.Length;
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
            int currentChunkSize = Mathf.Min(_maxCharactersInContainer, remainingCharacters);

            string potentialText = text.Substring(textCount, currentChunkSize);

            int lineBreakIndex = potentialText.IndexOf('\n');
            if (lineBreakIndex != -1)
            {
                currentChunkSize = lineBreakIndex + 1;
            }
            else if (remainingCharacters > _maxCharactersInContainer)
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
                onFinishTyping?.Invoke();

                while (_waitingInputToContinue)
                    yield return null;
            }
        }

        _isTyping = false;
    }

    public IEnumerator TypeText(string text)
    {
        _textContainer.text = text;
        _textContainer.maxVisibleCharacters = 0;
        _textContainer.ForceMeshUpdate();

        int totalVisibleCharacters = text.Length;
        int counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            // Si el usuario hizo SkipTyping, maxVisibleCharacters ya será igual al length
            if (_textContainer.maxVisibleCharacters >= totalVisibleCharacters)
                yield break;

            _textContainer.maxVisibleCharacters = counter;
            counter++;
            yield return new WaitForSeconds(_typingSpeed);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}