using System.Collections;
using UnityEngine;

/// <summary>
/// Clase boton generico para algunos botones que abran ventanas que tengan animacion de apertura
/// </summary>
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
    [SerializeField] private float animationSpeed = 10f;

    private bool isWindowOpen = false;
    private Coroutine currentAnimation;

    void Start()
    {
        if (window != null)
        {
            isWindowOpen = window.activeSelf;
        }
    }

    /// <summary>
    /// Llamado al clicar el boton
    /// </summary>
    public void OnClick()
    {
        if (window == null) return;

        switch (behaviourOnClick)
        {
            case BehaviourOnClick.OpenWindow:
                isWindowOpen = true;
                break;
            case BehaviourOnClick.CloseWindow:
                isWindowOpen = false;
                break;
            case BehaviourOnClick.ToggleWindow:
                isWindowOpen = !isWindowOpen;
                break;
        }

        switch (showHideBehaviour)
        {
            case ShowHideBehaviour.Move:
                Vector3 targetPosition = isWindowOpen ? endPosition : startPosition;
                startSmoothAnimation(animateMove(targetPosition));
                break;

            case ShowHideBehaviour.Scale:
                Vector3 targetScale = isWindowOpen ? endScale : startScale;
                startSmoothAnimation(animateScale(targetScale));
                break;

            case ShowHideBehaviour.EnableDisable:
                isWindowOpen = window.activeSelf ? !isWindowOpen : isWindowOpen;

                window.SetActive(isWindowOpen);

                break;
        }
    }

    /// <summary>
    /// Detiene la animacion en curso y activa una nueva
    /// </summary>
    private void startSmoothAnimation(IEnumerator animationRoutine)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(animationRoutine);
    }

    /// <summary>
    /// Coroutina para  animar el movimiento
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    private IEnumerator animateMove(Vector3 targetPos)
    {
        while (Vector3.Distance(window.transform.position, targetPos) > 0.001f)
        {
            window.transform.position = Vector3.Lerp(window.transform.position, targetPos, Time.deltaTime * animationSpeed);
            yield return null;
        }
        
        window.transform.position = targetPos; 
    }
    
    /// <summary>
    /// Coroutina para animar el escalado
    /// </summary>
    /// <param name="targetScale"></param>
    /// <returns></returns>
    private IEnumerator animateScale(Vector3 targetScale)
    {
        while (Vector3.Distance(window.transform.localScale, targetScale) > 0.001f)
        {
            window.transform.localScale = Vector3.Lerp(window.transform.localScale, targetScale, Time.deltaTime * animationSpeed);
            yield return null; 
        }
        
        window.transform.localScale = targetScale;
    }
}