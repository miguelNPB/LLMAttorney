using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase para gestionar la creacion de casos para el simulador
/// </summary>
public class CaseGenerationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LLMConnectorCaseDataGenerator _llmConnectorCaseDataGenerator;
    [SerializeField] private LLMCaseContentGenerator _llmConnectorContentGenerator;
    [SerializeField] private LLMConnectorClientIntroductionGenerator _llmConnectorClientIntroductionGenerator;
    [SerializeField] private PersistCaseData _persistor;

    [Header("UI")]
    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private Button _initialGenerateCaseButton;
    [SerializeField] private Button _retryGeneratingCaseButton;
    [SerializeField] private Button _continueGeneratingCaseButton;
    [SerializeField] private Button _closeButton;

    private bool _generating = false;
    private string _pendingMessage;

    private string _tmpClientName;
    private string _tmpRivalName;
    private string _tmpCaseSummary;
    private string _tmpCaseContent;
    private string _tmpClientIntroduction;

    /// <summary>
    /// Comienza la generacion de un caso. Llamado al pulsar el boton inicial de generar caso o el boton de regenerar caso
    /// </summary>
    public void GenerateCase()
    {
        if (_generating)
        {
            Debug.LogError("Error, continuando a generar cuando ya se esta generando, esto no deberia poder pasar");
            return;
        }

        _tmpClientName = "";
        _tmpRivalName = "";
        _tmpCaseSummary = "";
        _tmpCaseContent = "";
        _tmpClientIntroduction = "";

        if (_initialGenerateCaseButton.gameObject.activeSelf)
        {
            _initialGenerateCaseButton.gameObject.SetActive(false);
            _retryGeneratingCaseButton.gameObject.SetActive(true);
            _continueGeneratingCaseButton.gameObject.SetActive(true);
        }

        _retryGeneratingCaseButton.interactable = false;
        _continueGeneratingCaseButton.interactable = false;

        _closeButton.gameObject.SetActive(false);

        _generating = true;
        StartCoroutine(coroutinePendingMessage());

        _llmConnectorContentGenerator.SendPrompt(recieveCaseContent, recieveError);
    }

    /// <summary>
    /// Llamado al pulsar el boton de guardar caso generado
    /// </summary>
    public void SaveGeneratedCase()
    {
        if (_generating)
        {
            Debug.LogError("Error, intentado guardar cuando se esta generando aún, esto no deberia poder pasar");
            return;
        }

        _closeButton.gameObject.SetActive(true);
        _continueGeneratingCaseButton.interactable = false;
        _retryGeneratingCaseButton.interactable = true;

        _displayText.text = _persistor.PersistCase(_tmpClientName, _tmpRivalName, _tmpCaseSummary, _tmpCaseContent, _tmpClientIntroduction);
    }

    /// <summary>
    /// Llamado al recibir el contenido del caso
    /// </summary>
    /// <param name="content"></param>
    private void recieveCaseContent(string content)
    {
        _tmpCaseContent = content;
        _llmConnectorCaseDataGenerator.SendPrompt(recieveCaseData, recieveError, content);
    }

    /// <summary>
    /// Llamado al recibir la informacion del caso, ahora mostrarla y preguntar al jugador si quiere continuar o generar otro
    /// </summary>
    private void recieveCaseData(string clientName, string rivalName, string caseSummary)
    {
        _tmpClientName = clientName;
        _tmpRivalName = rivalName;
        _tmpCaseSummary = caseSummary;

        _pendingMessage = "Nombre del cliente: " + clientName + "   Nombre del demandado: " + rivalName + "\n\n" + "Resumen del caso: " + caseSummary;

        _llmConnectorClientIntroductionGenerator.SendPrompt(recieveCaseClientIntroduction, recieveError, _tmpCaseContent);
    }



    /// <summary>
    /// Llamado al recibir la introduccion que te cuenta el cliente en la fase 1, sobre el caso
    /// </summary>
    /// <param name="clientIntroduction"></param>
    private void recieveCaseClientIntroduction(string clientIntroduction)
    {
        _tmpClientIntroduction = clientIntroduction;

        finishGeneratingCase();
    }

    /// <summary>
    /// Llamado al terminar de generar un caso y tener todo listo
    /// </summary>
    private void finishGeneratingCase()
    {
        _closeButton.gameObject.SetActive(true);
        _retryGeneratingCaseButton.interactable = true;
        _continueGeneratingCaseButton.interactable = true;

        _generating = false;
    }

    /// <summary>
    /// Llamado al recibir un error del servidor
    /// </summary>
    /// <param name="error"></param>
    private void recieveError(string error)
    {
        _closeButton.gameObject.SetActive(true);

        _retryGeneratingCaseButton.interactable = true;
        _continueGeneratingCaseButton.interactable = false;

        _pendingMessage = "Error conectando al servidor: " + error;
        _generating = false;
    }

    /// <summary>
    /// Coroutina para animar la espera de generacion de caso
    /// </summary>
    /// <returns></returns>
    private IEnumerator coroutinePendingMessage()
    {
        float timer = 0;

        _displayText.text = "";
        while (_generating)
        {
            timer += Time.deltaTime;
            _displayText.text = "Generando caso";
            for (int i = 0; i < 3; i++)
                _displayText.text += (i <= (int)(timer % 3)) ? "." : " ";
            yield return null;
        }

        _displayText.text = _pendingMessage;
    }

    private void Start()
    {
        _initialGenerateCaseButton.gameObject.SetActive(true);
        _retryGeneratingCaseButton.gameObject.SetActive(false);
        _continueGeneratingCaseButton.gameObject.SetActive(false);
    }
}