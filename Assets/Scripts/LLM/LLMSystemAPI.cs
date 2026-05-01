using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Objeto serializable con las variables que incluye una llamada API a nuestro server python sin jsonschema
/// </summary>
[System.Serializable]
public class LLMAttorneyRequest
{
    public string mode;
    public string LLMConfig;
    public string prompt;
    public float temperature;
    public int max_length;
    public bool rag_use;
    public int rag_index;
}

/// <summary>
/// Objeto serializable con las variables que incluye una llamada API a nuestro server python con jsonschema
/// </summary>
[System.Serializable]
public class LLMAttorneyRequestJSONSchema
{
    public string mode;
    public string LLMConfig;
    public string prompt;
    public float temperature;
    public int max_length;
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

public enum API_TYPE { LLAMA }

/**
 * Clase Singleton que sirve para hacer llamadas a nuestro servidor LLMAttorney
 */
public class LLMSystemAPI : MonoBehaviour
{
    // nombre de la ip, si es local poner localhost
    public string ip = "localhost";
    public int port = 8000;

    private bool _sendingPrompt = false;
    private bool _invalidPrompt = false;

    public static LLMSystemAPI Instance { get; private set; }

    /**
     * Convierte API_TYPE a string
     */
    private string APItypeToString(API_TYPE type)
    {
        switch (type)
        {
            case API_TYPE.LLAMA:
                return "Llama";
            default:
                return "";
        }
    }


    /// <summary>
    /// Rellena automaticamente el campo required de un PropertyInfo
    /// </summary>
    public void UpdateRequiredField(PropertyInfo propertyInfo)
    {
        if (propertyInfo.type == JsonDataType.Object && propertyInfo.properties != null)
        {
            propertyInfo.required = new List<string>(propertyInfo.properties.Keys);
            foreach (PropertyInfo prop in propertyInfo.properties.Values)
            {
                UpdateRequiredField(prop);
            }
        }
    }

    /// <summary>
    /// Invaida un prompt en curso
    /// </summary>
    public void CancelPrompt()
    {
        _invalidPrompt = true;
    }

    /**
     * Manda un prompt y al recibir la respuesta del servidor llama al Action onComplete, con un booleano success y el string con el contenido.
     * @param prompt Prompt de generación de contenido
     * @param onComplete callback que llamara al recibir la respuesta del servidor
     * @param LLMConfig Texto con instrucciones de como debe responder el LLM
     * @param schema Esquema JSON de como queremos que responda el LLM de forma mas guiada. En caso de no necesitarlo, pasar null y devolvera un string
     * @param temperature float en el rango [0f, 1f] que indica como de creativo es el LLM. 0 = Predecible 1 = Creativo
     * @param ragUse bool que marca si el LLM debe usar la informacion aportada con el Rag para responder al prompt o no
     * @param ragIndex int que marca el rag que debemos de utilizar para la llamada
     * @param max_length Tokens maximos del texto, esto no usarlo mucho q no funciona muy bien
     * @return Devuelve true si se ha podido mandar, si no hay ningun prompt encolado
     */
    public bool SendPrompt(API_TYPE apiType, Action<bool, string> onComplete, string prompt, string LLMConfig, JsonSchema schema = null, float temperature = 0.8f, bool ragUse = false, int ragIndex = 0, int max_length = 99999)
    {

        if (_sendingPrompt)
            return false;

        if (schema == null)
        {
            // Crear la request
            var requestData = new LLMAttorneyRequest
            {
                mode = APItypeToString(apiType),
                LLMConfig = LLMConfig,
                prompt = prompt,
                temperature = temperature,
                max_length = max_length,
                rag_use = ragUse,
                rag_index = ragIndex
            };

            string json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
            StartCoroutine(SendRequest(json, onComplete));
        }
        else
        {
            schema.required = new List<string>();
            foreach (var pinfo in schema.properties)
            {
                schema.required.Add(pinfo.Key);
                UpdateRequiredField(pinfo.Value);
            }

            // Crear la request
            var requestData = new LLMAttorneyRequestJSONSchema
            {
                mode = APItypeToString(apiType),
                LLMConfig = LLMConfig,
                prompt = prompt,
                temperature = temperature,
                max_length = max_length,
                json_schema = schema,
                rag_use = ragUse,
                rag_index = ragIndex
            };

            string json = JsonConvert.SerializeObject(requestData, Formatting.Indented);

            StartCoroutine(SendRequest(json, onComplete));
        }

        return true;
    }

    /// <summary>
    /// Manda la request igual al servidor, pero espera a recibir respuesta antes de seguir.
    /// </summary>
    /// <param name="apiType"></param>
    /// <param name="onComplete"></param>
    /// <param name="prompt"></param>
    /// <param name="LLMConfig"></param>
    /// <param name="schema"></param>
    /// <param name="temperature"></param>
    /// <param name="ragUse"></param>
    /// <param name="ragIndex"></param>
    /// <param name="max_length"></param>
    /// <returns></returns>
    /// 
    public IEnumerator SendPromptCoroutine(API_TYPE apiType, Action<bool, string> onComplete, string prompt, string LLMConfig, JsonSchema schema = null, float temperature = 0.8f, bool ragUse = false, int ragIndex = 0, int max_length = 99999)
    {

        if (_sendingPrompt)
            yield break;

        if (schema == null)
        {
            // Crear la request
            var requestData = new LLMAttorneyRequest
            {
                mode = APItypeToString(apiType),
                LLMConfig = LLMConfig,
                prompt = prompt,
                temperature = temperature,
                max_length = max_length,
                rag_use = ragUse,
                rag_index = ragIndex
            };

            string json = JsonConvert.SerializeObject(requestData, Formatting.Indented);
            yield return StartCoroutine(SendRequest(json, onComplete));
        }
        else
        {
            schema.required = new List<string>();
            foreach (var pinfo in schema.properties)
            {
                schema.required.Add(pinfo.Key);
                UpdateRequiredField(pinfo.Value);
            }

            // Crear la request
            var requestData = new LLMAttorneyRequestJSONSchema
            {
                mode = APItypeToString(apiType),
                LLMConfig = LLMConfig,
                prompt = prompt,
                temperature = temperature,
                max_length = max_length,
                json_schema = schema,
                rag_use = ragUse,
                rag_index = ragIndex
            };

            string json = JsonConvert.SerializeObject(requestData, Formatting.Indented);

            yield return StartCoroutine(SendRequest(json, onComplete));
        }
    }

    /**
     * Funcion privada que manda la request al servidor
     */
    private IEnumerator SendRequest(string json, Action<bool, string> onComplete)
    {
        _sendingPrompt = true;
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

        string recievedString = www.downloadHandler.text;

        bool success = www.result == UnityWebRequest.Result.Success;
        string response = success ? recievedString : ("Error LLMAtorney: " + www.error);

        if (!_invalidPrompt)
        {
            onComplete?.Invoke(success, response);
        }
        else
        {
            Debug.LogWarning("Prompt invalidado porque el gameobject del callback asociado ha sido desactivado o destruido");
            _invalidPrompt = false;
        }

        if (!success)
        {
            Debug.LogError(response);
        }

        _sendingPrompt = false;
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //DontDestroyOnLoad(gameObject);
    }
}
