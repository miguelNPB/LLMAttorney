using UnityEngine;

/// <summary>
/// Pagina de la fase 2 para avanzar a la audiencia previa, es decir, la fase 3
/// </summary>
public class PriorHearingPage : IPage
{
    [SerializeField] private GameObject _holder;
    public override void Open()
    {
        // activar los gameobject de la pagina
        _computerSystem.ToggleNotification(Page.ClientChat, false);

        _holder.SetActive(true);
    }

    public override void Close()
    {
        // desactivar los gameobject de la pagina
        _holder.SetActive(false);
    }
}
