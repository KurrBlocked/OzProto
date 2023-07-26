using UnityEngine;

public class TurretController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject player;
    public PlayerController playerStats;
    public float fireRate = 1f;
    public float fireRange = 5f;

    private float fireTimer;

    private void Awake()
    {
        player = GameObject.Find("Player");
    }


    private void Update()
    {
        fireTimer += Time.deltaTime;

        // Check if it's time to fire
        if (fireTimer >= fireRate)
        {
            if (IsPlayerInRange())
            {
                Fire();
            }
            
            fireTimer = 0f; // Reset the timer
        }
        
    }

    private void Fire()
    {
        Vector2 direction = player.transform.position - transform.position;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Bullet>().SetTarget(direction);
    }

    private bool IsPlayerInRange()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= fireRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (playerStats.isBouncing)
            {
                Destroy(gameObject);
            }
        }
    }
}