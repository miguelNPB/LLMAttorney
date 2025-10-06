using UnityEngine;
using UnityEditor;
using System.Diagnostics;
public class LLMAttorney_Server : EditorWindow
{
    string batFilePath = @"C:\Ruta\TuArchivo.bat"; // Cambia la ruta aquí

    // Crear menú para abrir la ventana
    [MenuItem("Herramientas/Bat Runner")]
    public static void ShowWindow()
    {
        GetWindow<LLMAttorney_Server>("Bat Runner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Ejecutar archivo .bat", EditorStyles.boldLabel);

        // Campo para cambiar la ruta del .bat
        batFilePath = EditorGUILayout.TextField("Ruta del .bat", batFilePath);

        GUILayout.Space(10);

        // Botón que ejecuta el .bat
        if (GUILayout.Button("Ejecutar .bat"))
        {
            EjecutarBat();
        }
    }

    private void EjecutarBat()
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = batFilePath;
            startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(batFilePath);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = true;

            Process process = Process.Start(startInfo);
            process.WaitForExit(); // Opcional: espera a que termine

            UnityEngine.Debug.Log(".bat ejecutado correctamente.");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Error al ejecutar el .bat: " + e.Message);
        }
    }
}
