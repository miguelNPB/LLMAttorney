using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class LLMAttorney_Server : MonoBehaviour
{
    public static LLMAttorney_Server Instance { get; private set; }


    private Process backendProcess;
    private void OnEnable()
    {
        Init();
    }
    private void OnDisable()
    {
        Release();
    }

    private void OnDestroy()
    {
        Release();
    }
    private void Init()
    {
        string path = Path.Combine(Application.dataPath, @"..\PythonServer\");
        path = Path.GetFullPath(path);

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "uvicorn";
        psi.Arguments = "main:app --reload --host 0.0.0.0 --port 8000";
        psi.WorkingDirectory = path;
        psi.UseShellExecute = true;
        psi.WindowStyle = ProcessWindowStyle.Normal;

        try
        {
            backendProcess = Process.Start(psi);
            UnityEngine.Debug.Log("LLMAttorneyAPI backend inicado");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Error al ejecutar el backend: " + ex.Message);
        }
    }
    private void Release()
    {
        if (backendProcess != null && !backendProcess.HasExited)
        {
            foreach (var proc in Process.GetProcessesByName("python"))
            {
                proc.Kill();
            }
            backendProcess.Kill();
            backendProcess = null;
            UnityEngine.Debug.Log("LLMAttorneyAPI backend cerrado");
        }
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {

    }
}
