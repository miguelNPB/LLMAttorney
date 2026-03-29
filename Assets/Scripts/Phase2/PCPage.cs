using UnityEngine;

// De esta clase deben heredar todas las pestañas del pc de stage2
public abstract class PCPage : MonoBehaviour
{
    public ComputerSystem computerSystem;
    abstract public void Open();
    abstract public void Close();
}
