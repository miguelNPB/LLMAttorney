using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema para recibir eventos de input
/// </summary>
public class InputSystem : MonoBehaviour
{
    public static InputSystem Instance { get { return _instance; } }
    private static InputSystem _instance = null;

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
        {
            onSkipTextPerformed?.Invoke();
        }
    }

    /// <summary>
    /// Al scrollear con el raton
    /// </summary>
    /// <param name="context"></param>
    public void ScrollPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onScrollPerformed?.Invoke(context.ReadValue<float>());
        }
    }

    /// <summary>
    /// Al pulsar el tab para abrir el menu de cheats
    /// </summary>
    /// <param name="context"></param>
    public void CheatMenuPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cheatMenuPerformed?.Invoke();
        }
    }

    private void Init()
    {
        _initialized = true;
    }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        if (!_initialized)
        {
            _instance = this;
            Init();
        }
    }

}

