using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private bool isAtEndingPos = false;
    private bool isMoving = false;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float lerpProgress = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        notepadButton.onClick.AddListener(OnClick);
        notepad.transform.localPosition = startingPos;
    }
    

    // Update is called once per frame
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

    void OnClick()
    {
        Debug.Log("Clicked I have been");
        
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
}
