using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Phase2Manager : MonoBehaviour
{
    [SerializeField] private Button _priorHearingButton;
    [SerializeField] private Button _redactLawsuitButton;

    /// <summary>
    /// Se llama una vez se ha mandado la demanda al procurador y la devuelve completa y lista sin quejas
    /// Además se tiene que haber intentado la conciliacion
    /// </summary>
    /// <param name="on"></param>
    public void EnablePriorHearing(bool on)
    {
        _priorHearingButton.interactable = on;
    }

    /// <summary>
    /// Se llama una vez se ha intentado un acuerdo
    /// </summary>
    /// <param name="on"></param>
    public void EnableRedactLawsuit(bool on)
    {
        _redactLawsuitButton.interactable = on;
    }


    public void SuccesfulConciliation()
    {
        // volver al menu 
        GameSystem.Instance.ResetCaseData();
        SceneSystem.Instance.LoadMainMenu();
    }

    public void FailedConciliation()
    {
        EnableRedactLawsuit(true);
        GameSystem.Instance.CaseData.AttemptConciliacion();
    }

    public void ForceChangePhase()
    {
        SceneSystem.Instance.LoadPhase3();
    }
}
