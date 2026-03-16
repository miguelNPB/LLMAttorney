using UnityEngine;

public class ClientMessages : MessagesUIComponent
{
    void Start()
    {
        PlaceMessages(GameSystem.Instance.CaseData.clientMessages);   
    }
}
