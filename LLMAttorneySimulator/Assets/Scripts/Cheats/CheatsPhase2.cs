using UnityEngine;

/// <summary>
/// Clase para poder llamar a metodos de cheatSystem en la fase 2 en botones
/// </summary>
public class CheatsPhase2 : MonoBehaviour
{
    public void CallGeneratePremadeDocs()
    {
        CheatsSystem.Instance.UsePremadeDocuments();
    }
}
