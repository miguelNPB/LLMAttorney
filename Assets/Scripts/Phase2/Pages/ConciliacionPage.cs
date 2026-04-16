using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConciliacionPage : PCPage
{
    [SerializeField] private Phase2 phase2Manager;
    [SerializeField] private LLMConnectorConciliation llmConnector;
    [SerializeField] private Animator clientCharacterAnimator;
    [SerializeField] private Animator rivalCharacterAnimator;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button changeToTextTab;
    [SerializeField] private Button changeToClientTabButton;
    [SerializeField] private Button changeToRivalTabButton;
    [SerializeField] private GameObject clientTabExclamation;
    [SerializeField] private GameObject rivalTabExclamation;
    [SerializeField] private TMP_InputField inputFieldText;
    [SerializeField] private GameObject popupClientRejects;
    [SerializeField] private GameObject inputConciliacionTabHolder;
    [SerializeField] private GameObject clientTabHolder;
    [SerializeField] private GameObject rivalTabHolder;
    [SerializeField] private TMP_Text clienteAnswerText;
    [SerializeField] private TMP_Text rivalAnswerText;
    [SerializeField] private GameObject popupAfterFailedConciliation;
    [SerializeField] private GameObject popupAfterSuccessfulConciliation;

    private bool _open = false;
    private bool clientAgrees = false;
    private bool rivalAgrees = false;
    private string clientAnswer = "";
    private string rivalAnswer = "";

    public void SetClientAgrees(bool value, string answer)
    {
        clientAgrees = value;
        clientAnswer = answer;
    }
    public void SetRivalAgrees(bool value, string answer)
    {
        rivalAgrees = value;
        rivalAnswer = answer;
    }

    // Llamado al pulsar el boton de mandar intento de conciliacion
    public void SendAttempt()
    {
        sendButton.interactable = false;

        SendProposition();
    }

    // llamado cuando se manda la proposicion, se desactiva el boton y la escritura en el input
    private void SendProposition()
    {
        popupClientRejects.SetActive(false);
        inputFieldText.interactable = false;
        EnableClientResponse();
    }

    // pasa si no acepta el cliente
    private void RestartProposition()
    {
        popupClientRejects.SetActive(true);
        inputFieldText.interactable = true;
        sendButton.interactable = true;
        inputFieldText.text = "";
    }

    // activa la pestaña de respuesta de cliente y lo manda
    private void EnableClientResponse()
    {
        clientTabExclamation.SetActive(true);
        changeToClientTabButton.interactable = true;

        // mandar a LLM prompt para que responda el cliente
        StartCoroutine(PromptClientResponse());
    }



    private IEnumerator PromptClientResponse()
    {
        clientCharacterAnimator.SetTrigger("Thinking");

        yield return StartCoroutine(llmConnector.SendClientPrompt());

        clienteAnswerText.text = clientAnswer;

        if (!_open)
            computerSystem.ToggleNotification(Page.Conciliacion, true);


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
        rivalTabExclamation.SetActive(true);
        changeToRivalTabButton.interactable = true;

        // mandar a LLM prompt para que responda el rival
        StartCoroutine(PromptRivalResponse());
    }

    private IEnumerator PromptRivalResponse()
    {
        rivalCharacterAnimator.SetTrigger("Thinking");

        float random = Random.Range(0.0f, 1.0f);

        if (random > GameSystem.Instance.CaseData.conciliationRivalInstantRejectProbability)
            yield return StartCoroutine(llmConnector.SendRivalPromptNormal());
        else
            yield return StartCoroutine(llmConnector.SendRivalPromptRejectionConfirmed());

        rivalAnswerText.text = rivalAnswer;

        if (!_open)
            computerSystem.ToggleNotification(Page.Conciliacion, true);


        if (rivalAgrees)
        {
            rivalCharacterAnimator.SetTrigger("Success");
            popupAfterSuccessfulConciliation.SetActive(true);
            computerSystem.ToggleExitButton(false);
        }
        else
        {
            rivalCharacterAnimator.SetTrigger("Rejection");
            popupAfterFailedConciliation.SetActive(true);
            phase2Manager.FailedConciliation();
            computerSystem.ToggleNotification(Page.Redaccion, true);
        }
    }



    public void GoToPropuestaTab()
    {
        inputConciliacionTabHolder.SetActive(true);
        clientTabHolder.SetActive(false);
        rivalTabHolder.SetActive(false);
    }
    public void GoToClienteTab()
    {
        inputConciliacionTabHolder.SetActive(false);
        clientTabHolder.SetActive(true);
        rivalTabHolder.SetActive(false);
    }
    public void GoToRivalTab()
    {
        inputConciliacionTabHolder.SetActive(false);
        clientTabHolder.SetActive(false);
        rivalTabHolder.SetActive(true);
    }

    public override void Open()
    {
        _open = true;

        computerSystem.ToggleNotification(Page.Conciliacion, false);

        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(true);
        
    }

    public override void Close()
    {
        _open = false;

        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(false);

    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
