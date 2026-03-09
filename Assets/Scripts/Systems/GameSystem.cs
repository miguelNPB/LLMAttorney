using UnityEngine;

public class GameSystem : MonoBehaviour
{



    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static GameSystem Instance { get { return instance; } }
    private static GameSystem instance = null;
}
