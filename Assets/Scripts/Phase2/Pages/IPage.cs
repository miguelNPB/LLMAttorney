using UnityEngine;

/// <summary>
/// Interfaz para las paginas del sistema de ordenador de la fase 2
/// </summary>
public abstract class IPage : MonoBehaviour
{
    public ComputerSystem computerSystem;
    abstract public void Open();
    abstract public void Close();
}
