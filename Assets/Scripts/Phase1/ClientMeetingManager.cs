using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientMeetingManager : MonoBehaviour
{

    [SerializeField] private Button _sendMessageButton;
    [SerializeField] private GameObject _clientMeetingMenuView;
    [SerializeField] private TMP_InputField _inputField;

    [SerializeField] private TMP_Text resultText;

    [SerializeField] private LLMConnectorTextChecker _llmConnectorQuestionChecker;
    [SerializeField] private LLMConectorBudgetChecker _llmConnectorBudgetChecker;
    [SerializeField] private LLMConectorClientMeeting _llmConnectorClientMeeting;
    [SerializeField] private LLMConnectorTextChecker _llmConnectorResponseChecker;
    [SerializeField] private LLMConnectorHireLawyerCheck _llmConnectorLawyerHireCheck; //Falta

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
    /// Llamar para hacer la busqueda de precios. Llamara a tres LLMConnector y finalmente pondrá el resultado en resultText
    /// </summary>
    public void TalkToClient()
    {
        if (_waitingPendingMessage)
            return;

        _sendMessageButton.interactable = false;
        _waitingPendingMessage = true;
        StartCoroutine(CoroutinePendingMessage());

        _prompt = _inputField.text;
        _llmConnectorQuestionChecker.SendPrompt(recieveQuestionCheckerResponse, _prompt, 0);
    }

    /// <summary>
    /// Termina la busqueda de precios y pone un texto final de respuesta.
    /// </summary>
    /// <param name="text"></param>
    private void endClientMeeting(string text)
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
        if (isCoherent)
        {
            _llmConnectorBudgetChecker.SendPrompt(recieveBudgetCheckerResponse, _inputField.text);
        }
        else
        {
            endClientMeeting("Perdona pero ¿Podriamos centranos en mi caso?");
        }          
    }

    private void recieveBudgetCheckerResponse(bool budgetCoherent, float budget)
    {
        if (budgetCoherent)
        {
            _tempBudget = budget;
            _llmConnectorClientMeeting.SendPrompt(reciveClientMeetingResponse, _inputField.text);
        }
        else
        {
            endClientMeeting("No estoy de acuerdo con el precio que me estas diciendo. ¿Estas seguro que no hay otra alternativa?");
        }
    }

    private void reciveClientMeetingResponse(string answer, bool isValid)
    {
        if (isValid)
        {
            _tempAnswer = answer;
            _llmConnectorResponseChecker.SendPrompt(recieveResponseCheckerResponse, _inputField.text, 1);
        }
        else
        {
            endClientMeeting("No te he podido entender bien, podrias repetirmelo por favor");
        }
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta del ultimo LLMConnector, el ResponseChecker. Devuelve un booleano si la respuesta es coherente.
    /// </summary>
    /// <param name="isCoherent"></param>
    private void recieveResponseCheckerResponse(bool isCoherent)
    {
        if (isCoherent)
        {
            _llmConnectorLawyerHireCheck.SendPrompt(recieveHireLawyerResponse, _inputField.text);
        }         
        else
        {
            endClientMeeting("No te he podido entender bien, podrias repetirmelo por favor");
        }
            
    }

    /// <summary>
    /// Metodo llamado al recibir la respuesta dle LLMConnector de buscador de precios.
    /// </summary>
    /// <param name="text"></param>
    private void recieveHireLawyerResponse(bool hireLawyer)
    {
        endClientMeeting(_tempAnswer);   
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
