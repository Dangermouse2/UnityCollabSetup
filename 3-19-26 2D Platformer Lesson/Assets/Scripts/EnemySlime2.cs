using UnityEngine;

public class EnemySlime2 : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject enemyBullet;
    [SerializeField] private Transform bulletPoint;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float fireRate = 0.2f;
    private float burstValue = 0;
    [SerializeField] private int burstMax = 3;
    [SerializeField] private float pauseTime = 0.5f;
    private float nextFireTime;

    [Header("Movement & AI")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float attackRange = 5f;

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
        }
    }

    private void Update()
    {
        if (player == null) return;

        // 1. Calculate horizontal distance only (ignore jumping)
        float distanceToPlayerX = Mathf.Abs(player.position.x - transform.position.x);

        if (distanceToPlayerX <= chaseRange)
        {
            FacePlayer();

            if (distanceToPlayerX > attackRange)
            {
                // 2. Create a target position that keeps the enemy's current Y position
                Vector2 targetPosition = new Vector2(player.position.x, transform.position.y);

                // Move horizontally towards the player
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
            else
            {
                if (Time.time >= nextFireTime)
                {
                    if (burstValue >= burstMax)
                    {
                        burstValue = 0;
                        nextFireTime = Time.time + pauseTime + fireRate;
                    }
                    else
                    {
                        AttackPlayer();
                        nextFireTime = Time.time + fireRate;
                        burstValue++;
                    }
                }
            }
        }
    }

    private void FacePlayer()
    {
        Vector3 currentScale = transform.localScale;

        if (player.position.x > transform.position.x)
        {
            currentScale.x = -Mathf.Abs(currentScale.x);
        }
        else if (player.position.x < transform.position.x)
        {
            currentScale.x = Mathf.Abs(currentScale.x);
        }

        transform.localScale = currentScale;
    }

    private void AttackPlayer()
    {
        GameObject newBullet = Instantiate(enemyBullet, bulletPoint.position, bulletPoint.rotation);

        // 3. Shoot strictly left or right depending on where the player is
        Vector2 shootDirection = (player.position.x > transform.position.x) ? Vector2.right : Vector2.left;

        newBullet.GetComponent<Rigidbody2D>().linearVelocity = shootDirection * bulletSpeed;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}