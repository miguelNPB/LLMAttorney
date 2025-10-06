using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

#region LLMAttorney API 
[System.Serializable]
public class LLMAttorneyRequest
{
    public string mode;
    public string LLMConfig;
    public string prompt;
}
[System.Serializable]
public class LLMAttorneyResponse
{
    public string answer;
}
#endregion

public enum API_TYPE { GEMINI, LLAMA }
public class LLMAttorney_API : MonoBehaviour
{
    private string APItypeToString(API_TYPE type)
    {
        switch (type)
        {
            case API_TYPE.GEMINI:
                return "Gemini";
            case API_TYPE.LLAMA:
                return "Llama";
            default:
                return "";
        }
    }

    public void SendPrompt(API_TYPE apiType, string prompt, string LLMConfig, Action<bool, string> onComplete)
    {
        
            var requestData = new LLMAttorneyRequest
            {
                mode = APItypeToString(apiType),
                LLMConfig = LLMConfig,
                prompt = prompt,
            };

            string json = JsonConvert.SerializeObject(requestData);
            StartCoroutine(SendRequest(json, onComplete));
    }

    private IEnumerator SendRequest(string json, Action<bool, string> onComplete)
    {
        UnityWebRequest www = new UnityWebRequest("localhost:8000/ask", "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        var jsonResponse = JsonConvert.DeserializeObject<LLMAttorneyResponse>(www.downloadHandler.text);

        bool success = www.result == UnityWebRequest.Result.Success;
        string response = success ? jsonResponse.answer : ("Error LLMAtorney: " + www.error);

        onComplete?.Invoke(success, response);
    }
}
