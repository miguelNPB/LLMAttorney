using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton DontDestroyOnLoad que se llama para cambiar de escenas
/// </summary>
public class SceneSystem : MonoBehaviour
{
    [SerializeField] private SceneAsset MainMenu;

    [SerializeField] private SceneAsset Phase2;

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MainMenu.name);
    }

    public void LoadPhase2()
    {
        SceneManager.LoadScene(Phase2.name);
    }

    private void Awake()
    {
        if (SceneSystem.Instance != null)
            Destroy(this);

        instance = this;
    }

    public static SceneSystem Instance { get { return instance; } }
    private static SceneSystem instance = null;
}