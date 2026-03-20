using System;
using UnityEngine;
using UnityEngine.InputSystem;

    public class InputSystem : MonoBehaviour
    {
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

        private void Awake()
        {
            if (Instance != null)
                Destroy(this);

            instance = this;
        }

        public static InputSystem Instance { get { return instance; } }
        private static InputSystem instance = null;
    }

