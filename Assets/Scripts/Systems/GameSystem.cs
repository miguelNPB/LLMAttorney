using JetBrains.Annotations;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public CaseData caseData;


    private void Awake()
    {
        if (GameSystem.Instance != null)
            Destroy(this);

        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public static GameSystem Instance { get { return instance; } }
    private static GameSystem instance = null;
}
