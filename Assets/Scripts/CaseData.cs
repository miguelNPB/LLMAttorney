using System.Collections.Generic;

/// <summary>
/// Struct para almacenar mensajes de una conversacion entre el jugador y otra entidad
/// </summary>
public struct ConversationMessage
{
    public string text;
    public bool fromPlayer;

    public ConversationMessage(string text, bool fromPlayer)
    {
        this.text = text;
        this.fromPlayer = fromPlayer;
    }
}

/// <summary>
/// Clase con todos los datos necesarios sobre el caso
/// </summary>
public class CaseData
{
    private bool _isDemandaSent = false; // true = demanda realizada, false = No se ha hecho la demanda
    private bool _attemptedConciliation = false;
    private float _conciliationRivalInstantRejectProbability; // valor del 0.f al 1.f que indica la probabilidad de que el rival rechace cualquier intento de conciliacion (0 = nunca, 1 = siempre)
    private string _clientName;
    private string _procuratorName;
    private string _demandedEntityName; // demandado o demandador en caso de ser respuesta
    private string _caseDescription; // Descripcion del caso para pasar a llm contraria
    
    // getters
    public bool isDemandaSent => _isDemandaSent;
    public bool attemptedConciliation => _attemptedConciliation;
    public float conciliationRivalInstantRejectProbability => _conciliationRivalInstantRejectProbability;
    public string clientName => _clientName;
    public string procuratorName => _procuratorName;
    public string demandedEntityName => _demandedEntityName;
    public string caseDescription => _caseDescription;

    // variables publicas que cambian en el curso del caso
    public List<ConversationMessage> clientMessages = new List<ConversationMessage>();
    public List<ConversationMessage> procuratorMessages = new List<ConversationMessage>();
    public DocumentManager documentManager = new DocumentManager();


    public CaseData(
        float conciliationRivalInstantRejectProbability,
        string clientName,
        string procuratorName,
        string demandedEntityName,
        string caseDescription)
    {
        _conciliationRivalInstantRejectProbability = conciliationRivalInstantRejectProbability;
        _clientName = clientName;
        _procuratorName = procuratorName;
        _demandedEntityName = demandedEntityName;
        _caseDescription = caseDescription;
    }


    /// <summary>
    /// Se llama una vez se ha intentado una conciliacion
    /// </summary>
    public void AttemptConciliacion()
    {
        _attemptedConciliation = true;
    }

    /// <summary>
    /// Llamado al pulsar el boton de mandar demanda a procurador.
    /// </summary>
    public void SentDemandaToProcurador()
    {
        _isDemandaSent = true;
    }
}
