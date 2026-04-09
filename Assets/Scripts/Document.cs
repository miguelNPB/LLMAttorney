using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class Document : MonoBehaviour
{
    //Campos del Documento (Icono, nombre, contenido, valido para presentar)
    [SerializeField]
    Sprite imageChange;

    [SerializeField]
    TMP_Text fileNameObject = null;

    [SerializeField]
    GameObject fileContentObject = null;
    
    [SerializeField]
    [InspectorName("Document button")]
    GameObject documentButton;

    [SerializeField]
    [InspectorName("Exit button")]
    GameObject exitButton;

    private PromptType docType;
    private string fileName;
    private string content;
    private bool valid = false;
    private int cost =0;

    private bool sentToProcurador = false;

    private bool changed = false;
    void Start()
    {
        fileContentObject.gameObject.SetActive(false);
        exitButton.GetComponent<Button>().onClick.AddListener(OnClickDocument);
        documentButton.GetComponent<Button>().onClick.AddListener(OnClickDocument);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClickDocument()
    {
        if (!changed)
        {
            changed = true;
            documentButton.GetComponent<Image>().sprite = imageChange;
        }

        fileContentObject.gameObject.SetActive(!fileContentObject.gameObject.activeSelf);
    }

    #region GET&SET

    public string GetDocName() { return this.fileName; }
    public PromptType GetDocType() { return this.docType; }
    public string GetContent() { return this.content; }
    public bool IsValid() {  return this.valid; }
    public bool GetSentToProcurador() {  return sentToProcurador; }



    /// <summary>
    /// Llamar al mandar al procurador el documento
    /// </summary>
    public void OnSentToProcurador()
    {
        sentToProcurador = true;
    }

    /// <summary>
    /// Metodo para cambiar el nombre del documento
    /// </summary>
    /// <param name="name">Nombre nuevo del documento</param>
    public void SetDocName(string name)
    {
        this.fileName = name;

        if (fileNameObject != null)
            fileNameObject.text = fileName;
        
    }
    /// <summary>
    /// Metodo para cambiar el contenido del documento
    /// </summary>
    /// <param name="content">Contenido nuevo del dodumento</param>
    public void SetDocContent(string content)
    {
        this.content = content;
        if (fileContentObject != null)
            fileContentObject.GetComponentInChildren<TMP_Text>().text = this.content;
    }

    /// <summary>
    /// Sirve para inicializar todos los valores del documento
    /// </summary>
    /// <param name="docName">Nombre nuevo del documento</param>
    /// <param name="docType">Tipo de documento</param>
    /// <param name="content">Contenido nuevo para el documento</param>
    /// <param name="valid">Es valido el documento</param>
    public void SetDoc(string docName, PromptType docType, string content, bool valid, int cost)
    {
        SetDocName(docName);
        SetDocContent(content);
        this.valid = valid;
        this.docType = docType;
        this.cost = cost;
    }




    #endregion
}
