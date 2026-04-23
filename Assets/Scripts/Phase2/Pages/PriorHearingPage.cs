using UnityEngine;

/// <summary>
/// Pagina de la fase 2 para avanzar a la audiencia previa, es decir, la fase 3
/// </summary>
public class PriorHearingPage : IPage
{
    public override void Open()
    {
        // activar los gameobject de la pagina
        _computerSystem.ToggleNotification(Page.ClientChat, false);

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
