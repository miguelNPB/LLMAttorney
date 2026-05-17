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
    private string _tempAnswer;
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

    /// <summary>
    /// Termina la busqueda de precios y pone un texto final de respuesta.
    /// </summary>
    /// <param name="text"></param>
    private void endPriceSearch(string text)
    {
        _waitingPendingMessage = false;
        _pendingMessage = text;
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta del primer LLMConector, el de QuestionChecker. Devuelve un booleano si la pregunta es coherente
    /// </summary>
    /// <param name="isCoherent"></param>
    private void recieveQuestionCheckerResponse(bool isCoherent)
    {
        Debug.Log("1");
        if (isCoherent)
            _llmConnectorSearch.SendPrompt(recieveSearchConnectorResponse, _inputField.text);
        else
            endPriceSearch("Lo siento. No he entendido tu petición, deme mas detalles o cambie la forma de pedirlo.");
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta dle LLMConnector de buscador de precios.
    /// </summary>
    /// <param name="text"></param>
    private void recieveSearchConnectorResponse(string text)
    {
        _tempAnswer = text;
        Debug.Log("2");
        _llmConnectorResponseChecker.SendPrompt(recieveResponseCheckerResponse, _tempAnswer);
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta del ultimo LLMConnector, el ResponseChecker. Devuelve un booleano si la respuesta es coherente.
    /// </summary>
    /// <param name="isCoherent"></param>
    private void recieveResponseCheckerResponse(bool isCoherent)
    {
        Debug.Log("3");
        if (isCoherent)
            endPriceSearch(_tempAnswer);
        else
            endPriceSearch("Lo siento. No he podido encontrar nada de tu petición. Formule de nuevo la pregunta o pruebe algo distinto.");
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
