using UnityEngine;

/// <summary>
/// Manager para la fase 1
/// </summary>
public class Phase1Manager : MonoBehaviour
{
    public WriteTextSystem writeTextSystem;

    void Start()
    {
        writeTextSystem.WriteText(GameSystem.Instance.CaseData.initialClientSpeech);
        GameSystem.Instance.ToggleNotepad(true);
    }
}
