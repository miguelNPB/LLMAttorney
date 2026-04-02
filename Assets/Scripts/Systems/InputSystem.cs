using System;
using UnityEngine;
using UnityEngine.InputSystem;

    public class InputSystem : MonoBehaviour
    {
        private bool initialized = false;

        public Action OnSkipTextPerformed;
        public Action<float> OnScrollPerformed;

        public void SkipTextPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnSkipTextPerformed?.Invoke();
        }
        public void ScrollPerformed(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnScrollPerformed?.Invoke(context.ReadValue<float>());
        }


        private void Init()
        {
        
        }
        private void Awake()
        {
            if (InputSystem.Instance != null && Instance != this)
                Destroy(this);

            if (!initialized)
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

