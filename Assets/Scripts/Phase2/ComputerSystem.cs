using System;
using UnityEngine;

[Serializable] public enum Page { MainMenu, ChatCliente, ChatProcurador, Documentos, Gastos, Conciliacion, Redaccion, AudienciaPrevia, AbandonarCaso }
public class ComputerSystem : MonoBehaviour
{
    public GameObject exitButton;
    public GameObject mainMenuPage;
    public PCPage chatClientePage;
    public PCPage chatProcuradorPage;
    public PCPage documentosPage;
    public PCPage gastosPage;
    public PCPage conciliacionPage;
    public PCPage redaccionPage;
    public PCPage audienciaPreviaPage;
    public PCPage abandonarCasoPage;

    [Header("Notifications")]
    public GameObject chatClientNotification;
    public GameObject chatProcuradorNotification;
    public GameObject documentosNotification;
    public GameObject gastosNotification;
    public GameObject conciliacionNotification;
    public GameObject redaccionNotification;
    public GameObject audienciaPreviaNotification;
    public GameObject abandonarCasoNotification;


    private Page currentPage = Page.MainMenu;


    public void GoToMainMenu() => GoTo(Page.MainMenu);
    public void GoToClientPage() => GoTo(Page.ChatCliente);
    public void GoToChatProcurador() => GoTo(Page.ChatProcurador);
    public void GoToDocumentos() => GoTo(Page.Documentos);
    public void GoToGastos() => GoTo(Page.Gastos);
    public void GoToConciliacion() => GoTo(Page.Conciliacion);
    public void GoToRedaccion() => GoTo(Page.Redaccion);
    public void GoToAudienciaPrevia() => GoTo(Page.AudienciaPrevia);
    public void GoToAbandonarCaso() => GoTo(Page.AbandonarCaso);


    /// <summary>
    /// Navega a la pagina deseada, si hay una abierta, se cierra
    /// </summary>
    /// <param name="page"></param>
    public void GoTo(Page page)
    {
        TogglePage(currentPage, false);
        TogglePage(page, true);
        currentPage = page;

        exitButton.SetActive(page != Page.MainMenu);
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
            case Page.ChatCliente:
                chatClientNotification.SetActive(on);
                break;
            case Page.ChatProcurador:
                chatProcuradorNotification.SetActive(on);
                break;
            case Page.Documentos:
                documentosNotification.SetActive(on);
                break;
            case Page.Gastos:
                gastosNotification.SetActive(on);
                break;
            case Page.Conciliacion:
                conciliacionNotification.SetActive(on);
                break;
            case Page.Redaccion:
                redaccionNotification.SetActive(on);
                break;
            case Page.AudienciaPrevia:
                audienciaPreviaNotification.SetActive(on);
                break;
            case Page.AbandonarCaso:
                abandonarCasoNotification.SetActive(on);
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
                mainMenuPage.SetActive(on);
                break;
            case Page.ChatCliente:
                if (on) chatClientePage.Open(); else chatClientePage.Close();
                break;
            case Page.ChatProcurador:
                if (on) chatProcuradorPage.Open(); else chatProcuradorPage.Close();
                break;
            case Page.Documentos:
                if (on) documentosPage.Open(); else documentosPage.Close();
                break;
            case Page.Gastos:
                if (on) gastosPage.Open(); else gastosPage.Close();
                break;
            case Page.Conciliacion:
                if (on) conciliacionPage.Open(); else conciliacionPage.Close();
                break;
            case Page.Redaccion:
                if (on) redaccionPage.Open(); else redaccionPage.Close();
                break;
            case Page.AudienciaPrevia:
                if (on) audienciaPreviaPage.Open(); else audienciaPreviaPage.Close();
                break;
            case Page.AbandonarCaso:
                if (on) abandonarCasoPage.Open(); else abandonarCasoPage.Close();
                break;
        }
    }
}
