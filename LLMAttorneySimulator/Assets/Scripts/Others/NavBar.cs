using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Clase para el drag de ventanas flotantes
/// </summary>
public class NavBarDrag : MonoBehaviour
{
    [SerializeField] private Vector2 dragOffset;
    [SerializeField] private Vector2 defaultTabSize;
    [SerializeField] private RectTransform targetWindow;
    [SerializeField] private RectTransform dragLimits;

    [SerializeField] private TMP_Text winTitle;

    [Header("Snapping")]
    [SerializeField] float snapDistance = 80f;
    [SerializeField] float snapLerpSpeed = 10f;

    private RectTransform draggedWindow;
    private Vector2 pointerDragOffset;
    private Canvas rootCanvas;
    private RectTransform canvasRect;
    private RectTransform documentTabRect;

    private bool isSnapping = false;
    private Vector2 snapTarget;
    private bool isSnapped = false;


    /// <summary>
    /// Inicializa el sistema de drag
    /// </summary>
    private void setupNavBarDrag()
    {
        EventTrigger trigger = gameObject.GetComponent<EventTrigger>() ?? gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry onDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        onDown.callback.AddListener((e) => onNavBarDown((PointerEventData)e));
        trigger.triggers.Add(onDown);

        EventTrigger.Entry onDrag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        onDrag.callback.AddListener((e) => onNavBarDrag((PointerEventData)e));
        trigger.triggers.Add(onDrag);

        EventTrigger.Entry onUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        onUp.callback.AddListener((e) => onNavBarUp((PointerEventData)e));
        trigger.triggers.Add(onUp);
    }

    /// <summary>
    /// Al pulsar una ventana
    /// </summary>
    /// <param name="eventData"></param>
    private void onNavBarDown(PointerEventData eventData)
    {
        draggedWindow = documentTabRect;
        draggedWindow.SetAsLastSibling();
        isSnapping = false;

        if (isSnapped)
        {
            documentTabRect.sizeDelta = defaultTabSize;
            isSnapped = false;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, getCamera(), out Vector2 localPoint);

        pointerDragOffset = draggedWindow.anchoredPosition - localPoint + dragOffset;
    }

    /// <summary>
    /// Clamp a los limites de la pantalla
    /// </summary>
    /// <param name="targetPos"></param>
    /// <param name="window"></param>
    /// <returns></returns>
    private Vector2 clampToBounds(Vector2 targetPos, RectTransform window)
    {
        // Temporarily move window to targetPos, sample navbar corners, then decide
        Vector2 previousPos = window.anchoredPosition;
        window.anchoredPosition = targetPos;
        Canvas.ForceUpdateCanvases();

        Vector3[] navCorners = new Vector3[4];
        GetComponent<RectTransform>().GetWorldCorners(navCorners);

        Vector3[] limitCorners = new Vector3[4];
        dragLimits.GetWorldCorners(limitCorners);

        Vector2 nudge = Vector2.zero;
        if (navCorners[0].x < limitCorners[0].x) nudge.x = limitCorners[0].x - navCorners[0].x;
        if (navCorners[2].x > limitCorners[2].x) nudge.x = limitCorners[2].x - navCorners[2].x;
        if (navCorners[0].y < limitCorners[0].y) nudge.y = limitCorners[0].y - navCorners[0].y;
        if (navCorners[2].y > limitCorners[2].y) nudge.y = limitCorners[2].y - navCorners[2].y;

        window.anchoredPosition = previousPos; // restore, OnNavBarDrag will set final
        return targetPos + (Vector2)canvasRect.InverseTransformVector(nudge);
    }

    /// <summary>
    /// al mover una ventana arrastrandola
    /// </summary>
    /// <param name="eventData"></param>
    void onNavBarDrag(PointerEventData eventData)
    {
        if (draggedWindow == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, getCamera(), out Vector2 localPoint);

        Vector2 targetPos = localPoint + pointerDragOffset;

        if (dragLimits != null)
            targetPos = clampToBounds(targetPos, draggedWindow);

        draggedWindow.anchoredPosition = targetPos;
    }

