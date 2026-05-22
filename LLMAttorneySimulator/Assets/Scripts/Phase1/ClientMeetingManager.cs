using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase enfocada en el control y relacion entre la UI de la reunion con el cliente y los conectores implicados en el rol y el proceso de generación de
/// la respuesta.
/// </summary>
public class ClientMeetingManager : MonoBehaviour
{
    #region UI
    [SerializeField] private Button _sendMessageButton;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private GameObject _changePhaseButton;
    [SerializeField] private TMP_InputField _inputField;

    [SerializeField] private TMP_Text resultText;

    #endregion

    #region Conectores
    [SerializeField] private LLMConnectorTextChecker _llmConnectorTextChecker;
    [SerializeField] private LLMConnectorBudgetChecker _llmConnectorBudgetChecker;
    [SerializeField] private LLMConnectorClientMeeting _llmConnectorClientMeeting;
    #endregion

    private string _pendingMessage = "";
    private bool _waitingPendingMessage = false;

    private string _prompt;
    private string _tempAnswer;
    private float _tempBudget;

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
    /// Metodo de llamada inicial para comenzar el proceso de generación de respuesta por parte del cliente. Se encarga de desactivar la UI necesaria
    /// hasta la obtención de la respuesta y de iniciar el proceso al enviar la primera peticion del conjunto de conectores al servidor.
    /// </summary>
    public void TalkToClient()
    {
        if (_waitingPendingMessage)
        {
            return;
        }
            
        _sendMessageButton.interactable = false;
        _waitingPendingMessage = true;
        _continueButton.SetActive(false);
        StartCoroutine(CoroutinePendingMessage());

        _prompt = _inputField.text;
        _llmConnectorTextChecker.SendPrompt(recieveQuestionCheckerResponse, endClientMeeting, _prompt, 0);
    }

    /// <summary>
    /// Metodo que termina el procesamiento del mensaje del jugador al cliente y reactiva la UI para el siguiente envio
    /// </summary>
    /// <param name="text">Texto que se escribe en el cuadro del cliente al terminar el proceso</param>
    private void endClientMeeting(string text)
    {
        _continueButton.SetActive(true);
        _sendMessageButton.interactable = true;
        _waitingPendingMessage = false;
        _pendingMessage = text;
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta del TextChecker enfocado en analizar el mensaje del usuario al cliente. Devuelve un booleano 
    /// si la pregunta es coherente.
    /// </summary>
    /// <param name="isCoherent">Indica si el mensaje enviado por el usuario es coherente respecto al contexto relatado por el cliente</param>
    private void recieveQuestionCheckerResponse(bool isCoherent)
    {
        if (isCoherent)
        {
            _llmConnectorBudgetChecker.SendPrompt(recieveBudgetCheckerResponse, endClientMeeting, _inputField.text);
        }
        else
        {
            endClientMeeting("Perdona pero ¿Podriamos centranos en mi caso?");
        }          
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta del BudgetChecker. Dada la respuesta revisa si el precio es coherente y almacena una estimación 
    /// del dinero pedido por el usuario
    /// </summary>
    /// <param name="budgetCoherent">Indica si el presupuesto pasado por el jugador es coherente respecto al producto o servicio descrito</param>
    /// <param name="budget">Estimación media de los costes descritos por el usuario</param>
    private void recieveBudgetCheckerResponse(bool budgetCoherent, int budget)
    {
        if (budgetCoherent)
        {
            _tempBudget = budget;
            _llmConnectorClientMeeting.SendPrompt(reciveClientMeetingResponse, endClientMeeting, _inputField.text);
        }
        else
        {
            endClientMeeting("No estoy de acuerdo con el precio que me estas diciendo. ¿Estas seguro que no hay otra alternativa?");
        }
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta de el conector ClientMeeting. Este almacena el texto contestado por el cliente.
    /// </summary>
    /// <param name="answer">Respuesta dada por el cliente al prompt enviado</param>
    /// <param name="isValid">Indicador de si la respuesta a sido considerada valida. No se usa ya que su objetivo es permitir saltarse steps, no
    /// validar el resultado final, ya que eso es trabajo del siguiente conector</param>
    private void reciveClientMeetingResponse(string answer, bool isValid)
    {
        _tempAnswer = answer;
        _llmConnectorTextChecker.SendPrompt(recieveResponseCheckerResponse, endClientMeeting, _tempAnswer, 1);
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta del TextChecker encargado de revisar la respuesta. Devuelve un booleano si la respuesta es coherente.
    /// </summary>
    /// <param name="isCoherent">Indica si la respuesta se adapta al contexto y a la pregunta hecha</param>
    private void recieveResponseCheckerResponse(bool isCoherent)
    {
        if (isCoherent)
        {
            if (_tempBudget > 0)
            {
                _changePhaseButton.SetActive(true);
            }

            BudgetSystem.Instance.SetBudgetFromPhase1(_prompt, _tempBudget);

            endClientMeeting(_tempAnswer);
        }         
        else
        {
            endClientMeeting("No te he podido entender bien, podrias repetirmelo por favor");
        }
            
    }

    /// <summary>
    /// Metodo que desactiva las corutinas una vez se cambie de escena
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
