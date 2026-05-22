using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pagina de la fase 2 para avanzar a la audiencia previa, es decir, la fase 3
/// </summary>
public class PriorHearingPage : IPage
{
    [SerializeField] private GameObject _holder;
    [SerializeField] private GameObject _warningText;
    [SerializeField] private Button _changePhaseButton;

    /// <summary>
    /// Llamado al recibir todos los documentos del rival
    /// </summary>
    public void AllRivalDocsRecieved()
    {
        _warningText.SetActive(false);
        _changePhaseButton.interactable = true;
    }

    public override void Open()
    {
        _computerSystem.ToggleNotification(Page.PriorHearing, false);
        // activar los gameobject de la pagina

        _holder.SetActive(true);
    }

    public override void Close()
    {
        // desactivar los gameobject de la pagina
        _holder.SetActive(false);
    }
}
