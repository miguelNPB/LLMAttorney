using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PromptType { Pregunta, Dialogo, Perito, Informe, Testigo, DocAlt }

public class DocumentManager : MonoBehaviour
{
    [SerializeField] Button[] documentButtons;
    [SerializeField] GameObject documentTab;
    [SerializeField] GameObject documentContainer;
    [SerializeField] GameObject documentPrefab;
    [SerializeField] Vector2 startingPos;
    [SerializeField] Vector2 openPos;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] GameObject navBar;
    [SerializeField] Vector2 dragOffset;
    [SerializeField] GameObject winTitle;

    [Header("Snapping")]
    [SerializeField] float snapDistance = 80f;
    [SerializeField] float snapLerpSpeed = 10f;
    [SerializeField] Vector2 defaultTabSize;   // Set in inspector to documentTab's default sizeDelta

    private bool isAtEndingPos = false;
    private bool isMoving = false;
    private Vector2 startPos;
    private Vector2 targetPos;
    private float lerpProgress = 0f;

    private Vector2 docPos = new Vector2(-3, -30);
    public List<Document> documents;

    // Drag state
    private RectTransform draggedWindow = null;
    private Vector2 pointerDragOffset;
    private Canvas rootCanvas;
    private RectTransform canvasRect;
    private RectTransform documentTabRect;

    // Snap state
    private bool isSnapping = false;
    private Vector2 snapTarget;
    private bool isSnapped = false;

    private string allSentDocInfo = ""; //Para el procurador enemigo

    public string getAllSentDocsInfo()
    {
        return allSentDocInfo;
    }

    public void AddSentDocInfo(string docName, string docContent)
    {
        string docInfo = $"Documento enviado: {docName}\nContenido: {docContent}";
    
        allSentDocInfo += docInfo + "\n";
    }

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Confined;

        rootCanvas = documentTab.GetComponentInParent<Canvas>();
        while (rootCanvas != null && !rootCanvas.isRootCanvas)
            rootCanvas = rootCanvas.transform.parent?.GetComponentInParent<Canvas>();

        canvasRect = rootCanvas.GetComponent<RectTransform>();
        documentTabRect = documentTab.GetComponent<RectTransform>();

        foreach (Button b in documentButtons)
            b.onClick.AddListener(OnClickDocumentsIcon);

        documents = new List<Document>();
        //for (int i = 0; i < 60; i++)
        //    CreateDocument("DOC" + i + ".txt", PromptType.Perito, "ESTE ES EL DOC " + i, true, 10);

        SetupNavBarDrag();


        // RectTransform contentRT = documentContainer.GetComponent<RectTransform>();
        // contentRT.anchorMin = new Vector2(0.5f, 1f);
        // contentRT.anchorMax = new Vector2(0.5f, 1f);
        // contentRT.pivot = new Vector2(0.5f, 1f);
        // contentRT.anchoredPosition = Vector2.zero;

    }

    #region Movimiento
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) ReleaseWindow();
    }

    void SetupNavBarDrag()
    {
        if (navBar == null) return;

        EventTrigger trigger = navBar.GetComponent<EventTrigger>() ?? navBar.AddComponent<EventTrigger>();

        EventTrigger.Entry onDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        onDown.callback.AddListener((e) => OnNavBarDown((PointerEventData)e));
        trigger.triggers.Add(onDown);

        EventTrigger.Entry onDrag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        onDrag.callback.AddListener((e) => OnNavBarDrag((PointerEventData)e));
        trigger.triggers.Add(onDrag);

        EventTrigger.Entry onUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        onUp.callback.AddListener((e) => OnNavBarUp((PointerEventData)e));
        trigger.triggers.Add(onUp);
    }

    void OnNavBarDown(PointerEventData eventData)
    {
        draggedWindow = documentTabRect;
        draggedWindow.SetAsLastSibling();
        isSnapping = false;

        // Restore default size when grabbed from snapped state
        if (isSnapped)
        {
            documentTabRect.sizeDelta = defaultTabSize;
            isSnapped = false;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            GetCamera(),
            out Vector2 localPoint
        );

        pointerDragOffset = draggedWindow.anchoredPosition - localPoint + dragOffset;
    }

    void OnNavBarDrag(PointerEventData eventData)
    {
        if (draggedWindow == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            GetCamera(),
            out Vector2 localPoint
        );

        draggedWindow.anchoredPosition = localPoint + pointerDragOffset;
    }

    void OnNavBarUp(PointerEventData eventData)
    {
        if (draggedWindow == null) return;

        if (TryGetEdgeSnap(draggedWindow.anchoredPosition, out Vector2 snap))
        {
            snapTarget = snap;
            isSnapping = true;
        }
        else
        {
            draggedWindow = null;
        }
    }

    bool TryGetEdgeSnap(Vector2 windowPos, out Vector2 result)
    {
        Vector2 canvasHalf = canvasRect.rect.size * 0.5f;

        float snapHalfWidth = canvasRect.rect.size.x * 0.25f; // half of half-screen width
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

    void ResizeTabToEdge(Vector2 snappedPos)
    {
        Vector2 canvasSize = canvasRect.rect.size;

        // Resize documentTab to half screen width, full height
        documentTabRect.sizeDelta = new Vector2(canvasSize.x * 0.5f, canvasSize.y);
        documentTabRect.anchoredPosition = snappedPos;
        isSnapped = true;
    }

    void ReleaseWindow()
    {
        if (draggedWindow == null) return;

        if (TryGetEdgeSnap(draggedWindow.anchoredPosition, out Vector2 snap))
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
    #endregion

    void Update()
    {
        if (isMoving)
        {
            lerpProgress += moveSpeed * Time.deltaTime;
            documentTab.transform.localPosition = Vector3.Lerp(startPos, targetPos, lerpProgress);

            if (lerpProgress >= 1f)
            {
                documentTab.transform.localPosition = targetPos;
                isMoving = false;
                lerpProgress = 0f;
            }
        }

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
                ResizeTabToEdge(snapTarget);
                isSnapping = false;
                draggedWindow = null;
            }
        }
    }

    Camera GetCamera() =>
        rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

    #region Botones
    void OnClickDocumentsIcon()
    {
        startPos = documentTab.transform.localPosition;
        targetPos = isAtEndingPos ? startingPos : openPos;
        isAtEndingPos = !isAtEndingPos;
        lerpProgress = 0f;
        isMoving = true;
    }
    #endregion

    #region Documentos
    public void CreateDocument(string docName, PromptType docType, string content, bool valid, int cost, bool isOpponentDoc = false)
    {
        docPos.x++;
        if (docPos.x > 2)
        {
            docPos.x = -2;
            docPos.y -= 220;
        }

        GameObject aux = Instantiate(documentPrefab, documentContainer.transform);
        aux.GetComponent<Document>().SetDoc(docName, docType, content, valid, cost, isOpponentDoc);

        aux.transform.localScale = Vector3.one;

        // Position relative to top of content rect
        RectTransform rt = aux.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(docPos.x * 200f, docPos.y); // Y is negative, goes downward

        // Resize content to fit
        RectTransform contentRT = documentContainer.GetComponent<RectTransform>();
        float neededHeight = Mathf.Abs(docPos.y) + 220f;
        contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, neededHeight);

        Button docButton = aux.GetComponentInChildren<Button>();
        if (docButton != null)
        {
            string capturedName = docName;
            docButton.onClick.AddListener(() => SetWindowTitle(capturedName));
        }

        documents.Add(aux.GetComponent<Document>());

        if (BudgetManager.Instance != null)
            BudgetManager.Instance.AddExpense($"0 {cost}", docType, docName);
    }

    void SetWindowTitle(string title)
    {
        if (winTitle == null) return;

        var tmp = winTitle.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null) { tmp.text = title; return; }

        var ugui = winTitle.GetComponent<Text>();
        if (ugui != null) ugui.text = title;
    }
    #endregion
}