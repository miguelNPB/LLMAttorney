using Mono.Cecil.Cil;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;


public enum DocType
{
    Perito,
    Testigo,
    Informe
}
public class DocumentManager : MonoBehaviour
{
    // Region de controles del boton
    [SerializeField]
    Button[] documentButtons;
    [SerializeField]
    GameObject documentTab;
    [SerializeField]
    GameObject documentPrefab;
    [SerializeField]
    Vector2 startingPos;
    [SerializeField]
    Vector2 openPos;
    [SerializeField]
    float moveSpeed = 5f;

    // Region de movimiento de la pantalla
    private bool isAtEndingPos = false;
    private bool isMoving = false;
    private Vector2 startPos;
    private Vector2 targetPos;
    private float lerpProgress = 0f;

    //Region de generacion de documentos
    private Vector2 docPos = new Vector2(-60, 30);

    private List<GameObject> documents;
    void Start()
    {
        foreach(Button b in documentButtons)
            b.onClick.AddListener(OnClickDocumentsIcon);
        
        documents = new List<GameObject>();

        for (int i = 0; i < 10; i++)
        {
            CreateDocument("DOC" + i + ".txt", DocType.Perito, "ESTE ES EL DOC " + i, true);
        }
        
    }

    void Update()
    {
        if (isMoving)
        {
            lerpProgress += moveSpeed * Time.deltaTime;
            documentTab.transform.localPosition = Vector3.Lerp(startPos, targetPos, lerpProgress);

            if (lerpProgress >= 1f)
            {
                documentTab.transform.localPosition = targetPos;
                isMoving = false;
                lerpProgress = 0f;
            }
        }
    }
    #region Botones
    void OnClickDocumentsIcon()
    {

        if (isAtEndingPos)
        {
            // Move back to starting position
            startPos = documentTab.transform.localPosition;
            targetPos = startingPos;
            isAtEndingPos = false;
        }
        else
        {
            // Move to ending position
            startPos = documentTab.transform.localPosition;
            targetPos = openPos;
            isAtEndingPos = true;
        }

        lerpProgress = 0f;
        isMoving = true;
    }
    #endregion

    #region Documentos
    public void CreateDocument(string docName, DocType docType, string content, bool valid)
    {
        //Posicion puesta fea, hacerlo con mates mas tarde
        docPos.x += 20;
        if(docPos.x > 40)
        {
            docPos.x = -40;
            docPos.y -= 20;
        }

        //Creacion de un clon del prefab DOC
        GameObject aux = Instantiate(documentPrefab);
        
        //Inicializamos valores del DOC
        aux.GetComponent<Document>().SetDoc(docName, docType, content, valid);
        
        aux.transform.SetParent(documentTab.transform);
        aux.transform.localPosition = docPos;
        aux.transform.localScale = new Vector3(1, 1, 1);
        
        documents.Add(aux);

        Debug.Log(documents.Count);
    }
    #endregion
}
