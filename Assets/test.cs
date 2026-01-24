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


    /// <summary>
    /// Genera el caso juridico inicial
    /// </summary>
    public void GenerateCase()
    {

    }

    /// <summary>
    /// Metodo de prueba de mandar prompts
    /// </summary>
    public void SendPrompt()
    {
        llm.SendPrompt(apiType, promptField.text, llmConfigField.text, temperature, max_length, PrintPrompt);

        promptField.text = "";
        llmConfigField.text = "";
    }
    /// <summary>
    /// Printea el prompt al canvas
    /// </summary>
    /// <param name="success">True si ha salido bien</param>
    /// <param name="text">El texot</param>
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
