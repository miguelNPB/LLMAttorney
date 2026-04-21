using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sistema para recibir eventos de input
/// </summary>
public class InputSystem : MonoBehaviour
{
    public Action onSkipTextPerformed;
    public Action<float> onScrollPerformed;

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

