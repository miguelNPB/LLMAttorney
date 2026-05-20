using System;
using System.IO;
using UnityEngine;
using static PersistCaseData;

/// <summary>
/// Sistema para almacenar cosas persistentes y sobre el caso. Tiene el notepad y caseData
/// </summary>
public class GameSystem : MonoBehaviour
{
    [SerializeField] private Notepad _notepad;

    private static GameSystem instance = null;
    private bool initialized = false;
    public static GameSystem Instance
    {
        get {  return instance; }
    }
    private CaseData _caseData = null; 
    public CaseData CaseData { get { return _caseData; } }


    private CaseData _startingCaseData = null;

    /// <summary>
    /// Llamarlo al volver al menu prinicpal
    /// </summary>
    public void ResetCaseData()
    {
        _caseData = _startingCaseData;   
    }

    /// <summary>
    /// Lee el case data y lo inicializa. Lee el json con el contenido. LLamado al iniciar el juego para cargar el default, y en caso de pulsar el boton de cambiar caso por uno generado. Devuelve true si salio bien.
    /// </summary>
    public bool ReadCaseData(string path)
    {
        return readCaseData(path);
    }

    /// <summary>
    /// Lee y establece un case data con el path. Devuelve true si salio bien.
    /// </summary>
    /// <param name="path"></param>
    private bool readCaseData(string path)
    {
        try
        {
            string jsonText = File.ReadAllText(path);

            PersistCaseData.CaseDataSerializable caseData = JsonUtility.FromJson<CaseDataSerializable>(jsonText);

            _caseData = new CaseData(caseData.id, caseData.clientName, caseData.rivalName, caseData.caseSummary, caseData.caseClientIntroduction);
            _startingCaseData = _caseData;

            return true;
        }
        catch (Exception e)
        {
            _caseData = null;
            _startingCaseData = null;
            Debug.LogError($"Error leyendo case data {path} " + e.Message);
            return false;
        }
    }
    

    /// <summary>
    /// Metodo solo usable en pruebas para limpiar los documentos del cliente
    /// </summary>
    public void DEBUG_ClearPlayerDocs()
    {
        _caseData.documentManager.DEBUG_ClearPlayerDocs();
    }

    /// <summary>
    /// Activa o dessactiva el notepad
    /// </summary>
    /// <param name="on"></param>
    public void ToggleNotepad(bool on)
    {
        _notepad.ToggleNotepad(on);
    }

    /// <summary>
    /// Reinicia el notepad
    /// </summary>
    public void ResetNotepad()
    {
        _notepad.ResetText();
    }


    private void Init()
    {
        DontDestroyOnLoad(gameObject);

        // init default
        string path = Path.Combine(Application.streamingAssetsPath, "savedCaseData_default.json");
        readCaseData(path);

        initialized = true;
    }

    private void Awake()
    {
        if (GameSystem.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        if (!initialized)
        {
            instance = this;
            Init();
        }
    }
}
