using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConciliacionPage : PCPage
{
    [SerializeField] private Animator clientCharacterAnimator;
    [SerializeField] private Animator rivalCharacterAnimator;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button changeToTextTab;
    [SerializeField] private Button changeToClientTab;
    [SerializeField] private Button changeToRivalTab;
    [SerializeField] private TMP_InputField inputFieldText;
    [SerializeField] private GameObject popupConfirmSend;
    [SerializeField] private GameObject inputConciliacionTabHolder;
    [SerializeField] private GameObject clientTabHolder;
    [SerializeField] private GameObject rivalTabHolder;


    // Llamado al pulsar el boton de mandar intento de conciliacion
    public void TrySend()
    {

    }

    // Llamado al pulsar "Si" en el popup de confirmacion
    public void ConfirmSend()
    {

    }


    // Llamado al pulsar "No" en el popup de confirmacion
    public void CancelSend()
    {
        
    }


    public override void Open()
    {
        computerSystem.ToggleNotification(Page.Conciliacion, false);

        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(true);
        
    }

    public override void Close()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(false);

    }
}
