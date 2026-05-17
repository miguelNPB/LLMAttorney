using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISearchManager : MonoBehaviour
{

    [SerializeField] private Button _searchToolButton;
    [SerializeField] private GameObject _searchMenuView;
    [SerializeField] private TMP_InputField _inputField;

    [SerializeField] private TMP_Text resultText;

    [SerializeField] private LLMConnectorSearch _llmConnectorSearch;

    private string pendingMessage = "";
    private bool waitingPendingMessage = false;

    /// <summary>
    /// Corroutina para esperar al mensaje con una animacion
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoroutinePendingMessage()
    {
        float timer = 0;

        waitingPendingMessage = true;

        while (waitingPendingMessage)
        {
            timer += Time.deltaTime;

            resultText.text = "";
            for (int i = 0; i < (timer % 3); i++)
                resultText.text += ".";

            yield return null;
        }

        resultText.text = pendingMessage;

        resultText.ForceMeshUpdate();
    }

    /// <summary>
    /// Llamar para hacer la busqueda de precios. Llamara a tres LLMConnector y finalmente pondrá el resultado en resultText
    /// </summary>
    public void SearchPrice()
    {
        if (waitingPendingMessage)
            return;

        _searchToolButton.interactable = false;
        StartCoroutine(CoroutinePendingMessage());

        _llmConnectorSearch.SendPrompt(recieveSearchConnectorResponse, _inputField.text);

    }

    private void recieveSearchConnectorResponse(string text)
    {
        pendingMessage = text;
        waitingPendingMessage = false;
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
