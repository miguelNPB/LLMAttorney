using UnityEngine;

/// <summary>
/// Interfaz para las paginas del sistema de ordenador de la fase 2
/// </summary>
public abstract class IPage : MonoBehaviour
{
    public ComputerSystem _computerSystem;

    /// <summary>
    /// Llamado al abrir la pagina
    /// </summary>
    abstract public void Open();

    /// <summary>
    /// Llamado al cerrar la pagina
    /// </summary>
    abstract public void Close();
}
