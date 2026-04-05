using UnityEngine;
using TMPro;
using System.Collections;

public class WriteTextSystem : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private TextMeshProUGUI textContainer;
    [SerializeField] private int maxCharactersInContainer;
    [SerializeField] private float typingSpeed = 0.05f;

    private Coroutine _typingCoroutine;
    private bool _isTyping = false;
    private bool _waitingInputToContinue = false;

    /// <summary>
    /// Inicia la escritura del texto.
    /// </summary>
    public void WriteText(string text)
    {
        if (_isTyping) StopCoroutine(_typingCoroutine);
        _typingCoroutine = StartCoroutine(TypeConversation(text));
    }
    /// <summary>
    /// Si todavia hay texto skipea el typing y lo pone todo en pantalla. Si esta esperando a input, pone el flag 
    /// _waitingInputToContinue a false y salta al siguiente texto
    /// </summary>
    public void SkipTyping()
    {
        textContainer.maxVisibleCharacters = textContainer.text.Length;
        _waitingInputToContinue = false;
    }

    /// <summary>
    /// Escribe el texto poco a poco. Si hay mas texto que lo que puede almacenar la caja de dialogo, activa el flag 
    /// _waitingInputToContinue y espera a que sea false.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IEnumerator TypeConversation(string text)
    {

        Debug.Log("Write Conversation: " + text);

        _waitingInputToContinue = false;
        _isTyping = true;

        int textCount = 0;

        while (textCount < text.Length)
        {
            while (_waitingInputToContinue)
                yield return null;

            int remainingCharacters = text.Length - textCount;
            int currentChunkSize = Mathf.Min(maxCharactersInContainer, remainingCharacters);

            // logica para no cortar palabras
            if (remainingCharacters > maxCharactersInContainer)
            {
                int lastSpaceIndex = text.LastIndexOfAny(new char[] { ' ', '\n' }, textCount + currentChunkSize - 1, currentChunkSize);

                if (lastSpaceIndex != -1 && lastSpaceIndex > textCount)
                {
                    currentChunkSize = lastSpaceIndex - textCount;
                }
            }

            string subText = text.Substring(textCount, currentChunkSize);

            yield return StartCoroutine(TypeText(subText));

            textCount += currentChunkSize;

            _waitingInputToContinue = textCount < text.Length;
        }

        _isTyping = false;

        //textContainer.text = "";

    }

    
    /// <summary>
    /// Simplemente escribe el texto poco a poco
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public IEnumerator TypeText(string text)
    {
        textContainer.text = text;
        textContainer.maxVisibleCharacters = 0;

        textContainer.ForceMeshUpdate();

        int visibleCount = 0;

        while (textContainer.maxVisibleCharacters < textContainer.text.Length)
        {
            textContainer.maxVisibleCharacters = Mathf.Max(textContainer.maxVisibleCharacters, visibleCount);
            visibleCount++;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    

    /// <summary>
    /// Comprueba si todavia queda texto por mostrar, o esta esperando a input para avanzar el texto
    /// </summary>
    public bool IsTyping()
    {
        return _isTyping;
    }
}