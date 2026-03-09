using System.Collections;
using UnityEngine;

public class Boot : MonoBehaviour
{

    /// <summary>
    /// Esperar a que el servidor devuelva un mensaje confirmando que funciona
    /// </summary>
    private IEnumerator CheckServerActive()
    {

        // TODO
        yield return null;
    }

    /// <summary>
    /// Metodo principal que carga las cosas necesarias y comprobaciones antes de entrar al menu principal
    /// </summary>
    /// <returns></returns>
    private IEnumerator BootProcess()
    {
        yield return CheckServerActive();
       
        SceneSystem.Instance.LoadMainMenu();
    }

    void Start()
    {
        StartCoroutine(BootProcess());
    }
}