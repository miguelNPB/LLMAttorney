using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using static PersistCaseData;

/// <summary>
/// Clase para gestionar el cambio de caseData y cambio de pdf del caso en el servidor python
/// </summary>
public class CaseSwapManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _displayText;


    private bool _inProgress = false;
    private string _pendingMessage;


    private int id = -1;
    /// <summary>
    /// Metodo para gestionar el cambio de caso. Pide la ruta del json y el pdf y los gestiona
    /// </summary>
    public void SwapCase()
    {
        if (!_inProgress)
        {
            _inProgress = true;
            swapCase(onSuccess, onError);
        }
    }

    /// <summary>
    /// Llamado al cambiar con exito el pdf del rag
    /// </summary>
    private void onSuccess()
    {
        _displayText.color = Color.black;
        _pendingMessage = (id == -1 ? "Ninguno" : "caseData_" + id.ToString());
        _displayText.text = _pendingMessage;
        _inProgress = false;
    }

    /// <summary>
    /// Llamado si no consigue cambiarlo con exito
    /// </summary>
    private void onError(string errorText)
    {
        _displayText.color = Color.red;
        _pendingMessage = errorText;
        _displayText.text = _pendingMessage;
        _inProgress = false;
    }

    /// <summary>
    /// Metodo interno para gestionar el cambio de caso. Pide la ruta del json y el pdf y los gestiona
    /// </summary>
    public void swapCase(Action onComplete, Action<string> onError)
    {
        string pathCaseDataJSON = WinFileSelect.Open("Selecciona el fichero caseData .json a usar", null,"json");
        if (pathCaseDataJSON == null || !pathCaseDataJSON.EndsWith(".json"))
        {
            onError("Error, no seleccionado un caseData .json válido");
            return;
        }

        string pathCaseContentPDF = WinFileSelect.Open("Selecciona el fichero caseContent .pdf a usar", null,"pdf");
        if (pathCaseContentPDF == null || !pathCaseContentPDF.EndsWith(".pdf"))
        {
            onError("Error, no seleccionado un caseContent .pdf válido");
            return;
        }


        coroutinePendingMessage();

        try
        {
            string caseDataSerialized = File.ReadAllText(pathCaseDataJSON);
            CaseDataSerializable data = JsonUtility.FromJson<CaseDataSerializable>(caseDataSerialized);

            string filename = System.IO.Path.GetFileNameWithoutExtension(pathCaseContentPDF);
            string idString = filename.Substring(filename.LastIndexOf('_') + 1);
            id = int.Parse(idString);

            LLMSystemAPI.Instance.ChangePDFCase(pathCaseContentPDF, id, onComplete, onError);
        }
        catch (System.Exception e)
        {
            onError($"Error leyendo el fichero case data .json: {e.Message}");
            return;
        }
    }

    /// <summary>
    /// Coroutina para animar la espera de generacion de caso
    /// </summary>
    /// <returns></returns>
    private IEnumerator coroutinePendingMessage()
    {
        float timer = 0;

        _displayText.text = "";
        while (_inProgress)
        {
            timer += UnityEngine.Time.deltaTime;
            _displayText.text = "Cambiando caso...";
            for (int i = 0; i < 3; i++)
                _displayText.text += (i <= (int)(timer % 3)) ? "." : " ";
            yield return null;
        }

        _displayText.text = _pendingMessage;
    }
}
