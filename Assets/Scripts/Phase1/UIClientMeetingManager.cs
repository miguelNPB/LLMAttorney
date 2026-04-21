using TMPro;
using System.Collections;
using UnityEngine;

public class UIClientMeetingManager : MonoBehaviour
{
    [SerializeField]
    private WriteTextSystem _writeTextSystem;

    [SerializeField]
    private GameObject _searchToolButton;

    [SerializeField]
    private GameObject _clientMessageUI;

    [SerializeField]
    private GameObject _userMessageUI;

    [SerializeField]
    private GameObject _changePhaseButton;

    [SerializeField] private TMP_Text resultText;
    //[SerializeField] private VerticalLayoutGroup layoutGroup;

    private string _pendingMessage = "";
    private bool _waitingPendingMessage = false;

    private IEnumerator CoroutinePendingMessage()
    {
        _waitingPendingMessage = true;

        float timer = 0;

        while (_waitingPendingMessage)
        {
            timer += Time.deltaTime;

            resultText.text = "";
            for (int i = 0; i < (timer % 3); i++)
                resultText.text += ".";

            yield return null;
        }

        resultText.text = _pendingMessage;

        // actualizar caja de texto y lineas totales
        //resultText.ForceMeshUpdate();

    }

    /// <summary>
    /// Instancia un mensaje y le cambia el color segun si es player o no
    /// </summary>
    /// <param name="text"></param>
    /// <param name="fromPlayer"></param>
    public void AddMessage(string text)
    {
        resultText.text = text;

        resultText.ForceMeshUpdate();

    }

    public void ShowMessage(bool changePhase)
    {
        _writeTextSystem.WriteText(resultText.text);

        if (changePhase)
        {
            _changePhaseButton.SetActive(true);
        }
    }

    /// <summary>
    /// Llamar a esto para añadir un mensaje que tenga una animacion de puntos suspensivos hasta que se llame a EndPendingMessage
    /// </summary>
    /// <param name="fromPlayer"></param>
    public void StartPendingMessage()
    {
        AddMessage(".");

        StartCoroutine(CoroutinePendingMessage());
    }
    
    /// <summary>
    /// Llamar esto para detener la animacion de puntos suspensivos y rellenar el mensaje con el contenido del texto
    /// </summary>
    /// <param name="text"></param>
    public void EndPendingMessage(string text)
    {
        _pendingMessage = text;

        _waitingPendingMessage = false;
    }


    public void SwitchMenusConversation(bool activeUserMenu)
    {

        if (!activeUserMenu)
        {
            _clientMessageUI.SetActive(true);
            _userMessageUI.SetActive(false);
        }
        else
        {
            _clientMessageUI.SetActive(_writeTextSystem.IsTyping());
            _userMessageUI.SetActive(!_writeTextSystem.IsTyping());
            _searchToolButton.SetActive(!_writeTextSystem.IsTyping());

        }

        
    }
}
