using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable] public enum Page { MainMenu, ClientChat, ProcuratorChat, Documents, Spendings, Conciliation, Redaction, PriorHearing, Surrender }

/// <summary>
/// Sistema para la navegacion de la fase 2
/// </summary>
public class ComputerSystem : MonoBehaviour
{
    [SerializeField] private GameObject _exitButton;
    [SerializeField] private GameObject _mainMenuPage;
    [SerializeField] private IPage _clientChatPage;
    [SerializeField] private IPage _procuratorChatPage;
    [SerializeField] private IPage _documentsPage;
    [SerializeField] private IPage _spendingsPage;
    [SerializeField] private IPage _conciliationPage;
    [SerializeField] private IPage _redactionPage;
    [SerializeField] private IPage _priorHearingPage;
    [SerializeField] private IPage _surrenderPage;

    [Header("Notifications")]
    [SerializeField] private GameObject _clientChatNotification;
    [SerializeField] private GameObject _procuratorChatNotification;
    [SerializeField] private GameObject _documentsNotification;
    [SerializeField] private GameObject _spendingsNotification;
    [SerializeField] private GameObject _conciliationNotification;
    [SerializeField] private GameObject _redactionNotification;
    [SerializeField] private GameObject _priorHearingNotification;
    [SerializeField] private GameObject _surrenderNotification;
    [Header("Overlay Notifications")]
    [SerializeField] private GameObject _overlayNotification1;
    [SerializeField] private GameObject _overlayNotification2;
    [SerializeField] private GameObject _overlayNotification3;


    private TMP_Text _overlayNotificationText1;
    private TMP_Text _overlayNotificationText2;
    private TMP_Text _overlayNotificationText3;
    private Animator _overlayNotificationAnimator1;
    private Animator _overlayNotificationAnimator2;
    private Animator _overlayNotificationAnimator3;
    [SerializeField] private bool _overlayNotificationPlaying1 = false;
    [SerializeField] private bool _overlayNotificationPlaying2 = false;
    [SerializeField] private bool _overlayNotificationPlaying3 = false;
    private Page currentPage = Page.MainMenu;


    public void GoToMainMenu() => GoTo(Page.MainMenu);
    public void GoToClientChat() => GoTo(Page.ClientChat);
    public void GoToProcuratorChat() => GoTo(Page.ProcuratorChat);
    public void GoToDocuments() => GoTo(Page.Documents);
    public void GoToSpendings() => GoTo(Page.Spendings);
    public void GoToConciliation() => GoTo(Page.Conciliation);
    public void GoToRedaction() => GoTo(Page.Redaction);
    public void GoToPriorHearing() => GoTo(Page.PriorHearing);
    public void GoToSurrender() => GoTo(Page.Surrender);


    /// <summary>
    /// Navega a la pagina deseada, si hay una abierta, se cierra
    /// </summary>
    /// <param name="page"></param>
    public void GoTo(Page page)
    {
        TogglePage(currentPage, false);
        TogglePage(page, true);
        currentPage = page;

        _exitButton.SetActive(page != Page.MainMenu);
    }

    /// <summary>
    /// Activa o desactiva el gameobject de la notificacion asociada a la pagina pasada en parametro
    /// </summary>
    /// <param name="page"></param>
    /// <param name="on"></param>
    public void ToggleNotification(Page page, bool on)
    {
        switch (page)
        {
            case Page.ClientChat:
                _clientChatNotification.SetActive(on);
                break;
            case Page.ProcuratorChat:
                _procuratorChatNotification.SetActive(on);
                break;
            case Page.Documents:
                _documentsNotification.SetActive(on);
                break;
            case Page.Spendings:
                _spendingsNotification.SetActive(on);
                break;
            case Page.Conciliation:
                _conciliationNotification.SetActive(on);
                break;
            case Page.Redaction:
                _redactionNotification.SetActive(on);
                break;
            case Page.PriorHearing:
                _priorHearingNotification.SetActive(on);
                break;
            case Page.Surrender:
                _surrenderNotification.SetActive(on);
                break;
        }
    }

    /// <summary>
    /// Llama a los metodos de PCPage Open o Close de la pagina pasada en parametro
    /// </summary>
    /// <param name="page"></param>
    /// <param name="on"></param>
    private void TogglePage(Page page, bool on)
    {
        switch (page)
        {
            case Page.MainMenu:
                _mainMenuPage.SetActive(on);
                break;
            case Page.ClientChat:
                if (on) _clientChatPage.Open(); else _clientChatPage.Close();
                break;
            case Page.ProcuratorChat:
                if (on) _procuratorChatPage.Open(); else _procuratorChatPage.Close();
                break;
            case Page.Documents:
                if (on) _documentsPage.Open(); else _documentsPage.Close();
                break;
            case Page.Spendings:
                if (on) _spendingsPage.Open(); else _spendingsPage.Close();
                break;
            case Page.Conciliation:
                if (on) _conciliationPage.Open(); else _conciliationPage.Close();
                break;
            case Page.Redaction:
                if (on) _redactionPage.Open(); else _redactionPage.Close();
                break;
            case Page.PriorHearing:
                if (on) _priorHearingPage.Open(); else _priorHearingPage.Close();
                break;
            case Page.Surrender:
                if (on) _surrenderPage.Open(); else _surrenderPage.Close();
                break;
        }
    }


    /// <summary>
    /// Aparece una notificacion sobreponiendo a las paginas con un texto a elegir
    /// </summary>
    /// <param name="text"></param>
    public void PingOverlayNotification(string text)
    {
        // elegir notificacion que no este siendo usada
        if (!_overlayNotificationPlaying1)
        {
            _overlayNotificationPlaying1 = true;
            _overlayNotificationText1.text = text;
            _overlayNotificationAnimator1.Play("Ping");
        }
        else if (!_overlayNotificationPlaying2)
        {
            _overlayNotificationPlaying2 = true;
            _overlayNotificationText2.text = text;
            _overlayNotificationAnimator2.Play("Ping");
        }
        else if (!_overlayNotificationPlaying3)
        {
            _overlayNotificationPlaying3 = true;
            _overlayNotificationText3.text = text;
            _overlayNotificationAnimator3.Play("Ping");
        }
    }

    /// <summary>
    /// Activa o desactiva el boton de cerrar una pagina
    /// </summary>
    /// <param name="on"></param>
    public void ToggleExitButton(bool on)
    {
        _exitButton.SetActive(on);
    }

    private void Update()
    {
        if (_overlayNotificationPlaying1)
            _overlayNotificationPlaying1 = _overlayNotificationAnimator1.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;
        if (_overlayNotificationPlaying2)
            _overlayNotificationPlaying2 = _overlayNotificationAnimator2.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;
        if (_overlayNotificationPlaying3)
            _overlayNotificationPlaying3 = _overlayNotificationAnimator3.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;

    }

    private void Start()
    {
        _overlayNotificationText1 = _overlayNotification1.GetComponentInChildren<TMP_Text>();
        _overlayNotificationText2 = _overlayNotification2.GetComponentInChildren<TMP_Text>();
        _overlayNotificationText3 = _overlayNotification3.GetComponentInChildren<TMP_Text>();
        _overlayNotificationAnimator1 = _overlayNotification1.GetComponent<Animator>();
        _overlayNotificationAnimator2 = _overlayNotification2.GetComponent<Animator>();
        _overlayNotificationAnimator3 = _overlayNotification3.GetComponent<Animator>();
    }
}
