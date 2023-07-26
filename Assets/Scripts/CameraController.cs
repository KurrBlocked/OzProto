using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    private int currentZone = 0;

    private List<float[]> zone;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3 (0f, 0f, -10f);
        zone = new List<float[]>();
        //zone (leftPlayerBound, rightPlayerBound, upperPlayerBound, LowerPlayerBound, leftCameraBound, rightCameraBound, cameraYLevel)
        switch (SceneManager.GetActiveScene().name)
        {
            case "DemoLevel1":
                zone.Add(new float[] { -14.5f, 47.5f, 9.5f, -7.5f, 0f, 33f, 0f });
                zone.Add(new float[] { -14.5f, 47.5f, 23.5f, 9.5f, 0f, 33f, 16f });
                break;
            case "SampleScene":
                zone.Add(new float[] { -14.5f, 47.5f, 9.5f, -7.5f, 0f, 33f, 0f });
                zone.Add(new float[] { -14.5f, 47.5f, 23.5f, 9.5f, 0f, 33f, 16f });
                break;
            case "DemoLevel2":
                zone.Add(new float[] { -14.5f, 32.5f, 6.5f, -7.5f, 0f, 18f, 0f });
                zone.Add(new float[] { -14.5f, 32.5f, 25f, 6.5f, 0f, 18f, 16f });
                zone.Add(new float[] { -14.5f, 32.5f, 42f, 25f, 0f, 18f, 34f });
                break;
            case "DemoLevel3":
                zone.Add(new float[] { -14.5f, 46.5f, 8.5f, -7.5f, 0f, 32f, 0f });
                zone.Add(new float[] { -14.5f, 58.5f, 28.5f, 9.5f, 0f, 44f, 18f });
                zone.Add(new float[] { 49.5f, 78.5f, 8.5f, -7.5f, 44f, 64f, 0f });
                zone.Add(new float[] { -14.5f, 78.5f, 27f, 9.5f, 0f, 64f, 18f });
                break;
            default:
                Debug.Log("Unknown level");
                break;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //Check if player is in currentZone, if not updates
        if (!checkInZone(zone[currentZone]))
        {
            for (int x = 0; x < zone.Count; x++)
            {
                if (checkInZone(zone[x]))
                {
                    currentZone = x;
                    break;
                }
            }
        }


        transform.position = new Vector3(transform.position.x, zone[currentZone][6], transform.position.z);
        if (player.transform.position.x > zone[currentZone][4] && player.transform.position.x < zone[currentZone][5])
        {
            transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        }



    }

    private bool checkInZone(float[] zone)
    {
        return (player.transform.position.x > zone[0] && player.transform.position.x < zone[1] && player.transform.position.y < zone[2] && player.transform.position.y > zone[3]);
    }
}
