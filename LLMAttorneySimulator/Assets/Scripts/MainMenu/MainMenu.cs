using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase para manejar la logica del menu principal. Comprueba que el caseData es valido, y que el servidor esta operativo y que el caseData.json 
/// y el caseContent.pdf tienen mismo id (son el mismo caso) cada X tiempo
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button _playGameButton;
    [SerializeField] private TMP_InputField _serverIPInputField;
    [SerializeField] private TMP_Text _serverStatusText;
    [SerializeField] private TMP_Text _caseDisplayText;
    [SerializeField] private float _timeBetweenChecks = 3;

    float _timer = 0;
    bool _casesIDMatch = false;
    public void ChangeServerIP()
    {
        LLMSystemAPI.Instance.ChangeServerIP(_serverIPInputField.text);
        
        // para que compruebe
        _casesIDMatch = false;

        checkReadyToPlay();
    }

    /// <summary>
    /// Llamar para empezar a jugar
    /// </summary>
    public void StartPlaying()
    {
        GameSystem.Instance.ResetCaseData();
        BudgetSystem.Instance.ResetBudget();

        SceneSystem.Instance.LoadPhase1();
    }

    /// <summary>
    /// Comprueba si esta todo listo para jugar el simulador y no hay errores
    /// </summary>
    private void checkReadyToPlay()
    {
        LLMSystemAPI.Instance.GetServerCaseID(recieveServerCaseID, onError);

        bool ready = GameSystem.Instance.CaseData != null && _casesIDMatch;

        _playGameButton.interactable = ready;
        _serverStatusText.gameObject.SetActive(!ready);
    }

    /// <summary>
    /// Llamado al obtener el id del caso del servidor
    /// </summary>
    /// <param name="id"></param>
    private void recieveServerCaseID(int id)
    {
        if (GameSystem.Instance.CaseData != null)
        {
            _casesIDMatch = id == GameSystem.Instance.CaseData.id;

            if (!_casesIDMatch)
            {
                _serverStatusText.text = $"IDs de caso no coinciden con el cliente y el servidor, arreglarlo o mandar un nuevo caso.\nID server: {id}, ID cliente: {(GameSystem.Instance.CaseData.id == 0 ? "default" : GameSystem.Instance.CaseData.id)}";
            }
            else
            {
                _serverStatusText.text = "Comprobando conexión con el servidor...";
            }
        }
    }

    /// <summary>
    /// Llamado si hay un error comunicando con el servidor o sacando el id
    /// </summary>
    /// <param name="error"></param>
    private void onError(string error)
    {
        _serverStatusText.text = "Error comunicando con el servidor: " + error;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer > _timeBetweenChecks)
        {
            _timer = 0;
            checkReadyToPlay();
        }

    }
    void Start()
    {
        GameSystem.Instance.ResetNotepad();
        GameSystem.Instance.ToggleNotepad(false);

        _caseDisplayText.text = GameSystem.Instance.CaseData != null ? "caseData_default" : "Ninguno";
    }
}
