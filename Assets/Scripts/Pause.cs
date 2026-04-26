using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject PausePanel;

    [SerializeField]
    private Button ContinueButton;
    [SerializeField]
    private Button MainMenuButton;
    [SerializeField]
    private KeyCode ToggleKey = KeyCode.Escape;

    private void Awake()
    {
        if (ContinueButton != null)
            ContinueButton.onClick.AddListener(Continue);

        if (MainMenuButton != null)
            MainMenuButton.onClick.AddListener(GoToMainMenu);

        // Empezar despausado
        SetPauseActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(ToggleKey))
            SetPauseActive(!IsPauseActive());
    }

    public void SetPauseActive(bool active)
    {
        if (PausePanel != null)
            PausePanel.SetActive(active);
    }

    public bool IsPauseActive()
    {
        return PausePanel != null && PausePanel.activeSelf;
    }

    public void Continue()
    {
        SetPauseActive(false);
    }

    public void GoToMainMenu()
    {
        //Time.timeScale = 1f;

        //Destruir el GameSystem para resetear los datos del caso al volver al menu principal
        var gs = FindAnyObjectByType<GameSystem>();
        if (gs != null) Destroy(gs.gameObject);
        
        SceneSystem.Instance.LoadMainMenu();

        
    }
}