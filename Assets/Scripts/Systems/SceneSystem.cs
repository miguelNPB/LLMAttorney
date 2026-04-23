using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton DontDestroyOnLoad que se llama para cambiar de escenas
/// </summary>
public class SceneSystem : MonoBehaviour
{
    [SerializeField] private SceneAsset MainMenu;
    [SerializeField] private SceneAsset Phase1;
    [SerializeField] private SceneAsset Phase2;
    [SerializeField] private SceneAsset Phase3;
    [SerializeField] private SceneAsset Phase4;

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MainMenu.name);
    }

    public void LoadPhase1()
    {
        SceneManager.LoadScene(Phase1.name);
    }

    public void LoadPhase2()
    {
        SceneManager.LoadScene(Phase2.name);
    }

    public void LoadPhase3()
    {
        SceneManager.LoadScene(Phase3.name);
    }
    public void LoadPhase4()
    {
        SceneManager.LoadScene(Phase4.name);
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