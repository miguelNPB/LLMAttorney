using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

/// <summary>
/// Pagina para redactar el archivo de la demanda
/// </summary>
public class RedactLawsuitPage : IPage
{
    [SerializeField] private int _scrollSpeed = 25;

    [Header("References")]
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
    [SerializeField] private Button _sendDemandButton;
    [SerializeField] private GameObject _warningSendDemandText;
    [SerializeField] private Phase2Manager _phase2Manager;
    [SerializeField] private List<TMP_Text> _textsInOrder;
    [SerializeField] private ProcuratorChatPage _procuratorChatPage;

    private RectTransform _rectTr;
    private const int MIN_SCROLL_VALUE = 20;
    private const int SCROLL_OFFSET = 1000;
    private bool _readyToSign = false;
    private bool _signed = false;

    /// <summary>
    /// Llamado al terminar la demanda, firmarla y mandarla al procurador
    /// </summary>
    public void SignDemandAndSendToProcurador()
    {
        if (_readyToSign)
        {
            setLawsuitText();

            _signed = true;
            _phase2Manager.EnablePriorHearing(true);

            _inputLawyerName.interactable = false;
            _inputDigo.interactable = false;
            _inputListHechos.interactable = false;
            _inputConciliacion.interactable = false;
            _inputListArticulos.interactable = false;
            _inputPeticion.interactable = false;

            _procuratorChatPage.ManuallyAddMessage("Buenas, te adjunto la demanda escrita.", true);
            _procuratorChatPage.ManuallyAddMessage("Perfecto, se lo paso al tribunal y a la otra parte. Con esto ya estaría todo, cuando me digas, llamo al tribunal para agendar la audiencia previa.", false);

            _computerSystem.ToggleNotification(Page.PriorHearing, true);
            _computerSystem.ToggleNotification(Page.ProcuratorChat, true);
            _computerSystem.GoToMainMenu();
        }
    }


    private void setLawsuitText()
    {
        string initialText = _textsInOrder[0].text + "\n" + _textsInOrder[1].text;

        string placeholderText = "________________________________________";

        int index = initialText.IndexOf(placeholderText);
        initialText = initialText.Remove(index, placeholderText.Length).Insert(index, _textProcuratorName.text);

        index = initialText.IndexOf(placeholderText);
        initialText = initialText.Remove(index, placeholderText.Length).Insert(index, _textClientName.text);

        index = initialText.IndexOf(placeholderText);
        initialText = initialText.Remove(index, placeholderText.Length).Insert(index, _inputLawyerName.text);

        initialText += _textsInOrder[2].text + "\n";

        string nextText = _textsInOrder[3].text;
        index = nextText.IndexOf(placeholderText);
        nextText = nextText.Remove(index, placeholderText.Length).Insert(index, _textDemandedEntity.text) + "\n";

        initialText += nextText;


        string lawsuitText = initialText;
        for (int i = 4; i < _textsInOrder.Count; i++)
        {
            lawsuitText += (_textsInOrder[i].text + "\n");
        }

        GameSystem.Instance.CaseData.SetLawsuitText(lawsuitText);

        Debug.Log(lawsuitText);
    }

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
        _computerSystem.ToggleNotification(Page.Redaction, false);

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

    private void Update()
    {
        if (!_signed)
        {
            _readyToSign = _inputLawyerName.text.Length > 1 && _inputDigo.text.Length > 1 && _inputListHechos.text.Length > 1 
                && _inputConciliacion.text.Length > 1 && _inputListArticulos.text.Length > 1 && _inputPeticion.text.Length > 1;

            _warningSendDemandText.SetActive(!_readyToSign);
            _sendDemandButton.interactable = _readyToSign;
        }
    }
}
