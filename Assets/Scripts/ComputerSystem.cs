using System;
using UnityEngine;

[Serializable] public enum Page { MainMenu, ChatClient }
public class ComputerSystem : MonoBehaviour
{
    public GameObject mainMenuPage;
    public GameObject chatClientPage;


    private Page currentPage = Page.MainMenu;


    public void GoToMainMenu() => GoTo(Page.MainMenu);
    public void GoToClientPage() => GoTo(Page.ChatClient);

    public void GoTo(Page page)
    {
        TogglePage(currentPage, false);
        TogglePage(page, true);
        currentPage = page;
    }

    private void TogglePage(Page page, bool on)
    {
        switch (page)
        {
            case Page.MainMenu:
                mainMenuPage.SetActive(on);
                break;
            case Page.ChatClient:
                chatClientPage.SetActive(on);
                break;
        }
    }
}
