using System.Collections.Generic;


public struct Message
{
    public bool fromPlayer;
    public string text;
}

public class CaseData
{
    List<Message> clientMessages; 
}
