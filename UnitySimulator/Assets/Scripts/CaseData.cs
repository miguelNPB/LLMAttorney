using System;
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
    private int _id; // id unico de este caso para diferenciar entre casos
    private bool _isDemandaSent = false; // true = demanda realizada, false = No se ha hecho la demanda
    private bool _attemptedConciliation = false;
    private float _conciliationRivalInstantRejectProbability; // valor del 0.f al 1.f que indica la probabilidad de que el rival rechace cualquier intento de conciliacion (0 = nunca, 1 = siempre)
    private string _clientName;
    private string _procuratorName;
    private string _demandedEntityName; // demandado o demandador en caso de ser respuesta
    private string _caseDescription; // Descripcion del caso para pasar a llm contraria
    private string _initialClientSpeech; // Monologo inicial del cliente explicando el caso
    private string _lawsuitText; // texto de la demanda
    private List<Document> _finalPlayerDocuments; // docuemntos finales para la sentencia del player
    private List<Document> _finalRivalDocuments; // docuemntos finales para la sentencia del rival

    // getters
    public int id => _id;
    public bool isDemandaSent => _isDemandaSent;
    public bool attemptedConciliation => _attemptedConciliation;
    public float conciliationRivalInstantRejectProbability => _conciliationRivalInstantRejectProbability;
    public string clientName => _clientName;
    public string procuratorName => _procuratorName;
    public string demandedEntityName => _demandedEntityName;
    public string caseDescription => _caseDescription;
    public string initialClientSpeech => _initialClientSpeech;
    public string lawsuitText => _lawsuitText;
    public List<Document> finalPlayerDocuments { get { return _finalPlayerDocuments; } }
    public List<Document> finalRivalDocuments { get { return _finalRivalDocuments; } }



    // variables publicas que cambian en el curso del caso
    public List<ConversationMessage> clientMessages = new List<ConversationMessage>();
    public List<ConversationMessage> procuratorMessages = new List<ConversationMessage>();
    public DocumentManager documentManager = new DocumentManager();




    public CaseData(
        int id,
        string clientName,
        string demandedEntityName,
        string caseDescription,
        string initialClientSpeech)
    {
        _id = id;
        _conciliationRivalInstantRejectProbability = UnityEngine.Random.Range(0.5f,1f);
        _clientName = clientName;
        _procuratorName = "Máximo Décimo Meridio";
        _demandedEntityName = demandedEntityName;
        _caseDescription = caseDescription;
        _initialClientSpeech = initialClientSpeech;

        clientMessages.Add(new ConversationMessage($"Hola, soy {_clientName} si tienes alguna duda sobre algo que pueda contarte para nuestro caso y demanda a {_demandedEntityName} o cuando sepas que documentos debo conseguir por favor dímelo.", false));
        procuratorMessages.Add(new ConversationMessage($"Buenas! Mi nombre es {_procuratorName}, seré tu procurador para este caso. Cualquier documento que consideres pertinente adjuntar al proceso, mándamelo y lo registraré.", false));
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

    /// <summary>
    /// Establece el texto de la demanda
    /// </summary>
    /// <param name="text"></param>
    public void SetLawsuitText(string text)
    {
        _lawsuitText = text;
    }

    /// <summary>
    /// Establece los documentos finales para considerar en la sentencia
    /// </summary>
    /// <param name="clientValidDocuments"></param>
    /// <param name="rivalValidDocuments"></param>
    public void SetSentenceDocuments(List<Document> clientValidDocuments, List<Document> rivalValidDocuments)
    {
        _finalPlayerDocuments = clientValidDocuments;
        _finalRivalDocuments = rivalValidDocuments;
    }
}
