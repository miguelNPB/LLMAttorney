using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pagina para redactar el archivo de la demanda
/// </summary>
public class RedactLawsuitPage : IPage
{
    [SerializeField] private int _scrollSpeed = 5;

    [Header("Demanda referencias")]
    [SerializeField] private VerticalLayoutGroup _verticalLayout;
    [SerializeField] private RectTransform _lastElementLayout;
    [SerializeField] private TMP_Text _textProcuratorName;
    [SerializeField] private TMP_Text _textClientName;
    [SerializeField] private TMP_Text _textDemandedEntity;
    [SerializeField] private TMP_InputField _inputLawyerName;
    [SerializeField] private TMP_InputField _inputDigo;
    [SerializeField] private TMP_InputField _inputListHechos;
    [SerializeField] private TMP_InputField _inputConciliacion;
    [SerializeField] private TMP_InputField _inputListArticulos;
    [SerializeField] private TMP_InputField _inputPeticion;


    private RectTransform _rectTr;
    private const int MIN_SCROLL_VALUE = 20;
    private const int SCROLL_OFFSET = 1000;

    /// <summary>
    /// Aumenta el padding top para scrollear el texto de la demanda
    /// </summary>
    /// <param name="direction"></param>
    private void scroll(float direction)
    {
        int oldValue = _verticalLayout.padding.top;
        _verticalLayout.padding.top = Mathf.Clamp(oldValue + (_scrollSpeed * (int)direction), (int)_lastElementLayout.position.y + _verticalLayout.padding.top - SCROLL_OFFSET, MIN_SCROLL_VALUE);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTr);
    }

    /// <summary>
    /// Inicializa los textos a rellenar de nombres de las entidades principales de la demanda
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="procuratorName"></param>
    /// <param name="demandedEntityName"></param>
    private void setup(string clientName, string procuratorName, string demandedEntityName)
    {
        _textClientName.text = clientName;
        _textProcuratorName.text = procuratorName;
        _textDemandedEntity.text = demandedEntityName;
    }

    private void OnDisable()    
    {
        InputSystem.Instance.onScrollPerformed -= scroll;
    }

    private void OnEnable()
    {
        InputSystem.Instance.onScrollPerformed += scroll;
    }

    private void Start()
    {
        _rectTr = _verticalLayout.GetComponent<RectTransform>();
    }
    public override void Open()
    {
        computerSystem.ToggleNotification(Page.Redaction, false);

        // activar los gameobject de la pagina
        setup(GameSystem.Instance.CaseData.clientName, GameSystem.Instance.CaseData.procuratorName, GameSystem.Instance.CaseData.demandedEntityName);

        _rectTr.gameObject.SetActive(true);
        _verticalLayout.padding.top = 20;

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTr);
    }

    public override void Close()
    {
        _rectTr.gameObject.SetActive(false);
    }
}
