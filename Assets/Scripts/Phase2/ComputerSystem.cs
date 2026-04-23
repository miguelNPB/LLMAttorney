using System;
using UnityEngine;

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

        Debug.Log("Pagina a la que se va: " +  page);

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
    /// Activa o desactiva el boton de cerrar una pagina
    /// </summary>
    /// <param name="on"></param>
    public void ToggleExitButton(bool on)
    {
        _exitButton.SetActive(on);
    }
}
