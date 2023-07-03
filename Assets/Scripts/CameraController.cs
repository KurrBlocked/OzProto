using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float leftBound;
    public float rightBound;
    public float upperBound;
    public float lowerBound;
    // Start is called before the first frame update
    void Start()
    {
        leftBound = 0f;
        rightBound = 40f;
        upperBound = 0f;
        lowerBound = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (player.transform.position.x > leftBound && player.transform.position.x < rightBound)
        {
            transform.position = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
        }

        if (player.transform.position.y > lowerBound && player.transform.position.y < upperBound)
        {
            transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
        }

    }
}
