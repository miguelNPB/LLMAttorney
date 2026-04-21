using UnityEngine;

/// <summary>
/// Pagina para mostrar los gastos efectuados durante el caso
/// </summary>
public class SpendingsPage : IPage
{
    public override void Open()
    {
        // activar los gameobject de la pagina
        computerSystem.ToggleNotification(Page.Spendings, false);

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
