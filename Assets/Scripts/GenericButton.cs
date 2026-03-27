using System.Collections;
using UnityEngine;

public class GenericButtonCanvas : MonoBehaviour
{
    enum BehaviourOnClick
    {
        OpenWindow,
        CloseWindow,
        ToggleWindow
    }

    enum ShowHideBehaviour
    {
        Move,
        Scale,
        EnableDisable
    }

    [SerializeField] private GameObject window;
    [SerializeField] private BehaviourOnClick behaviourOnClick;
    [SerializeField] private ShowHideBehaviour showHideBehaviour;

    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 endPosition;
    [SerializeField] private Vector3 startScale;
    [SerializeField] private Vector3 endScale;

    [Header("Animation Settings")]
    [SerializeField] private float animationSpeed = 10f; // Adjust this in the Inspector

    private bool isWindowOpen = false; // Tracks the current state reliably
    private Coroutine currentAnimation; // Stores the active coroutine so we can interrupt it

    void Start()
    {
        // Optional: Initialize state based on the window's starting active state
        if (window != null)
        {
            isWindowOpen = window.activeSelf;
        }
    }

    // Call this method from your Button's OnClick event in the Inspector
    public void OnClick()
    {
        if (window == null) return;

        // 1. Determine the target state based on the button's behaviour
        switch (behaviourOnClick)
        {
            case BehaviourOnClick.OpenWindow:
                isWindowOpen = true;
                break;
            case BehaviourOnClick.CloseWindow:
                isWindowOpen = false;
                break;
            case BehaviourOnClick.ToggleWindow:
                // Depends on the current state, so we just flip it
                isWindowOpen = !isWindowOpen;
                break;
        }

        // 2. Execute the animation or instant change
        switch (showHideBehaviour)
        {
            case ShowHideBehaviour.Move:
                Vector3 targetPosition = isWindowOpen ? endPosition : startPosition;
                StartSmoothAnimation(AnimateMove(targetPosition));
                break;

            case ShowHideBehaviour.Scale:
                Vector3 targetScale = isWindowOpen ? endScale : startScale;
                StartSmoothAnimation(AnimateScale(targetScale));
                break;

            case ShowHideBehaviour.EnableDisable:
                // Check if we need to enable or disable the window based on the target state
                isWindowOpen = window.activeSelf ? !isWindowOpen : isWindowOpen; // Ensure state matches the actual active state

                window.SetActive(isWindowOpen);

                break;
        }
    }

    /// <summary>
    /// Stops any ongoing animation and starts the new one to prevent jittering.
    /// </summary>
    private void StartSmoothAnimation(IEnumerator animationRoutine)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(animationRoutine);
    }

    private IEnumerator AnimateMove(Vector3 targetPos)
    {
        // Continue lerping until the distance is practically zero
        while (Vector3.Distance(window.transform.position, targetPos) > 0.001f)
        {
            window.transform.position = Vector3.Lerp(window.transform.position, targetPos, Time.deltaTime * animationSpeed);
            yield return null; // Wait until next frame
        }
        
        // Snap to exact position at the end
        window.transform.position = targetPos; 
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        // Continue lerping until the distance is practically zero
        while (Vector3.Distance(window.transform.localScale, targetScale) > 0.001f)
        {
            window.transform.localScale = Vector3.Lerp(window.transform.localScale, targetScale, Time.deltaTime * animationSpeed);
            yield return null; // Wait until next frame
        }
        
        // Snap to exact scale at the end
        window.transform.localScale = targetScale;
    }
}