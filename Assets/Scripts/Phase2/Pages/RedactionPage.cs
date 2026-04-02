using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RedactionPage : PCPage
{
    public VerticalLayoutGroup verticalLayoutDemanda;
    public VerticalLayoutGroup verticalLayoutRespuestaDemanda;
    public RectTransform lastElementLayoutDemanda;
    public RectTransform lastElementLayoutRespuestaDemanda;
    public int scrollSpeed = 5;

    private RectTransform _rectTrDemanda;
    private RectTransform _rectTrRespuestaDemanda;

    private float _totalHeight;

    private void OnScroll(float direction)
    {
        if (GameSystem.Instance.CaseData.isDemanda)
        {
            int oldValue = verticalLayoutDemanda.padding.top;
            verticalLayoutDemanda.padding.top = Mathf.Clamp(oldValue + (scrollSpeed * (int)direction), (int)lastElementLayoutDemanda.position.y + verticalLayoutDemanda.padding.top - 1000, 20);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTrDemanda);
        }
        else
        {
            int oldValue = verticalLayoutDemanda.padding.top;
            verticalLayoutRespuestaDemanda.padding.top = Mathf.Clamp(oldValue + (scrollSpeed * (int)direction), (int)lastElementLayoutRespuestaDemanda.position.y + verticalLayoutDemanda.padding.top - 1000, 20);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTrRespuestaDemanda);
        }   
    }

    private void OnDisable()    
    {
        InputSystem.Instance.OnScrollPerformed -= OnScroll;
    }

    private void OnEnable()
    {
        InputSystem.Instance.OnScrollPerformed += OnScroll;
    }

    private void Awake()
    {
        _rectTrDemanda = verticalLayoutDemanda.GetComponent<RectTransform>();
        _rectTrRespuestaDemanda = verticalLayoutRespuestaDemanda.GetComponent<RectTransform>();
    }
    public override void Open()
    {
        computerSystem.ToggleNotification(Page.Redaccion, false);

        // activar los gameobject de la pagina
        if (GameSystem.Instance.CaseData.isDemanda)
        {
            _rectTrDemanda.gameObject.SetActive(true);
            for (int i = 0; i < _rectTrDemanda.transform.childCount; i++)
                _rectTrDemanda.GetChild(i).gameObject.SetActive(true);

            verticalLayoutDemanda.padding.top = 20;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTrDemanda);
        } 
        else
        {
            _rectTrRespuestaDemanda.gameObject.SetActive(true);
            for (int i = 0; i < _rectTrRespuestaDemanda.transform.childCount; i++)
                _rectTrRespuestaDemanda.GetChild(i).gameObject.SetActive(true);

            verticalLayoutRespuestaDemanda.padding.top = 20;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTrRespuestaDemanda);
        }
    }

    public override void Close()
    {
        // desactivar los gameobject de la pagina
        if (GameSystem.Instance.CaseData.isDemanda)
        {
            for (int i = 0; i < _rectTrDemanda.transform.childCount; i++)
                _rectTrDemanda.GetChild(i).gameObject.SetActive(false);
        }
        else
        {
            for (int i = 0; i < _rectTrRespuestaDemanda.transform.childCount; i++)
                _rectTrRespuestaDemanda.GetChild(i).gameObject.SetActive(false);
        }
    }
}
