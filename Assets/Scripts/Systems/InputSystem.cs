using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema para recibir eventos de input
/// </summary>
public class InputSystem : MonoBehaviour
{
    public Action onSkipTextPerformed;
    public Action<float> onScrollPerformed;
    public Action cheatMenuPerformed;

    private bool _initialized = false;

    /// <summary>
    /// Al pulsar el boton para saltar texto
    /// </summary>
    /// <param name="context"></param>
    public void SkipTextPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
            onSkipTextPerformed?.Invoke();
    }

    /// <summary>
    /// Al scrollear con el raton
    /// </summary>
    /// <param name="context"></param>
    public void ScrollPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
            onScrollPerformed?.Invoke(context.ReadValue<float>());
    }

    public void CheatMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CheatsSystem.Instance.cheatMenu.gameObject.SetActive(!CheatsSystem.Instance.cheatMenu.activeSelf);
            if (SceneManager.GetActiveScene().name == "Phase2") CheatsSystem.Instance.cheatMenu.gameObject.SetActive(true);
            cheatMenuPerformed?.Invoke();
        }
    }

    private void Init()
    {
        _initialized = true;
    }
    private void Awake()
    {
        if (InputSystem.Instance != null && Instance != this)
            Destroy(this);

        if (!_initialized)
        {
            instance = this;
            Init();
        }
    }

    public static InputSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<InputSystem>();
                if (instance != null)
                {
                    instance.Init();
                }
            }
            return instance;
        }
    }
    private static InputSystem instance = null;
}

