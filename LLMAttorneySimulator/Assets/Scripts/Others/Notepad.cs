using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase para getsionar el comportamiento del bloc de notas
/// </summary>
public class Notepad : MonoBehaviour
{
    [SerializeField]
    Button notepadButton;
    [SerializeField]
    GameObject notepad;
    [SerializeField]
    Vector3 startingPos;
    [SerializeField]
    Vector3 openPos;
    [SerializeField]
    float moveSpeed = 5f;
    [SerializeField] private TMP_InputField _inputField;

    private bool isAtEndingPos = false;
    private bool isMoving = false;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float lerpProgress = 0f;

    /// <summary>
    /// Activa o desactiva notepad
    /// </summary>
    /// <param name="on"></param>
    public void ToggleNotepad(bool on)
    {
        notepadButton.gameObject.SetActive(on);
        notepad.transform.localPosition = startingPos;
        notepad.SetActive(on);
    }

    /// <summary>
    /// vacia el texto del notepad
    /// </summary>
    public void ResetText()
    {
        _inputField.text = "";
    }

    /// <summary>
    /// Llamado al clicar el bloc
    /// </summary>
    private void onClick()
    {
        
        if (isAtEndingPos)
        {
            // Move back to starting position
            startPos = notepad.transform.localPosition;
            targetPos = startingPos;
            isAtEndingPos = false;
        }
        else
        {
            // Move to ending position
            startPos = notepad.transform.localPosition;
            targetPos = openPos;
            isAtEndingPos = true;
        }
        
        lerpProgress = 0f;
        isMoving = true;
    }

    void Start()
    {
        notepadButton.onClick.AddListener(onClick);
        notepad.transform.localPosition = startingPos;
    }

    void Update()
    {
        if (isMoving)
        {
            lerpProgress += moveSpeed * Time.deltaTime;
            notepad.transform.localPosition = Vector3.Lerp(startPos, targetPos, lerpProgress);

            if (lerpProgress >= 1f)
            {
                notepad.transform.localPosition = targetPos;
                isMoving = false;
                lerpProgress = 0f;
            }
        }
    }
}
