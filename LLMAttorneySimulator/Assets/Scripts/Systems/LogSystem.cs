using UnityEngine;

/// <summary>
/// Sistema para registrar los logs con tiempo y poder registrarlos en un fichero
/// </summary>
public class LogSystem : MonoBehaviour
{

    private static LogSystem _instance = null;

    public static LogSystem Instance
    {
        get { return _instance; }
    }

    private string _path; 

    /// <summary>
    /// Log a un string, punto de entrada principal para este sistema
    /// </summary>
    /// <param name="message"></param>
    public void LogString(string message)
    {
        string log = $"{System.DateTime.Now} {message}";

        Debug.Log(log);
    }

    /// <summary>
    /// Se registra a logMessageRecieved para loggear los log al fichero game.log
    /// </summary>
    /// <param name="logString"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    private void handleLog(string logString, string stackTrace, LogType type)
    {
        string msg = $"{System.DateTime.Now} [{type}] {logString}\n";
        System.IO.File.AppendAllText(_path, msg);
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;

            _path = Application.persistentDataPath + "/game.log";
            Application.logMessageReceived += handleLog;

        }
        else
        {
            Destroy(gameObject);
        }
    }
}
