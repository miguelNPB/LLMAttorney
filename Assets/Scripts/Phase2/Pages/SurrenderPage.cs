using UnityEngine;

/// <summary>
/// Pagina para abandonar el caso y volver al menu princpial
/// </summary>
public class SurrenderPage : IPage
{
    [SerializeField] private GameObject _holder;
    /// <summary>
    /// Abandona el caso y volver al menu principal
    /// </summary>
    public void Surrender()
    {
        SceneSystem.Instance.LoadMainMenu();
    }

    public override void Open()
    {
        _computerSystem.ToggleNotification(Page.ClientChat, false);

        _holder.SetActive(true);
    }

    public override void Close()
    {
        _holder.SetActive(true);
    }
}
