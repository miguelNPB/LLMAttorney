using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// Objeto serializable con las variables que incluye una llamada API a nuestro server python sin jsonschema
/// </summary>
[System.Serializable]
public class LLMAttorneyRequest
{
    public string LLMConfig;
    public string prompt;
    public float temperature;
    public bool rag_use;
    public int rag_index;
}

/// <summary>
/// Objeto serializable con las variables que incluye una llamada API a nuestro server python con jsonschema
/// </summary>
[System.Serializable]
public class LLMAttorneyRequestJSONSchema
{
    public string LLMConfig;
    public string prompt;
    public float temperature;
    public JsonSchema json_schema;
    public bool rag_use;
    public int rag_index;
}

/// <summary>
/// Enum con todos los tipos de campos que puede tener un JSON Schema
/// </summary>
public enum JsonDataType
{
    [System.Runtime.Serialization.EnumMember(Value = "string")]
    String,
    [System.Runtime.Serialization.EnumMember(Value = "number")]
    Float,
    [System.Runtime.Serialization.EnumMember(Value = "integer")]
    Integer,
    [System.Runtime.Serialization.EnumMember(Value = "boolean")]
    Boolean,
    [System.Runtime.Serialization.EnumMember(Value = "object")]
    Object,
    [System.Runtime.Serialization.EnumMember(Value = "array")]
    Array
}

/// <summary>
/// Serializable para almacenar un JSON Schema
/// </summary>
[Serializable]
public class JsonSchema
{
    [JsonConverter(typeof(StringEnumConverter), true)]
    readonly JsonDataType type = JsonDataType.Object;

    [Tooltip("Diccionario con pares de identificadores string y tipos de propiedades que devolvera el json")]
    public Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

    [Tooltip("Este campo se rellena solo, no hace falta meter nada. Esta aqui para que salga en la request al servidor")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string> required = null;
}


/// <summary>
/// Esta clase sirve para almacenar un campo dentro de una estructura JSON que mandaremos a la LLM
/// </summary>
[Serializable]
public class PropertyInfo
{
    [JsonConverter(typeof(StringEnumConverter), true)]
    public JsonDataType type;

