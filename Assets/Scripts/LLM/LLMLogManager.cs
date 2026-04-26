using System.IO;
using Telemetry;
using UnityEngine;

public class LLMLogManager : MonoBehaviour
{

    private LLMLogManager _instance = null;

    public LLMLogManager Instance
    {
        get { return _instance; }
    }

    private string _path; 

    private int _messageSent = 0;
    private int _messageReceived = 0;


    public void LogMessageSent(string message)
    {
        string log = $"{System.DateTime.Now} [{_messageSent}] {message}";

        Debug.Log(log);
        _messageSent++;
    }

    public void LogMessageReceived(string message)
    {
        string log = $"{System.DateTime.Now} [{_messageReceived}] {message}";

        Debug.Log(log);
        _messageReceived++;
    }

    public int getNumMessageSent()
    {
        return _messageSent;
    }

    public int getNumMessageReceived()
    {
        return _messageReceived;
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
            Destroy(this);
        }
    }
}
