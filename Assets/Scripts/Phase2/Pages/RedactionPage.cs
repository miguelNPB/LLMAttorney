using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RedactionPage : PCPage
{
    public int scrollSpeed = 5;

    [Header("Demanda referencias")]
    public VerticalLayoutGroup verticalLayoutDemanda;
    public RectTransform lastElementLayoutDemanda;
    public TMP_Text textNombreProcuradorDemanda;
    public TMP_Text textNombreClienteDemanda;
    public TMP_Text textEntidadDemandanteDemanda;
    public TMP_InputField inputNombreAbogadoDemanda;
    public TMP_InputField inputResumenDemanda;
    public TMP_InputField inputListaHechosDemanda;
    public TMP_InputField inputResumenIntentoConciliacionDemanda;
    public TMP_InputField inputListaArticulosUsadosDemanda;
    public TMP_InputField inputPeticionTribunalDemanda;

    [Header("Respuesta a demanda referencias")]
    public VerticalLayoutGroup verticalLayoutRespuestaDemanda;
    public RectTransform lastElementLayoutRespuestaDemanda;
    public TMP_Text textNombreProcuradorRespuestaDemanda;
    public TMP_Text textNombreClienteRespuestaDemanda;
    public TMP_Text textEntidadDemandanteRespuestaDemanda;
    public TMP_InputField inputNombreAbogadoRespuestaDemanda;
    public TMP_InputField inputListaHechosRespuestaDemanda;
    public TMP_InputField inputResumenIntentoConciliacionRespuestaDemanda;
    public TMP_InputField inputListaArticulosUsadosRespuestaDemanda;



    private RectTransform _rectTrDemanda;
    private RectTransform _rectTrRespuestaDemanda;

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

    private void SetupDemanda(string nameCliente, string nameProcurador, string nameDemandado)
    {
        textNombreClienteDemanda.text = nameCliente;
        textNombreProcuradorDemanda.text = nameProcurador;
        textEntidadDemandanteDemanda.text = nameDemandado;
    }

    private void SetupRespuestaDemanda(string nameCliente, string nameProcurador, string nameDemandador)
    {
        textNombreClienteRespuestaDemanda.text = nameCliente;
        textNombreProcuradorRespuestaDemanda.text = nameProcurador;
        textEntidadDemandanteRespuestaDemanda.text = nameDemandador;
    }

    private void OnDisable()    
    {
        InputSystem.Instance.OnScrollPerformed -= OnScroll;
    }

    private void OnEnable()
    {
        InputSystem.Instance.OnScrollPerformed += OnScroll;
    }

    private void Start()
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
            SetupDemanda(GameSystem.Instance.CaseData.clientName, GameSystem.Instance.CaseData.procuradorName, GameSystem.Instance.CaseData.clienteRivalName);

            _rectTrDemanda.gameObject.SetActive(true);

            verticalLayoutDemanda.padding.top = 20;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTrDemanda);
        } 
        else
        {
            SetupRespuestaDemanda(GameSystem.Instance.CaseData.clientName, GameSystem.Instance.CaseData.procuradorName, GameSystem.Instance.CaseData.clienteRivalName);

            _rectTrRespuestaDemanda.gameObject.SetActive(true);

            verticalLayoutRespuestaDemanda.padding.top = 20;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTrRespuestaDemanda);
        }
    }

    public override void Close()
    {
        // desactivar los gameobject de la pagina
        if (GameSystem.Instance.CaseData.isDemanda)
        {
            _rectTrDemanda.gameObject.SetActive(false);
        }
        else
        {
            _rectTrRespuestaDemanda.gameObject.SetActive(false);
        }
    }
}
