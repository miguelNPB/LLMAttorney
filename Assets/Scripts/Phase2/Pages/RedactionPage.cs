using UnityEngine;

public class RedactionPage : PCPage
{
    public override void Open()
    {
        // activar los gameobject de la pagina
        computerSystem.ToggleNotification(Page.ChatCliente, false);

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
