using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton DontDestroyOnLoad que se llama para cambiar de escenas
/// </summary>
public class SceneSystem : MonoBehaviour
{
    [SerializeField] private SceneAsset MainMenu;

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MainMenu.name);
    }

    private void Awake()
    {
        instance = this;
    }

    public static SceneSystem Instance { get { return instance; } }
    private static SceneSystem instance = null;
}