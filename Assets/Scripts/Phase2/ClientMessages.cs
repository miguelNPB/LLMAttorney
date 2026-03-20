using System;
using UnityEngine;

public class ClientMessages : MessagesUIComponent
{
    [Serializable]
    private class Campo3
    {
        public string subcampo1;
        public string subcampo2;
    }
    [Serializable]
    private class ExampleClass
    {
        public string campo1;
        public float campo2;
        public Campo3 campo3;
        public string[] campo4;
    }



    public Action<string> OnRecieveMessage;

    public void RecieveDocument()
    {

    }

    public void RecieveChatMessage(bool success, string answer)
    {
        // deserializamos la respuesta
        ExampleClass jsonResponse = JsonUtility.FromJson<ExampleClass>(answer);

        Debug.Log("CAMPO1: " + jsonResponse.campo1);
        Debug.Log("CAMPO2: " + jsonResponse.campo2);
        Debug.Log("CAMPO3-subcampo1: " + jsonResponse.campo3.subcampo1);
        Debug.Log("CAMPO3-subcampo2: " + jsonResponse.campo3.subcampo2);


        foreach(var iterm in jsonResponse.campo4)
        {
            Debug.Log("CAMPO4: " + iterm);
        }
    }

    public void SendChatMessage()
    {
        JsonSchema schema = new JsonSchema();

        schema.properties.Add("campo1", new PropertyInfo(JsonDataType.String));
        schema.properties.Add("campo2", new PropertyInfo(JsonDataType.Float));

        PropertyInfo subProperty = new PropertyInfo(JsonDataType.Object);
        subProperty.properties.Add("subcampo1", new PropertyInfo(JsonDataType.String));
        subProperty.properties.Add("subcampo2", new PropertyInfo(JsonDataType.String));
        schema.properties.Add("campo3", subProperty);

        schema.properties.Add("campo4", new PropertyInfo(JsonDataType.Array));

        LLMAttorney_API.Instance.SendPrompt(API_TYPE.LLAMA, RecieveChatMessage, "Rellena el campo 1 con un pais y el campo 2 con el numero de habitantes, luego rellena el campo 3 con nombres de perros, y el cmapo 4 con nombres de mujeres", "", schema);
    }
    void Start()
    {
        PlaceMessages(GameSystem.Instance.CaseData.clientMessages);   
    }
}
