using UnityEngine;

/// <summary>
/// Manager para la fase 1 encargado de iniciar la conversación del cliente dado el caso usado
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
