using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        GameSystem.Instance.ResetNotepad();
        GameSystem.Instance.ToggleNotepad(false);
    }
}
