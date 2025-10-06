using UnityEngine;

public class test : MonoBehaviour
{
    public API_TYPE apiType;
    public string llmConfig;
    public string prompt;

    private LLMAttorney_API llm;


    void DebugPrompt(bool success, string text)
    {
        Debug.Log(text);
    }
    void Start()
    {
        llm = GetComponent<LLMAttorney_API>();
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
