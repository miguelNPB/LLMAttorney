using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pagina para gestionar la conciliacion
/// </summary>
public class ConciliationPage : IPage
{
    [SerializeField] private Phase2Manager _phase2Manager;
    [SerializeField] private LLMConnectorConciliation _llmConnector;
    [SerializeField] private Animator _clientCharacterAnimator;
    [SerializeField] private Animator _rivalCharacterAnimator;
    [SerializeField] private Button _sendButton;
    [SerializeField] private Button _changeToTextTab;
    [SerializeField] private Button _changeToClientTabButton;
    [SerializeField] private Button _changeToRivalTabButton;
    [SerializeField] private GameObject _clientTabExclamation;
    [SerializeField] private GameObject _rivalTabExclamation;
    [SerializeField] private TMP_InputField _inputFieldText;
    [SerializeField] private GameObject _popupClientRejects;
    [SerializeField] private GameObject _inputConciliationTabHolder;
    [SerializeField] private GameObject _clientTabHolder;
    [SerializeField] private GameObject _rivalTabHolder;
    [SerializeField] private TMP_Text clienteAnswerText;
    [SerializeField] private TMP_Text rivalAnswerText;
    [SerializeField] private GameObject popupAfterFailedConciliation;
    [SerializeField] private GameObject popupAfterSuccessfulConciliation;

    private bool _open = false;
    private bool _clientAgrees = false;
    private bool _rivalAgrees = false;
    private string _clientAnswer = "";
    private string _rivalAnswer = "";

    public void SetClientAgrees(bool value, string answer)
    {
        _clientAgrees = value;
        _clientAnswer = answer;
    }
    public void SetRivalAgrees(bool value, string answer)
    {
        _rivalAgrees = value;
        _rivalAnswer = answer;
    }

    // Llamado al pulsar el boton de mandar intento de conciliacion
    public void SendAttempt()
    {
        _sendButton.interactable = false;

        SendProposition();
    }

    // llamado cuando se manda la proposicion, se desactiva el boton y la escritura en el input
    private void SendProposition()
    {
        _popupClientRejects.SetActive(false);
        _inputFieldText.interactable = false;
        EnableClientResponse();
    }

    // pasa si no acepta el cliente
    private void RestartProposition()
    {
        _popupClientRejects.SetActive(true);
        _inputFieldText.interactable = true;
        _sendButton.interactable = true;
        _inputFieldText.text = "";
    }

    // activa la pestaña de respuesta de cliente y lo manda
    private void EnableClientResponse()
    {
        _clientTabExclamation.SetActive(true);
        _changeToClientTabButton.interactable = true;

        // mandar a LLM prompt para que responda el cliente
        StartCoroutine(PromptClientResponse());
    }



    private IEnumerator PromptClientResponse()
    {
        _clientCharacterAnimator.SetTrigger("Thinking");

        yield return StartCoroutine(_llmConnector.SendClientPrompt());

        clienteAnswerText.text = _clientAnswer;

        if (!_open)
        {
            _computerSystem.PingOverlayNotification("¡Has recibido la contestacion del cliente a la conciliacion!");
            _computerSystem.ToggleNotification(Page.Conciliation, true);
        }


        if (_clientAgrees)
        {
            _clientCharacterAnimator.SetTrigger("Success");
            EnableRivalResponse();
        }
        else
        {
            _clientCharacterAnimator.SetTrigger("Rejection");

            RestartProposition();
        }
    }

    private void EnableRivalResponse()
    {
        _rivalTabExclamation.SetActive(true);
        _changeToRivalTabButton.interactable = true;

        // mandar a LLM prompt para que responda el rival
        StartCoroutine(PromptRivalResponse());
    }

    private IEnumerator PromptRivalResponse()
    {
        _rivalCharacterAnimator.SetTrigger("Thinking");

        float random = Random.Range(0.0f, 1.0f);

        if (random > GameSystem.Instance.CaseData.conciliationRivalInstantRejectProbability)
        {
            yield return StartCoroutine(_llmConnector.SendRivalPromptNormal());
        }
        else
        {
            yield return StartCoroutine(_llmConnector.SendRivalPromptRejectionConfirmed());
        }

        rivalAnswerText.text = _rivalAnswer;

        if (!_open)
        {
            _computerSystem.PingOverlayNotification("¡Has recibido la contestacion del rival a la conciliacion!");
            _computerSystem.ToggleNotification(Page.Conciliation, true);
        }


        if (_rivalAgrees)
        {
            _rivalCharacterAnimator.SetTrigger("Success");
            popupAfterSuccessfulConciliation.SetActive(true);
            _computerSystem.ToggleExitButton(false);
        }
        else
        {
            _rivalCharacterAnimator.SetTrigger("Rejection");
            popupAfterFailedConciliation.SetActive(true);
            _phase2Manager.FailedConciliation();
            _computerSystem.ToggleNotification(Page.Redaction, true);
        }
    }



    public void GoToInputConciliationTab()
    {
        _inputConciliationTabHolder.SetActive(true);
        _clientTabHolder.SetActive(false);
        _rivalTabHolder.SetActive(false);
    }
    public void GoToClienteTab()
    {
        _inputConciliationTabHolder.SetActive(false);
        _clientTabHolder.SetActive(true);
        _rivalTabHolder.SetActive(false);
    }
    public void GoToRivalTab()
    {
        _inputConciliationTabHolder.SetActive(false);
        _clientTabHolder.SetActive(false);
        _rivalTabHolder.SetActive(true);
    }

    public override void Open()
    {
        _open = true;

        _computerSystem.ToggleNotification(Page.Conciliation, false);

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
