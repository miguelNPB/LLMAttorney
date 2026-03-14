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
    TMP_Text fileNameObject;

    [SerializeField]
    TMP_Text fileContentObject;

    private DocType docType;

    private bool valid = false;

    private bool changed = false;
    void Start()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(OnClickDocument);
        fileContentObject.gameObject.SetActive(false);
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
            this.gameObject.GetComponent<Image>().sprite = imageChange;
        }

        fileContentObject.gameObject.SetActive(!fileContentObject.gameObject.active);
    }

    /// <summary>
    /// Metodo para cambiar el nombre del documento
    /// </summary>
    /// <param name="name">Nombre nuevo del documento</param>
    public void SetDocName(string name)
    {
        fileNameObject.text = name;
    }
    /// <summary>
    /// Metodo para cambiar el contenido del documento
    /// </summary>
    /// <param name="content">Contenido nuevo del dodumento</param>
    public void SetDocContent(string content)
    {
        fileContentObject.text = content;
    }

    /// <summary>
    /// Sirve para inicializar todos los valores del documento
    /// </summary>
    /// <param name="docName">Nombre nuevo del documento</param>
    /// <param name="docType">Tipo de documento</param>
    /// <param name="content">Contenido nuevo para el documento</param>
    /// <param name="valid">Es valido el documento</param>
    public void SetDoc(string docName, DocType docType, string content, bool valid)
    {
        SetDocName(docName);
        SetDocContent(content);
        this.valid = valid;
        this.docType = docType;
    }
}
