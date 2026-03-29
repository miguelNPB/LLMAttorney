using UnityEngine;
using UnityEngine.UI;

public class Phase2 : MonoBehaviour
{
    [SerializeField] private Button audienciaPreviaButton;

    /// <summary>
    /// Se llama una vez se ha mandado la demanda al procurador y la devuelve completa y lista sin quejas
    /// Además se tiene que haber intentado la conciliacion
    /// </summary>
    /// <param name="on"></param>
    public void EnableAudienciaPrevia(bool on)
    {
        audienciaPreviaButton.interactable = on;
    }
}
