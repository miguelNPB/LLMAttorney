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
    public bool isDemanda = false; // true = demanda realizada, false = No se ha hecho la demanda
    public bool attemptedConciliation = false;
    public float conciliationRivalInstantRejectProbability; // valor del 0.f al 1.f que indica la probabilidad de que el rival rechace cualquier intento de conciliacion (0 = nunca, 1 = siempre)
    // datos interacciones cliente
    public List<ConversationMessage> clientMessages = new List<ConversationMessage>();
    public List<ConversationMessage> procuratorMessages = new List<ConversationMessage>();
    public string clientName = "";
    public string procuratorName = "";
    public string demandedEntityName = ""; // demandado o demandador en caso de ser respuesta

    public string caseDescription = ""; // Descripcion del caso para pasar a llm contraria

    // resto

    public void SetDemanda() 
    {
        isDemanda = true;
    }
}
