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
    // datos interacciones cliente
    public List<ConversationMessage> clientMessages = new List<ConversationMessage>();
    public string clientName = "";

    // resto
}
