using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISearchManager : MonoBehaviour
{

    [SerializeField]
    private GameObject _searchToolButton;

    [SerializeField]
    private GameObject _searchMenuView;

    [SerializeField] private TMP_Text resultText;
    //[SerializeField] private VerticalLayoutGroup layoutGroup;

    private string pendingMessage = "";
    private bool waitingPendingMessage = false;

    private IEnumerator CoroutinePendingMessage()
    {
        waitingPendingMessage = true;

        float timer = 0;

        while (waitingPendingMessage)
        {
            timer += Time.deltaTime;

            resultText.text = "";
            for (int i = 0; i < (timer % 3); i++)
                resultText.text += ".";

            yield return null;
        }

        resultText.text = pendingMessage;

        // actualizar caja de texto y lineas totales
        //resultText.ForceMeshUpdate();

    }

    /// <summary>
    /// Instancia un mensaje y le cambia el color segun si es player o no
    /// </summary>
    /// <param name="text"></param>
    /// <param name="fromPlayer"></param>
    public void AddMessage(string text)
    {
        resultText.text = text;

        resultText.ForceMeshUpdate();

    }

    public void ShowMessage()
    {
        resultText.ForceMeshUpdate();
    }

    public void PressAccessButton()
    {
        Debug.Log(!_searchMenuView.activeSelf);
        _searchMenuView.SetActive(!_searchMenuView.activeSelf);
    }

    /// <summary>
    /// Llamar a esto para añadir un mensaje que tenga una animacion de puntos suspensivos hasta que se llame a EndPendingMessage
    /// </summary>
    /// <param name="fromPlayer"></param>
    public void StartPendingMessage()
    {
        AddMessage(".");

        StartCoroutine(CoroutinePendingMessage());
    }
    
    /// <summary>
    /// Llamar esto para detener la animacion de puntos suspensivos y rellenar el mensaje con el contenido del texto
    /// </summary>
    /// <param name="text"></param>
    public void EndPendingMessage(string text)
    {
        pendingMessage = text;

        waitingPendingMessage = false;
    }

}
