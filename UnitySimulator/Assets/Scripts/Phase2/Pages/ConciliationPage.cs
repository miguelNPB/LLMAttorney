using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pagina para gestionar la conciliacion
/// </summary>
public class ConciliationPage : IPage
{
    [SerializeField] private Phase2Manager _phase2Manager;
    [SerializeField] private LLMConnectorConciliationAgreeBool _llmConnectorAgreeBool;
    [SerializeField] private LLMConnectorConciliationAgreeText _llmConnectorAgreeText;
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
    [SerializeField] private TMP_Text _clienteAnswerText;
    [SerializeField] private TMP_Text _rivalAnswerText;
    [SerializeField] private GameObject _popupAfterFailedConciliation;
    [SerializeField] private GameObject _popupAfterSuccessfulConciliation;

    private string _conciliationProposition;
    private bool _clientAgrees;
    private bool _rivalAgrees;

    private bool _open = false;


    /// <summary>
    /// Llamado al pulsar el boton de mandar intento de conciliacion
    /// </summary>
    public void SendAttempt()
    {
        _sendButton.interactable = false;

        sendProposition();
    }

    /// <summary>
    /// llamado cuando se manda la proposicion, se desactiva el boton y la escritura en el input
    /// </summary>
    private void sendProposition()
    {
        _popupClientRejects.SetActive(false);
        _inputFieldText.interactable = false;
        _conciliationProposition = _inputFieldText.text;

        _clientTabExclamation.SetActive(true);
        _changeToClientTabButton.interactable = true;
        _clientCharacterAnimator.SetTrigger("Thinking");

        // mandar a LLM prompt para que responda el cliente
        sendClientProposition();
    }
    
    /// <summary>
    /// Se llama si el cliente no acepta, reinicia el sistema de conciliacion
    /// </summary>
    private void restartProposition()
    {
        _popupClientRejects.SetActive(true);
        _inputFieldText.interactable = true;
        _sendButton.interactable = true;
        _inputFieldText.text = "";
    }

    /// <summary>
    /// Se llama si algo falla con la comunicacion al servidor
    /// </summary>
    /// <param name="text"></param>
    private void errorResponse(string text)
    {
        restartProposition();
        _clienteAnswerText.text = "Error con el servidor: " + text;
    }

    /// <summary>
    /// manda el prompt del cliente.
    /// </summary>
    private void sendClientProposition()
    {
        _llmConnectorAgreeBool.SendPrompt(recieveClientBoolAnswer, errorResponse, _conciliationProposition, true);
    }

    /// <summary>
    /// manda el prompt del rival. Tambien hace una tirada de probabilidad de si debe rechazar instantaneamente o no
    /// </summary>
    private void sendRivalProposition()
    {
        // mandar prompt rival
        float random = Random.Range(0.0f, 1.0f);
        // instant rejection
        if (random < GameSystem.Instance.CaseData.conciliationRivalInstantRejectProbability)
        {
            recieveRivalBoolAnswer(false);
        }
        else
        {
            _llmConnectorAgreeBool.SendPrompt(recieveRivalBoolAnswer, errorResponse, _conciliationProposition, false);
        }
    }


    /// <summary>
    /// Recibe la respuesta del cliente en booleano de si concuerda con la proposicion o no, manda el prompt a sacar el texto
    /// </summary>
    /// <param name="agree"></param>
    private void recieveClientBoolAnswer(bool agree)
    {
        _clientAgrees = agree;
        _llmConnectorAgreeText.SendPrompt(recieveClientTextAnswer, errorResponse, _conciliationProposition, agree, true);
    }

    /// <summary>
    /// Recibe la respuesta de texto del cliente explicando porque concuerda o no concuerda con la proposicion
    /// </summary>
    /// <param name="text"></param>
    private void recieveClientTextAnswer(string text)
    {
        if (!_open)
        {
            _computerSystem.PingOverlayNotification("¡Has recibido la contestacion del cliente a la conciliacion!");
            _computerSystem.ToggleNotification(Page.Conciliation, true);
        }

        // actualizar animator
        _clientCharacterAnimator.SetTrigger(_clientAgrees ? "Success" : "Rejection");
        _clienteAnswerText.text = text;

        if (_clientAgrees)
        {
            _rivalTabExclamation.SetActive(true);
            _changeToRivalTabButton.interactable = true;
            _rivalCharacterAnimator.SetTrigger("Thinking");

            sendRivalProposition();
        }
        else
        {
            restartProposition();
        }
    }

    /// <summary>
    /// Recibe la respuesta del RIVAL en booleano de si concuerda con la proposicion o no, manda el prompt a sacar el texto
    /// </summary>
    /// <param name="agree"></param>
    private void recieveRivalBoolAnswer(bool agree)
    {
        _rivalAgrees = agree;
        _llmConnectorAgreeText.SendPrompt(recieveRivalClientTextAnswer, errorResponse, _conciliationProposition, agree, false);
    }

    /// <summary>
    /// Recibe la respuesta de texto del rival explicando porque concuerda o no concuerda con la proposicion
    /// </summary>
    /// <param name="text"></param>
    private void recieveRivalClientTextAnswer(string text)
    {
        if (!_open)
        {
            _computerSystem.PingOverlayNotification("¡Has recibido la contestacion del rival a la conciliacion!");
            _computerSystem.ToggleNotification(Page.Conciliation, true);
        }

        // actualizar animator
        _rivalCharacterAnimator.SetTrigger(_rivalAgrees ? "Success" : "Rejection");
        _rivalAnswerText.text = text;

        if (_rivalAgrees)
        {
            _rivalCharacterAnimator.SetTrigger("Success");
            _popupAfterSuccessfulConciliation.SetActive(true);
            _computerSystem.ToggleExitButton(false);
        }
        else
        {
            _rivalCharacterAnimator.SetTrigger("Rejection");
            _popupAfterFailedConciliation.SetActive(true);
            _phase2Manager.FailedConciliation();
            _computerSystem.ToggleNotification(Page.Redaction, true);
        }
    }


    /// <summary>
    /// Abrir el menu de input de propuesta de conciliacion
    /// </summary>
    public void GoToInputConciliationTab()
    {
        _inputConciliationTabHolder.SetActive(true);
        _clientTabHolder.SetActive(false);
        _rivalTabHolder.SetActive(false);
    }

    /// <summary>
    /// Abrir el menu de respuseta del cliente
    /// </summary>
    public void GoToClienteTab()
    {
        _inputConciliationTabHolder.SetActive(false);
        _clientTabHolder.SetActive(true);
        _rivalTabHolder.SetActive(false);
    }

    /// <summary>
    /// Abrir el menu de respuesta del rival
    /// </summary>
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
}
