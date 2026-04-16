using System.Collections.Generic;

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

public class CaseData
{
    public bool isDemanda = true; // true = demanda, false = respuesta a demanda
    public bool attemptedConciliation = false;
    public float conciliationRivalInstantRejectProbability; // valor del 0.f al 1.f que indica la probabilidad de que el rival rechace cualquier intento de conciliacion (0 = nunca, 1 = siempre)
    // datos interacciones cliente
    public List<ConversationMessage> clientMessages = new List<ConversationMessage>();
    public List<ConversationMessage> procuradorMessages = new List<ConversationMessage>();
    public string clientName = "";
    public string procuradorName = "";
    public string clienteRivalName = ""; // demandado o demandador en caso de ser respuesta

    public string caseDescription = ""; // Descripcion del caso para pasar a llm contraria

    // resto
}
