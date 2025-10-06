using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class GeminiAPI : MonoBehaviour
{
    public string apiKey = "";
    private string _url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    [System.Serializable]
    public class GeminiRequest
    {
        public Content[] contents;
    }

    [System.Serializable]
    public class Content
    {
        public Part[] parts;
    }

    [System.Serializable]
    public class Part
    {
        public string text;
    }

    [System.Serializable]
    public class GeminiResponse
    {
        public Candidate[] candidates;
    }

    [System.Serializable]
    public class Candidate
    {
        public Content content;
    }

    public void SendPrompt(string prompt, Action<string> onComplete)
    {
        StartCoroutine(SendAPIRequest(prompt, onComplete));
    }

    private IEnumerator SendAPIRequest(string prompt, Action<string> onComplete)
    {
        var requestData = new GeminiRequest
        {
            contents = new[]
            {
                new Content
                {
                    parts = new[]
                    {
                        new Part { text = prompt }
                    }
                }
            }
        };

        string json = JsonConvert.SerializeObject(requestData);
        
        UnityWebRequest www = new UnityWebRequest(_url + "?key=" + apiKey, "POST");
        
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        var jsonResponse = JsonConvert.DeserializeObject<GeminiResponse>(www.downloadHandler.text);
        string response = "";

        if (www.result == UnityWebRequest.Result.Success)
        {
            response = jsonResponse.candidates[0].content.parts[0].text;
        }
        else
        {
            response = "Gemini ha fallado: " + www.error;
        }

        onComplete?.Invoke(response);
    }
}
