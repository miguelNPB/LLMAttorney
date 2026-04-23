using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Pagina para gestionar el visualizado y registro de los documentos del caso, tanto del player como del rival
/// </summary>
public class DocumentsPage : IPage
{
    [Header("References")]
    [SerializeField] private GameObject _pageHolder;
    [SerializeField] private GameObject _playerDocumentsContainer;
    [SerializeField] private GameObject _rivalDocumentsContainer;
    [SerializeField] private GameObject _playerDocumentPrefab;

    [SerializeField] Button[] documentButtons;
    [SerializeField] GameObject documentTab;

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


    private DocumentManager _documentManager;
    private List<UIDocument> _playerDocumentsInstanciated = new List<UIDocument>();
    private List<UIDocument> _rivalDocumentsInstanciated = new List<UIDocument>();

    void Awake()
    {
        _documentManager = GameSystem.Instance.CaseData.documentManager;
        /*
        rootCanvas = documentTab.GetComponentInParent<Canvas>();
        while (rootCanvas != null && !rootCanvas.isRootCanvas)
            rootCanvas = rootCanvas.transform.parent?.GetComponentInParent<Canvas>();

        canvasRect = rootCanvas.GetComponent<RectTransform>();
        documentTabRect = documentTab.GetComponent<RectTransform>();

        foreach (Button b in documentButtons)
            b.onClick.AddListener(OnClickDocumentsIcon);
        SetupNavBarDrag();
        */
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
        Vector2 snapLeft = new Vector2(-canvasHalf.x + snapHalfWidth, 0);
        Vector2 snapRight = new Vector2(canvasHalf.x - snapHalfWidth, 0);

        float distLeft = Mathf.Abs(windowPos.x - (-canvasHalf.x));
        float distRight = Mathf.Abs(windowPos.x - canvasHalf.x);

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

    void OnClickDocumentsIcon()
    {
        startPos = documentTab.transform.localPosition;
        targetPos = isAtEndingPos ? startingPos : openPos;
        isAtEndingPos = !isAtEndingPos;
        lerpProgress = 0f;
        isMoving = true;
    }
    private void addUIDocument(Document doc)
    {
        docPos.x++;
        if (docPos.x > 2)
        {
            docPos.x = -2;
            docPos.y -= 220;
        }

        GameObject parent = doc.IsRivalDoc() ? _rivalDocumentsContainer : _playerDocumentsContainer;

        GameObject instanciatedUIDoc = Instantiate(_playerDocumentPrefab, parent.transform);
        instanciatedUIDoc.GetComponent<UIDocument>().SetDocValues(doc.GetDocName(), doc.GetContent(), doc.IsSentToProcurador());

        /*
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
            docButton.onClick.AddListener(() => setWindowTitle(capturedName));
        }
        */
    }

    private void updateUIDocument(UIDocument uiDoc, Document doc)
    {

    }

    private void setWindowTitle(string title)
    {
        if (winTitle == null) return;

        var tmp = winTitle.GetComponent<TMPro.TextMeshProUGUI>();
        if (tmp != null) { tmp.text = title; return; }

        var ugui = winTitle.GetComponent<Text>();
        if (ugui != null) ugui.text = title;
    }


    private void setupUIDocuments()
    {
        // setup la seccion del player
        List<uint> playerDocuments = _documentManager.GetPlayerDocs();
        for (int i = 0; i < playerDocuments.Count; i++)
        {
            if (i < _playerDocumentsInstanciated.Count) {
                updateUIDocument(_playerDocumentsInstanciated[i], _documentManager.GetDocument(playerDocuments[i]));
            }
            else{
                addUIDocument(_documentManager.GetDocument(playerDocuments[i]));
            }
        }

        // setup la seccion del rival
        List<uint> rivalDocuments = _documentManager.GetRivalDocs();
        for (int i = 0; i < rivalDocuments.Count; i++)
        {
            if (i < _rivalDocumentsInstanciated.Count)
            {
                updateUIDocument(_rivalDocumentsInstanciated[i], _documentManager.GetDocument(rivalDocuments[i]));
            }
            else
            {
                addUIDocument(_documentManager.GetDocument(rivalDocuments[i]));
            }
        }
    }

    public override void Open()
    {
        setupUIDocuments();

        // mover pantalla ?
    }

    public override void Close()
    {
        
    }
}