using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchPricesManager : MonoBehaviour
{

    [SerializeField] private Button _searchToolButton;
    [SerializeField] private GameObject _searchMenuView;
    [SerializeField] private TMP_InputField _inputField;

    [SerializeField] private TMP_Text resultText;

    [SerializeField] private LLMConnectorQuestionChecker _llmConnectorQuestionChecker;
    [SerializeField] private LLMConnectorSearch _llmConnectorSearch;
    [SerializeField] private LLMConnectorResponseChecker _llmConnectorResponseChecker;

    private string _pendingMessage = "";
    private bool _waitingPendingMessage = false;

    private string _prompt;
    /// <summary>
    /// Corroutina para esperar al mensaje con una animacion
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoroutinePendingMessage()
    {
        float timer = 0;

        while (_waitingPendingMessage)
        {
            timer += Time.deltaTime;

            resultText.text = "";
            for (int i = 0; i < (timer % 3); i++)
                resultText.text += ".";

            yield return null;
        }

        resultText.text = _pendingMessage;

        resultText.ForceMeshUpdate();
    }

    /// <summary>
    /// Llamar para hacer la busqueda de precios. Llamara a tres LLMConnector y finalmente pondrá el resultado en resultText
    /// </summary>
    public void SearchPrice()
    {
        if (_waitingPendingMessage)
            return;

        _searchToolButton.interactable = false;
        _waitingPendingMessage = true;
        StartCoroutine(CoroutinePendingMessage());

        _prompt = _inputField.text;
        _llmConnectorQuestionChecker.SendPrompt(recieveQuestionCheckerResponse, _prompt);
    }

    private void recieveQuestionCheckerResponse(bool isCoherent)
    {
        _llmConnectorSearch.SendPrompt(recieveSearchConnectorResponse, _inputField.text);

    }

    private void recieveSearchConnectorResponse(string text)
    {
        _pendingMessage = text;
        _waitingPendingMessage = false;
    }

    private void recieveResponseCheckerResponse(bool isCoherent)
    {

    }

    /// <summary>
    /// Metodo que togglea el menu de busqueda de precios
    /// </summary>
    public void OpenMenu()
    {
        _searchMenuView.SetActive(!_searchMenuView.activeSelf);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
