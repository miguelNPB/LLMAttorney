using TMPro;
using System.Collections;
using UnityEngine;

/// <summary>
/// Clase encargada del manejo de la UI de los mensajes con el cliente
/// </summary>
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

    /// <summary>
    /// Instancia un mensaje en el cuadro de texto del cliente
    /// </summary>
    /// <param name="text">Texto que se debe escribir</param>
    public void AddMessage(string text)
    {
        resultText.text = text;

        resultText.ForceMeshUpdate();

    }

    /// <summary>
    /// Metodo que controla el cambio de cuadros de dialogo entre los mensajes del cliente y el input escrito por el usuario
    /// </summary>
    /// <param name="activeUserMenu">Indica si se debe de activar el cuadro de dialogo del usuario o por el contrario el del cliente</param>
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
