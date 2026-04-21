using UnityEngine;

/// <summary>
/// Pagina para abandonar el caso y volver al menu princpial
/// </summary>
public class SurrenderPage : IPage
{
    /// <summary>
    /// Abandona el caso y volver al menu principal
    /// </summary>
    public void Surrender()
    {
        SceneSystem.Instance.LoadMainMenu();
    }

    public override void Open()
    {
        // activar los gameobject de la pagina
        computerSystem.ToggleNotification(Page.ClientChat, false);

        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(true);
    }

    public override void Close()
    {
        // desactivar los gameobject de la pagina
        for (int i = 0; i < gameObject.transform.childCount; i++)
            gameObject.transform.GetChild(i).gameObject.SetActive(false);
    }
}
