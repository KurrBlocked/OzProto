using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public FinishLine finish;
    public PlayerController stats;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (stats.healthCount == 0)
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "DemoLevel1":
                    SceneManager.LoadScene("DemoLevel1");
                    break;
                case "DemoLevel2":
                    SceneManager.LoadScene("DemoLevel2");
                    break;
                case "DemoLevel3":
                    SceneManager.LoadScene("DemoLevel3");
                    break;
                default:
                    Debug.Log("Unknown level");
                    break;
            }
        }
        if (finish.isOnFinishLine)
        {
            switch (SceneManager.GetActiveScene().name)
            {
                case "DemoLevel1":
                    SceneManager.LoadScene("DemoLevel2");
                    break;
                case "DemoLevel2":
                    SceneManager.LoadScene("DemoLevel3");
                    break;
                case "DemoLevel3":
                    SceneManager.LoadScene("WinScreen");
                    break;
                default:
                    Debug.Log("Unknown level");
                    break;
            }
        }
    }

}
