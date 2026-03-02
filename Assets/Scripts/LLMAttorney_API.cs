using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;



#region LLMAttorney API 

/**
 * Objeto serializable con las variables que incluye una llamada API a nuestro server python
 */
[System.Serializable]
public class LLMAttorneyRequest
{
    public string mode;
    public string LLMConfig;
    public string prompt;
    public float temperature;
    public int max_length;
}

/**
 * Objeto serializado que contiene el string con la respuesta de la LLM
 */
[System.Serializable]
public class LLMAttorneyResponse
{
    public string answer;
}
#endregion

public enum API_TYPE { GEMINI, LLAMA }

/**
 * Clase Singleton que sirve para hacer llamadas a nuestro servidor LLMAttorney
 */
public class LLMAttorney_API : MonoBehaviour
{
    // nombre de la ip, si es local poner localhost
    public string ip = "localhost";
    public int port = 8000;

    public static LLMAttorney_API Instance { get; private set; }

    /**
     * Convierte API_TYPE a string
     */
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

    /**
     * Manda un prompt y al recibir la respuesta del servidor llama al Action onComplete, con un booleano success y el string con el contenido.
     * @param prompt Prompt de generación de contenido
     * @param LLMConfig Texto con instrucciones de como debe responder el LLM
     * @param temperature float en el rango [0f, 1f] que indica como de creativo es el LLM. 0 = Predecible 1 = Creativo
     * @param max_length Longitud maxima del texto
     * @param onComplete callback que llamara al recibir la respuesta del servidor
     */
    public void SendPrompt(API_TYPE apiType, string prompt, string LLMConfig, float temperature, int max_length, Action<bool, string> onComplete)
    {
        var requestData = new LLMAttorneyRequest
        {
            mode = APItypeToString(apiType),
            LLMConfig = LLMConfig,
            prompt = prompt,
            temperature = temperature,
            max_length = max_length,
        };

        string json = JsonConvert.SerializeObject(requestData);
        StartCoroutine(SendRequest(json, onComplete));
    }

    /**
     * Funcion privada que manda la request al servidor
     */
    private IEnumerator SendRequest(string json, Action<bool, string> onComplete)
    {
        UnityWebRequest www = new UnityWebRequest(ip + ":" + port.ToString() + "/ask", "POST");

        // empaquetamos el contenido en la UnityWebRequest
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // mandamos la request y esperamos
        yield return www.SendWebRequest();

        // deserializamos la respuesta
        var jsonResponse = JsonConvert.DeserializeObject<LLMAttorneyResponse>(www.downloadHandler.text);

        bool success = www.result == UnityWebRequest.Result.Success;
        string response = success ? jsonResponse.answer : ("Error LLMAtorney: " + www.error);

        // llamamos al callback
        onComplete?.Invoke(success, response);
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
}
