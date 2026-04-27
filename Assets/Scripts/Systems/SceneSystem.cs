using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton DontDestroyOnLoad que se llama para cambiar de escenas
/// </summary>
public class SceneSystem : MonoBehaviour
{

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadGameOver()
    {
        SceneManager.LoadScene(5);
    }

    public void LoadPhase1()
    {
        SceneManager.LoadScene(1);
    }

    public void LoadPhase2()
    {
        SceneManager.LoadScene(2);
    }

    public void LoadPhase3()
    {
        SceneManager.LoadScene(3);
    }
    public void LoadPhase4()
    {
        SceneManager.LoadScene(4);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void Awake()
    {
        if (SceneSystem.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public static SceneSystem Instance { get { return instance; } }
    private static SceneSystem instance = null;
}