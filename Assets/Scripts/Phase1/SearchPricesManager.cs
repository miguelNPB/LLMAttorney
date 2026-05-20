using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase enfocada en el control y relacion entre la UI del buscador de presupuestos y los conectores implicados en el rol, y el proceso 
/// de generación de la respuesta.
/// </summary>
public class SearchPricesManager : MonoBehaviour
{

    #region UI
    [SerializeField] private Button _searchToolButton;
    [SerializeField] private GameObject _searchMenuView;
    [SerializeField] private TMP_InputField _inputField;

    [SerializeField] private TMP_Text resultText;
    #endregion

    #region Conectores
    [SerializeField] private LLMConnectorTextChecker _llmConnectorResponseChecker;
    [SerializeField] private LLMConnectorSearch _llmConnectorSearch;
    #endregion

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
    /// Metodo de llamada inicial para comenzar el proceso de busqueda de presupuesto por la herramienta. Se encarga de desactivar la UI necesaria
    /// hasta la obtención de la respuesta y de iniciar el proceso al enviar la primera peticion del conjunto de conectores al servidor.
    /// </summary>
    public void SearchPrice()
    {
        if (_waitingPendingMessage)
        {
            return;
        }         

        _waitingPendingMessage = true;
        StartCoroutine(CoroutinePendingMessage());

        _prompt = _inputField.text;
        _llmConnectorResponseChecker.SendPrompt(recieveQuestionCheckerResponse, endPriceSearch, _prompt, 0);
    }

    /// <summary>
    /// Termina la busqueda de precios y pone un texto final de respuesta.
    /// </summary>
    /// <param name="text">Texto final que se escribe en el buscador tras el fin del proceso</param>
    private void endPriceSearch(string text)
    {
        _waitingPendingMessage = false;
        _pendingMessage = text;
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta del TextChecker enfocado en el analisis del texto enviado por el usuario. Devuelve un booleano 
    /// si la pregunta es coherente
    /// </summary>
    /// <param name="isCoherent">Indica si la pregunta se relaciona al derecho civil y el coste de servicios o productos del proceso</param>
    private void recieveQuestionCheckerResponse(bool isCoherent)
    {
        if (isCoherent)
        {
            _llmConnectorSearch.SendPrompt(recieveSearchConnectorResponse, endPriceSearch, _inputField.text);
        }
        else
        {
            endPriceSearch("Lo siento. No he entendido tu petición, deme mas detalles o cambie la forma de pedirlo.");
        }        
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta del SearchConnector, obteniendo el texto generado por el LLM.
    /// </summary>
    /// <param name="text">Texto que indica el coste de un producto o servicio del ambito del derecho civil</param>
    private void recieveSearchConnectorResponse(string text)
    {
        _tempAnswer = text;
        _llmConnectorResponseChecker.SendPrompt(recieveResponseCheckerResponse, endPriceSearch, _tempAnswer, 1);
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta del TextChecker enfocado en analizar la respuesta del LLM. Devuelve un booleano si la respuesta 
    /// es coherente.
    /// </summary>
    /// <param name="isCoherent">Indica si la respuesta se adapta al contexto determinado del rol y tiene un formato correcto</param>
    private void recieveResponseCheckerResponse(bool isCoherent)
    {
        if (isCoherent)
        {
            endPriceSearch(_tempAnswer);
        }
        else
        {
            endPriceSearch("Lo siento. No he podido encontrar nada de tu petición. Formule de nuevo la pregunta o pruebe algo distinto.");
        }        
    }

    /// <summary>
    /// Metodo que togglea el menu de busqueda de precios
    /// </summary>
    public void OpenMenu()
    {
        _searchMenuView.SetActive(!_searchMenuView.activeSelf);
    }

    /// <summary>
    /// Metodo que desactiva las corutinas una vez se cambie de escena
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
