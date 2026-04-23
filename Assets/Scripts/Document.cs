using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase para almacnear datos de un Documento
/// </summary>
public class Document
{
    private uint _id;
    private DocumentType _docType;
    private string _docName;
    private string _docContent;
    private bool _docIsRelevant; 
    private int _cost = 0;
    private bool _docIsRival = false;
    private bool _sentToProcurador = false;

    /// <summary>
    /// Devuelve el id unico del documento
    /// </summary>
    /// <returns></returns>
    public uint GetId() { return _id; }

    /// <summary>
    /// Devuelve el tipo del documento
    /// </summary>
    /// <returns></returns>
    public DocumentType GetDocType() { return _docType; }

    /// <summary>
    /// Devuelve el nombre de un documento
    /// </summary>
    /// <returns></returns>
    public string GetDocName() { return _docName; }

    /// <summary>
    /// Devuelve el contenido del documento
    /// </summary>
    /// <returns></returns>
    public string GetContent() { return _docContent; }

    /// <summary>
    /// Devuelve el coste de conseguir ese documento
    /// </summary>
    /// <returns></returns>
    public int GetCost() { return _cost; }

    /// <summary>
    /// Devuelve si el documento tiene relacion al caso o no. Si no lo es, al recurrirlo durante la fase 3, se deberá considerar invalidado
    /// </summary>
    /// <returns></returns>
    public bool IsDocumentRelevant() {  return _docIsRelevant; }

    /// <summary>
    /// Devuelve si el documento se ha mandado al procurador para adjunarlo al caso o no
    /// </summary>
    /// <returns></returns>
    public bool IsSentToProcurador() {  return _sentToProcurador; }

    /// <summary>
    /// Devuelve si el documento es de la parte rival
    /// </summary>
    /// <returns></returns>
    public bool IsRivalDoc() { return _docIsRival; }



    /// <summary>
    /// Llamar al mandar al procurador el documento. Este metodo solo debe llamarse desde el DocumentManager, 
    /// </summary>
    public void SendDocumentToProcurator()
    {
        _sentToProcurador = true;
    }


    public Document(uint id, DocumentType docType, string docName, string docContent, bool docIsRelevant,
        int cost = 0, bool docIsOpponent = false, bool sentToProcurador = false)
    {
        _id = id;
        _docType = docType;
        _docName = docName;
        _docContent = docContent;
        _docIsRelevant = docIsRelevant;
        _cost = cost;
        _docIsRival = docIsOpponent;
        _sentToProcurador = sentToProcurador;
    }
}
