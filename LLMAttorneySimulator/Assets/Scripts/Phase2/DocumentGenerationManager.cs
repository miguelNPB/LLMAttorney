using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// Clase manager para gestionar la peticion al LLM para crear un documento
/// </summary>
public class DocumentGenerationManager : MonoBehaviour
{
    [SerializeField] private LLMConnectorDocuments _llmConnectorDocContent;
    [SerializeField] private LLMConnectorDocumentsBudget _llmConnectorDocBudget;
    [SerializeField] private LLMConnectorTextChecker _llmConnectorTextChecker;

    [SerializeField] private string _prompt;

    // docTitle, docContent, docType, cost, isPlayer, isValid
    private Action<string, string, DocumentType, int, bool, bool> _responseCallback;
    private Action<string> _errorCallback;

    private DocumentType _currentDocType;
    private string _docTitle;
    private string _docContent;
    private int _cost;
    private bool _isPlayer;
    private bool _isValid;

    /// <summary>
    /// Metodo publico para empezar la generacion de un documento
    /// </summary>
    /// <param name="docType"></param>
    /// <param name="isPlayer"></param>
    /// <param name="isValid"></param>
    public void PromptGenerateDocument(Action<string, string, DocumentType, int, bool, bool> responseCallback, Action<string> errorCallback, DocumentType docType, bool isPlayer, bool isValid = true)
    {
        _responseCallback = responseCallback;
        _errorCallback = errorCallback;

        _currentDocType = docType;
        _docTitle = "";
        _docContent = "";
        _cost = 0;
        _isValid = isValid;
        _isPlayer = isPlayer;

        _llmConnectorDocContent.SendPrompt(recieveDocumentContent, errorCallback, _prompt, docType, isPlayer, isValid);
    }

    /// <summary>
    /// Llamado al recibir el contenido del documento
    /// </summary>
    /// <param name="docTitle"></param>
    /// <param name="docContent"></param>
    private void recieveDocumentContent(string docTitle, string docContent)
    {
        _docTitle = docTitle;
        _docContent = docContent;

        if (_isPlayer)
        {
            // si el documento es perito o reporte es necesario calcular su coste
            if (_currentDocType == DocumentType.Perito || _currentDocType == DocumentType.Report)
            {
                _llmConnectorDocBudget.SendPrompt(recieveDocumentBudget, _errorCallback, _docContent);
            } 
            else // si no, ir directamente a comprobar si es valido
            {
                _cost = 0;
                _llmConnectorTextChecker.SendPrompt(recieveTextCheckerAnswer, _errorCallback, _docContent, (int)_currentDocType);
            }
        }
        else
        {
            _cost = 0;
            // si el documento del rival queremos que sea valido, comprobarlo
            if (_isValid)
            {
                _llmConnectorTextChecker.SendPrompt(recieveTextCheckerAnswer, _errorCallback, _docContent, (int)_currentDocType);
            }
            else // si no, ya esta, se manda el final
            {
                sendFinalDocument();
            }
        }
    }

    /// <summary>
    /// Llamado al recibir el coste de un documento
    /// </summary>
    /// <param name="cost"></param>
    private void recieveDocumentBudget(int cost)
    {
        _cost = cost;
        _llmConnectorTextChecker.SendPrompt(recieveTextCheckerAnswer, _errorCallback, _docContent, (int)_currentDocType);
    }

    /// <summary>
    /// Se llama al recibir la respuesta de si el documento generado es coherente
    /// </summary>
    /// <param name="isCoherent"></param>
    private void recieveTextCheckerAnswer(bool isCoherent)
    {
        _isValid = isCoherent;
        sendFinalDocument();
    }


    /// <summary>
    /// Metodo final que devuelve los contenidos del documento generado
    /// </summary>
    private void sendFinalDocument()
    {
        Telemetry.TelemetryDispatch.SendAskedDocument(_cost, (int)_currentDocType);

        _responseCallback?.Invoke(_docTitle, _docContent, _currentDocType, _cost, _isPlayer, _isValid);
    }
}
