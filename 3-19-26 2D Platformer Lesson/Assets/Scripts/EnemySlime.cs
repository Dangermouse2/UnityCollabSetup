using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private Transform bulletPoint;
    [SerializeField] private float bulletSpeed = 5f; // Renamed from 'speed' for clarity
    [SerializeField] private float fireRate = 0.2f;
    private float nextFireTime;

    [Header("Movement & AI")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 8f;   // When the enemy starts following
    [SerializeField] private float attackRange = 5f;  // When the enemy stops to shoot

    private Transform player;
    private SpriteRenderer spriteImage;

    private void Start()
    {
        spriteImage = GetComponentInChildren<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            InvokeRepeating(nameof(ToggleColor), 0f, .1f);
            Invoke(nameof(SlimeDeath), 1.5f);
            Destroy(collision.transform);
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if the player is close enough to be noticed
        if (distanceToPlayer <= chaseRange)
        {
            // Face the player as long as they are in the chase range
            FacePlayer();

            // If the player is outside attack range, run toward them
            if (distanceToPlayer > attackRange)
            {
                // MoveTowards takes (Current Position, Target Position, Speed per frame)
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
            // If the player is inside the attack range, stop moving and shoot
            else
            {
                if (Time.time >= nextFireTime)
                {
                    AttackPlayer();
                    nextFireTime = Time.time + fireRate;
                }
            }
        }
    }

    private void FacePlayer()
    {
        // Grab the enemy's exact current scale (keeps your custom size intact!)
        Vector3 currentScale = transform.localScale;

        if (player.position.x > transform.position.x)
        {
            // Player is to the right. 
            // We use -Mathf.Abs to guarantee the X scale becomes negative.
            currentScale.x = -Mathf.Abs(currentScale.x);
        }
        else if (player.position.x < transform.position.x)
        {
            // Player is to the left. 
            // We use Mathf.Abs to guarantee the X scale becomes positive.
            currentScale.x = Mathf.Abs(currentScale.x);
        }

        // Apply the newly flipped scale back to the enemy
        transform.localScale = currentScale;
    }

    private void AttackPlayer()
    {
        GameObject newBullet = Instantiate(enemyBullet, bulletPoint.position, bulletPoint.rotation);

        Vector2 direction = (player.position - bulletPoint.position).normalized;

        newBullet.GetComponent<Rigidbody2D>().linearVelocity = direction * bulletSpeed;
        Destroy(newBullet, 2f);
    }

    private void ToggleColor()
    {
        if (spriteImage.color != Color.red)
            spriteImage.color = Color.red;
        else
            spriteImage.color = Color.white;
    }

    private void SlimeDeath()
    {
        CancelInvoke(nameof(ToggleColor));
        spriteImage.color = Color.white;
        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the Chase Range in Yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Draw the Attack Range in Red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}