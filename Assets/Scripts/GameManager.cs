using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void Start()
    {
        Debug.Log("GameManager started");
        Cursor.lockState = CursorLockMode.Confined;
    }
}
