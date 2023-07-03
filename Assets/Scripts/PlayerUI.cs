using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public PlayerController player;
    public GameObject healthTokenPrefab;
    public Canvas canvas;

    private GameObject[] healthTokens;
    private int localHealthTracker;

    void Start()
    {
        localHealthTracker = player.healthCount;
        healthTokens = new GameObject[player.healthCount];
        for (int x = 0; x < player.totalHealth; x++)
        {
            healthTokens[x] = Instantiate(healthTokenPrefab, new Vector3 (canvas.renderingDisplaySize.x /8  + x * canvas.renderingDisplaySize.x / 23, canvas.renderingDisplaySize.y / 1.1f, 0), Quaternion.identity, transform);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        textMeshPro.text = "" + player.bouncesRemaining;

        if (localHealthTracker != player.healthCount)
        {
            foreach (GameObject token in healthTokens)
            {
                token.SetActive(false);
            }
            for (int x = 0; x < player.healthCount; x++)
            {
                healthTokens[x].SetActive(true);
            }
        }
    }
}