    /// <summary>
    /// Al soltar una ventana arrastrable
    /// </summary>
    /// <param name="eventData"></param>
    void onNavBarUp(PointerEventData eventData)
    {
        if (draggedWindow == null) return;

        if (tryGetEdgeSnap(draggedWindow.anchoredPosition, out Vector2 snap))
        {
            snapTarget = dragLimits != null ? clampToBounds(snap, draggedWindow) : snap;
            isSnapping = true;
        }
        else
        {
            draggedWindow = null;
        }
    }

    /// <summary>
    /// Intenta obtener un borde
    /// </summary>
    /// <param name="windowPos"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    bool tryGetEdgeSnap(Vector2 windowPos, out Vector2 result)
    {
        Vector2 canvasHalf = canvasRect.rect.size * 0.5f;
        float snapHalfWidth = canvasRect.rect.size.x * 0.25f;
        Vector2 snapLeft  = new Vector2(-canvasHalf.x + snapHalfWidth, 0);
        Vector2 snapRight = new Vector2( canvasHalf.x - snapHalfWidth, 0);

        float distLeft  = Mathf.Abs(windowPos.x - (-canvasHalf.x));
        float distRight = Mathf.Abs(windowPos.x -  canvasHalf.x);

        result = Vector2.zero;

        if (distLeft < snapDistance || distRight < snapDistance)
        {
            result = distLeft < distRight ? snapLeft : snapRight;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Llamado para reescalarlo para que se ajuste a un borde
    /// </summary>
    /// <param name="snappedPos"></param>
    void resizeTabToEdge(Vector2 snappedPos)
    {
        Vector2 canvasSize = canvasRect.rect.size;
        documentTabRect.sizeDelta = new Vector2(canvasSize.x * 0.5f, canvasSize.y);
        documentTabRect.anchoredPosition = snappedPos;
        isSnapped = true;
    }

    /// <summary>
    /// Llamado al soltar la ventana
    /// </summary>
    private void releaseWindow()
    {
        if (draggedWindow == null) return;

        if (tryGetEdgeSnap(draggedWindow.anchoredPosition, out Vector2 snap))
        {
            snapTarget = snap;
            isSnapping = true;
        }
        else
        {
            isSnapping = false;
            draggedWindow = null;
        }
    }

    void Update()
    {
        if (isSnapping && draggedWindow != null)
        {
            draggedWindow.anchoredPosition = Vector2.Lerp(
                draggedWindow.anchoredPosition,
                snapTarget,
                Time.deltaTime * snapLerpSpeed
            );

            if (Vector2.Distance(draggedWindow.anchoredPosition, snapTarget) < 0.5f)
            {
                draggedWindow.anchoredPosition = snapTarget;
                resizeTabToEdge(snapTarget);
                isSnapping = false;
                draggedWindow = null;
            }
        }
    }

    void Start()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        while (rootCanvas != null && !rootCanvas.isRootCanvas)
            rootCanvas = rootCanvas.transform.parent?.GetComponentInParent<Canvas>();

        canvasRect = rootCanvas.GetComponent<RectTransform>();
        documentTabRect = targetWindow != null ? targetWindow : transform.parent.GetComponent<RectTransform>();

        setupNavBarDrag();

        if (winTitle != null)
            //Search in children for a TMP_Text component to set the title
            GetComponentInChildren<TMP_Text>().text = winTitle.text;

        if (dragLimits == null)
            Debug.LogWarning("Drag limits not set. Searching for game object with tag 'NavLimits'...");
        dragLimits = GameObject.FindGameObjectWithTag("NavLimits")?.GetComponent<RectTransform>();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) releaseWindow();
    }
    private Camera getCamera() =>
        rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
} 