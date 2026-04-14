using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConciliacionPage : PCPage
{
    [SerializeField] private Animator clientCharacterAnimator;
    [SerializeField] private Animator rivalCharacterAnimator;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button changeToTextTab;
    [SerializeField] private Button changeToClientTabButton;
    [SerializeField] private Button changeToRivalTabButton;
    [SerializeField] private GameObject clientTabExclamation;
    [SerializeField] private GameObject rivalTabExclamation;
    [SerializeField] private TMP_InputField inputFieldText;
    [SerializeField] private GameObject popupConfirmSend;
    [SerializeField] private GameObject inputConciliacionTabHolder;
    [SerializeField] private GameObject clientTabHolder;
    [SerializeField] private GameObject rivalTabHolder;
    [SerializeField] private TMP_Text clienteAnswerText;
    [SerializeField] private TMP_Text rivalAnswerText;
    [SerializeField] private GameObject popupAfterFailedConciliation;


    // Llamado al pulsar el boton de mandar intento de conciliacion
    public void TrySend()
    {
        sendButton.interactable = false;
        popupConfirmSend.SetActive(true);
    }

    // Llamado al pulsar "Si" en el popup de confirmacion
    public void ConfirmSend()
    {
        popupConfirmSend.SetActive(false);

        SendProposition();
    }


    // Llamado al pulsar "No" en el popup de confirmacion
    public void CancelSend()
    {
        sendButton.interactable = true;
        popupConfirmSend.SetActive(false);
    }


    // llamado cuando se manda la proposicion, se desactiva el boton y la escritura en el input
    private void SendProposition()
    {
        inputFieldText.interactable = false;
        EnableClientResponse();
    }

    // pasa si no acepta el cliente
    private void RestartProposition()
    {
        inputFieldText.interactable = true;
        sendButton.interactable = true;
        inputFieldText.text = "";
    }

    // activa la pestaña de respuesta de cliente y lo manda
    private void EnableClientResponse()
    {
        clientTabExclamation.SetActive(true);
        changeToClientTabButton.interactable = true;

        // mandar a LLM cliente para que responda
        StartCoroutine(PromptClientResponse());
    }

    private IEnumerator PromptClientResponse()
    {
        yield return null;


        bool clientAgrees = false;

        if (clientAgrees)
        {
            clientCharacterAnimator.SetTrigger("Success");
            EnableRivalResponse();
        }
        else
        {
            clientCharacterAnimator.SetTrigger("Rejection");
            RestartProposition();
        }
    }

    private void EnableRivalResponse()
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
