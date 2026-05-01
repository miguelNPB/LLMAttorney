using System.IO;
using Telemetry;
using UnityEngine;

public class LLMLogManager : MonoBehaviour
{

    private static LLMLogManager _instance = null;

    public static LLMLogManager Instance
    {
        get { return _instance; }
    }

    private string _path; 

    private int _messageSent = 0;

    public void LogMessageSent(string message, int id)
    {
        string log = $"{System.DateTime.Now} [{id}] {message}";

        Debug.Log(log);
    }

    public int getMessageID()
    {
        return System.DateTime.Now.Millisecond;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
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
            Application.logMessageReceived += HandleLog;

        }
        else
        {
            Destroy(gameObject);
        }
    }
}