    [Tooltip("Si usamos tipo object, rellenar esta lista con los campos que queramos")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, PropertyInfo> properties = null;

    [Tooltip("Este campo se rellena solo, no hace falta meter nada. Esta aqui para que salga en la request al servidor")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string> required = null;


    public PropertyInfo(JsonDataType type)
    {
        this.type = type;

        if (type == JsonDataType.Object)
            properties = new Dictionary<string, PropertyInfo>();
    }
}

/// <summary>
/// Clase Singleton que sirve para hacer llamadas a nuestro servidor LLMAttorney
/// </summary>
public class LLMSystemAPI : MonoBehaviour
{
    // nombre de la ip, si es local poner localhost
    public string ip = "localhost";
    public int port = 8000;

    private bool _sendingPrompt = false;
    private bool _changingPDF = false;

    public static LLMSystemAPI Instance { get; private set; }

    private struct PromptData
    {
        public Action<bool, string> onComplete;
        public bool useJsonSchema;
        public LLMAttorneyRequest requestNormalData;
        public LLMAttorneyRequestJSONSchema requestJsonData;
    }

    private Queue<PromptData> _promptsQueue = new Queue<PromptData>();

    /// <summary>
    /// Rellena automaticamente el campo required de un PropertyInfo para prepararlo para una peticion al servidor
    /// </summary>
    private void updateRequiredField(PropertyInfo propertyInfo)
    {
        if (propertyInfo.type == JsonDataType.Object && propertyInfo.properties != null)
        {
            propertyInfo.required = new List<string>(propertyInfo.properties.Keys);
            foreach (PropertyInfo prop in propertyInfo.properties.Values)
            {
                updateRequiredField(prop);
            }
        }
    }

    /// <summary>
    /// Cambia la ip del servidor
    /// </summary>
    public void ChangeServerIP(string newIP)
    {
        ip = newIP;
    }

    /// <summary>
    /// Encola un prompt en la cola de prompts. Cuando sea su turno en la cola, se manda y al recibir la respuesta del servidor llama al Action onComplete, con un booleano success y el string con el contenido.
    /// </summary>
    /// <param name="onComplete">Callback que se llamara con el resultado</param>
    /// <param name="prompt">Prompt de generación de contenido</param>
    /// <param name="LLMConfig">Texto con instrucciones de como debe responder el LLM</param>
    /// <param name="schema">Esquema JSON de como queremos que responda el LLM de forma mas guiada. En caso de no necesitarlo, pasar null y devolvera un string</param>
    /// <param name="temperature">float en el rango [0f, 1f] que indica como de creativo es el LLM. 0 = Predecible 1 = Creativo</param>
    /// <param name="ragUse">bool que marca si el LLM debe usar la informacion aportada con el Rag para responder al prompt o no</param>
    /// <param name="ragIndex">int que marca el rag que debemos de utilizar para la llamada</param>
    public void SendPrompt(Action<bool, string> onComplete, string prompt, string LLMConfig, JsonSchema schema = null, float temperature = 0.8f, bool ragUse = false, int ragIndex = 0)
    {
        if (schema == null)
        {
            // Crear la request
            var requestData = new LLMAttorneyRequest
            {
                LLMConfig = LLMConfig,
                prompt = prompt,
                temperature = temperature,
                rag_use = ragUse,
                rag_index = ragIndex
            };

            _promptsQueue.Enqueue(new PromptData { onComplete = onComplete, requestJsonData = null, requestNormalData = requestData, useJsonSchema = false });
        }
        else
        {
            schema.required = new List<string>();
            foreach (var pinfo in schema.properties)
            {
                schema.required.Add(pinfo.Key);
                updateRequiredField(pinfo.Value);
            }

            // Crear la request
            var requestData = new LLMAttorneyRequestJSONSchema
            {
                LLMConfig = LLMConfig,
                prompt = prompt,
                temperature = temperature,
                json_schema = schema,
                rag_use = ragUse,
                rag_index = ragIndex
            };

            _promptsQueue.Enqueue(new PromptData { onComplete = onComplete, requestJsonData = requestData, requestNormalData = null, useJsonSchema = true });
        }
    }

    /// <summary>
    /// Metodo interno para registrar el log del envio de prompt y con un PromptData serializar a JSON y mandar a SendRequest.
    /// </summary>
    /// <param name="promptData"></param>
    private void sendPrompt(PromptData promptData)
    {
        string prompt = promptData.useJsonSchema ? promptData.requestJsonData.prompt : promptData.requestNormalData.prompt;
        string llmConfig = promptData.useJsonSchema ? promptData.requestJsonData.LLMConfig: promptData.requestNormalData.LLMConfig;
        float temperature = promptData.useJsonSchema ? promptData.requestJsonData.temperature : promptData.requestNormalData.temperature;
        bool ragUse = promptData.useJsonSchema ? promptData.requestJsonData.rag_use: promptData.requestNormalData.rag_use;
        int ragIndex = promptData.useJsonSchema ? promptData.requestJsonData.rag_index : promptData.requestNormalData.rag_index;
        LogSystem.Instance.LogString($"[Fase: {SceneManager.GetActiveScene().buildIndex}] [SEND PROMPT]:" + "\nPrompt: " + prompt + "\nContext: " + llmConfig + "\nTemperature: " + temperature + " RagUse: " + ragUse + " RagIndex: " + ragIndex);

        string json = JsonConvert.SerializeObject(promptData.useJsonSchema ? promptData.requestJsonData : promptData.requestNormalData, Formatting.Indented);

        StartCoroutine(SendPromptRequest(json, promptData.onComplete));
    }

    /// <summary>
    /// Coroutina que manda la request al servidor
    /// </summary>
    /// <param name="json"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    private IEnumerator SendPromptRequest(string json, Action<bool, string> onComplete)
    {
        string _ip = ip;
        if (ip != "localhost")
            _ip = "http://" + ip;

        UnityWebRequest www = new UnityWebRequest(_ip + ":" + port.ToString() + "/ask", "POST");

        // empaquetamos el contenido en la UnityWebRequest
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        // mandamos la request y esperamos
        yield return www.SendWebRequest();

        _sendingPrompt = false;

        string recievedString = www.downloadHandler.text;

        bool success = www.result == UnityWebRequest.Result.Success;
        string response = success ? recievedString : ("Error LLMAtorney: " + www.error);

        bool isCallbackValid = onComplete != null;
        if (isCallbackValid && onComplete.Target is UnityEngine.Object targetUnityObject)
        {
            if (targetUnityObject == null)
            {
                isCallbackValid = false;
            }
        }
        if (isCallbackValid)
        {
            LogSystem.Instance.LogString($"[Fase: {SceneManager.GetActiveScene().buildIndex}] [RECIEVE PROMPT]:\n" + response);
            onComplete?.Invoke(success, response);
            if (!success)
                Debug.LogError(response);
        }
        else
        {
            Debug.LogWarning("Prompt invalidado porque el gameobject del callback asociado ha sido desactivado o destruido");
        }

        if (!success)
        {
            Debug.LogError(response);
        }
    }

    /// <summary>
    /// Metodo publico parr cambiar el pdf que usa el servidor como rag para los datos del caso.
    /// </summary>
    /// <param name="pdfPath"></param>
    /// <param name="id"></param>
    /// <param name="onComplete"></param>
    public void ChangePDFCase(string pdfPath, int id, Action onComplete, Action<string> onError)
    {
        if (!_changingPDF)
        {
            _changingPDF = true;
            StartCoroutine(sendChangeCasePDFRequest(pdfPath, id, onComplete, onError));
        }
        else
        {
            Debug.LogError("Error, ya se esta cambiando el pdf en curso");
        }
    }

    /// <summary>
    /// Coroutina para mandar peticiones para cambiar el pdf que usa el servidor como rag case data.
    /// </summary>
    /// <param name="pdfPath"></param>
    /// <param name="id"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    private IEnumerator sendChangeCasePDFRequest(string pdfPath, int id, Action onComplete, Action<string> onError)
    {
        if (!File.Exists(pdfPath))
        {
            Debug.LogError($"El archivo no existe en la ruta: {pdfPath}");
            _changingPDF = false;
            yield break;
        }

        byte[] pdfBytes = File.ReadAllBytes(pdfPath);
        string pdfFilename = Path.GetFileName(pdfPath);

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        formData.Add(new MultipartFormDataSection("id", id.ToString()));
        formData.Add(new MultipartFormFileSection("file", pdfBytes, pdfFilename, "application/pdf"));

        string _ip = ip;
        if (ip != "localhost")
            _ip = "http://" + ip;

        string url = $"{_ip}:{port.ToString()}/upload-pdf";
        using (UnityWebRequest www = UnityWebRequest.Post(url, formData))
        {
            // enviar peticion
            yield return www.SendWebRequest();

            bool success = www.result == UnityWebRequest.Result.Success;
            if (success)
            {
                Debug.Log($"Exito cambiando pdf de case data RAG: {www.downloadHandler.text}");
                onComplete?.Invoke();
            }
            else
            {
                Debug.LogError($"Error cambiando pdf de case data RAG: {www.error} + {www.downloadHandler.text}");
                onError?.Invoke($"Error cambiando pdf de case data RAG: {www.error} + {www.downloadHandler.text}\"");
            }
        }
        _changingPDF = false;
    }

    /// <summary>
    /// Pide el id del caso usado en el servidor 
    /// </summary>
    /// <param name="onComplete"></param>
    /// <param name="onError"></param>
    public void GetServerCaseID(Action<int> onComplete, Action<string> onError)
    {
        StartCoroutine(sendGetServerCaseIDRequest(onComplete, onError));
    }

    private IEnumerator sendGetServerCaseIDRequest(Action<int> onComplete, Action<string> onError)
    {
        string _ip = ip;
        if (ip != "localhost")
            _ip = "http://" + ip;

        string url = $"{_ip}:{port.ToString()}/getcase-id";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                onError?.Invoke($"Servidor no activo o ip errónea. \nMas información del error: {request.error}");
            }
            else
            {
                try
                {
                    // Leemos la respuesta del servidor
                    string responseText = request.downloadHandler.text;
                    string cleanResponse = responseText.Replace("\"", "").Trim();

                    if (int.TryParse(cleanResponse, out int caseId))
                    {
                        onComplete?.Invoke(caseId);
                    }
                    else
                    {
                        onError?.Invoke($"El servidor ha devuelto ID con formato inválido. Respuesta cruda: {responseText}");
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Error procesando la del ID del caso del servidor: {e.Message}");
                }
            }
        }
    }

    private void Update()
    {
        if (!_sendingPrompt && _promptsQueue.Count > 0)
        {
            _sendingPrompt = true;
            PromptData prompt = _promptsQueue.Dequeue();
            sendPrompt(prompt);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
