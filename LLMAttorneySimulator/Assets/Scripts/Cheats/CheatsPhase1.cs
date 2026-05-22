using TMPro;
using UnityEngine;

/// <summary>
/// Clase para poder llamar a metodos de cheatSystem en la fase 1 en botones
/// </summary>
public class CheatsPhase1 : MonoBehaviour
{
    [SerializeField] private TMP_InputField _forceMoneyInputField;
    public void Phase1ToPhase2()
    {
        CheatsSystem.Instance.Phase1toPhase2(_forceMoneyInputField.text);
    }
}
