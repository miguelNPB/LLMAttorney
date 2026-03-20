using System;
using UnityEngine;

[Serializable] public enum Page { MainMenu, ChatClient }
public class ComputerSystem : MonoBehaviour
{
    public GameObject exitButton;
    public GameObject mainMenuPage;
    public PCPage chatClientPage;

    [Header("Notifications")]
    public GameObject chatClientNotification;


    private Page currentPage = Page.MainMenu;


    public void GoToMainMenu() => GoTo(Page.MainMenu);
    public void GoToClientPage() => GoTo(Page.ChatClient);

    public void GoTo(Page page)
    {
        TogglePage(currentPage, false);
        TogglePage(page, true);
        currentPage = page;

        exitButton.SetActive(page != Page.MainMenu);
    }

    public void ToggleNotification(Page page, bool on)
    {
        switch (page)
        {
            case Page.ChatClient:
                chatClientNotification.SetActive(on);
                break;
        }
    }

    private void TogglePage(Page page, bool on)
    {
        switch (page)
        {
            case Page.MainMenu:
                mainMenuPage.SetActive(on);
                break;
            case Page.ChatClient:
                if (on)
                    chatClientPage.Open();
                else
                    chatClientPage.Close();
                break;
        }
    }
}
