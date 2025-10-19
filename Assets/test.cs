using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    private LLMAttorney_API llm;
    public API_TYPE apiType;
    [Range(0, 1f)]
    public float temperature = 0.8f;
    public int max_length = 9999;

    [SerializeField] private TMP_Text outputText;
    [SerializeField] private TMP_InputField promptField;
    [SerializeField] private TMP_InputField llmConfigField;
    public void SendPrompt()
    {
        llm.SendPrompt(apiType, promptField.text, llmConfigField.text, temperature, max_length, PrintPrompt);

        promptField.text = "";
        llmConfigField.text = "";
    }
    void PrintPrompt(bool success, string text)
    {
        outputText.text = text;
    }
    void Start()
    {
        llm = LLMAttorney_API.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
