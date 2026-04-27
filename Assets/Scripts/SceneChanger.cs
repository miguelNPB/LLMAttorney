using UnityEngine;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] 
    private int sceneNumber;

    void Awake()
    {
        Button but = GetComponent<Button>();
        if (but != null)
            but.onClick.AddListener(OnClick);
    }

    void OnClick()
    {

        if(sceneNumber == -1)
        {
            Application.Quit();
        }

        switch (sceneNumber)
        {
            case 0:
                SceneSystem.Instance.LoadMainMenu();
                break;
            case 1:
                SceneSystem.Instance.LoadPhase1();
                break;
            case 2:
                SceneSystem.Instance.LoadPhase2();
                break;
            case 3:
                SceneSystem.Instance.LoadPhase3();
                break;
            case 4:
                SceneSystem.Instance.LoadPhase4();
                break;
            case 5:
                SceneSystem.Instance.LoadGameOver();
                break;
            default:
                Debug.LogError("Scene number not recognized: " + sceneNumber);
                break;
        }
    }


}
