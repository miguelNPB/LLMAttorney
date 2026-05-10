using UnityEngine;

public class Phase1 : MonoBehaviour
{
    public WriteTextSystem writeTextSystem;

    [SerializeField, TextArea(3, 10)]
    private string _startingText;

    void Start()
    {
        writeTextSystem.WriteText(_startingText);
        GameSystem.Instance.ToggleNotepad(true);
    }
}
