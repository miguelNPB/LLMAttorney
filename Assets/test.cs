using UnityEngine;

public class test : MonoBehaviour
{
    private LLMAttorney_API llm;
    public API_TYPE apiType;
    public string llmConfig;
    public string prompt;

    void DebugPrompt(bool success, string text)
    {
        Debug.Log(text);
    }
    void Start()
    {
        llm = LLMAttorney_API.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            llm.SendPrompt(apiType, prompt, llmConfig, DebugPrompt);
        }
    }
}
