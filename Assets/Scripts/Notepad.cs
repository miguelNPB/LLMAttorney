using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Notepad : MonoBehaviour
{
    [SerializeField]
    Button notepadButton;
    [SerializeField]
    GameObject notepad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        notepadButton.onClick.AddListener(OnClick);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClick()
    {
        Debug.Log("Clicked I have been");
        notepad.gameObject.SetActive(!notepad.gameObject.activeSelf);
    }
}
