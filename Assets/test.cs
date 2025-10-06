using UnityEngine;

public class test : MonoBehaviour
{
    public string prompt;

    private GeminiAPI gemini;


    void DebugPrompt(string text)
    {
        Debug.Log(text);
    }
    void Start()
    {
        gemini = GetComponent<GeminiAPI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gemini.SendPrompt(prompt, DebugPrompt);
        }
    }
}
