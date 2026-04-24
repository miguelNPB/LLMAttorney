using System.Collections.Generic;
using UnityEngine;

public enum DocumentType { Perito, Report, Witness, ReceiptFacture }

/// <summary>
/// Clase para gestionar la creacion y lectura de documentos. Una vez creado un documento no se puede eliminar.
/// </summary>
public class DocumentManager : MonoBehaviour
{
    private Dictionary<uint, Document> _documents = new Dictionary<uint, Document>();

    private List<uint> _playerDocs = new List<uint>();
    private List<uint> _rivalDocs = new List<uint>();

    uint ids = 0;

    /// <summary>
    /// Devuelve la cantidad de documentos en la lista de doucmentos
    /// </summary>
    /// <returns></returns>
    public int GetDocumentCount()
    {
        return _documents.Count;
    }

    /// <summary>
    /// Devuelve el documento que corresopnde a la id pasada
    /// </summary>
    public Document GetDocument(uint id)
    {
        return _documents[id];
    }

    /// <summary>
    /// Devuelve una lista con los ids de los documentos generados por el jugador
    /// </summary>
    /// <returns></returns>
    public List<uint> GetPlayerDocs()
    {

        return _playerDocs;
    }

    /// <summary>
    /// Devuelve una lista con los ids de los documentos generados del rival
    /// </summary>
    /// <returns></returns>
    public List<uint> GetRivalDocs()
    {
        return _rivalDocs;
    }

    public void RegisterSentDocumentToProcurador(uint docId)
    {
        _documents[docId].SendDocumentToProcurator();
    }

    /// <summary>
    /// Crea un documento y devuelve su ID.
    /// </summary>
    public uint CreateDocument(string docName, DocumentType docType, string content, bool docIsRelevant, int cost, bool isOpponentDoc = false, bool isSentToProcurador = false)
    {
        Document doc = new Document(ids, docType, docName, content, docIsRelevant, cost, isOpponentDoc, isSentToProcurador);
        _documents.Add(ids, doc);

        if (isOpponentDoc)
            _rivalDocs.Add(ids);
        else
        {
            _playerDocs.Add(ids);

            if (BudgetManager.Instance != null)
                BudgetManager.Instance.AddExpense($"0 {cost}", docType, docName);
        }

        ids++;


        return doc.GetId();
    }


    private void Awake()
    {

    }
}