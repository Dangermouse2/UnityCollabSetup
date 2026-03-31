using System.Collections;
using UnityEngine;

public class InigoMontoya : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int health = 10;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 15f; // You will need to tweak this to reach the platform!

    [Header("Shooting")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float waitBeforeShooting = 0.5f;
    [SerializeField] private float bulletSpeed = 10f;

    [Header("Movement Limits")]
    // Instead of exact waypoints, we just tell the boss how far left/right to run
    [SerializeField] private float rightSideX = 8f;
    [SerializeField] private float leftSideX = -8f;
    private float startPointX;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isInvincible = false;
    private bool isDead = false;

    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        startPointX = transform.position.x;

      //  StartCoroutine(BossPattern());
    }

    // ADD THIS NEW PUBLIC METHOD!
    // Because it is 'public', other scripts are allowed to press this "button"
    public void StartBossFight()
    {
        StartCoroutine(BossPattern());
    }

    private IEnumerator BossPattern()
    {
        while (isDead == false)
        {
            // --- RIGHT SIDE SEQUENCE ---
            // 1. Run to the right side
            yield return MoveToXPosition(startPointX + rightSideX);

            // 2. Stop, face left, and shoot
            FlipToFaceLeft(true);
            yield return new WaitForSeconds(waitBeforeShooting);
            Shoot();
            yield return new WaitForSeconds(waitBeforeShooting);

            // 3. Jump straight up onto the right platform
            Jump();
            // Wait enough time for the jump to finish before shooting again
            // (You might need to adjust this 1.5s based on your gravity/jumpForce)
            yield return new WaitForSeconds(1.5f);

            // 4. Shoot from the platform
            Shoot();
            yield return new WaitForSeconds(waitBeforeShooting);

            // --- LEFT SIDE SEQUENCE ---
            // 5. Run to the left side (Gravity naturally pulls them down when they run off the edge!)
            yield return MoveToXPosition(startPointX + leftSideX);

            // 6. Stop, face right, and shoot
            FlipToFaceLeft(false);
            yield return new WaitForSeconds(waitBeforeShooting);
            Shoot();
            yield return new WaitForSeconds(waitBeforeShooting);

            // 7. Jump straight up onto the left platform
            Jump();
            yield return new WaitForSeconds(1.5f);

            // 8. Shoot from the platform
            Shoot();
            yield return new WaitForSeconds(waitBeforeShooting);

            // Loops back to the start!
        }
    }

    // This handles our physics-based horizontal movement
    private IEnumerator MoveToXPosition(float targetX)
    {
        // Figure out if we need to move left (-1) or right (1)
        float direction = Mathf.Sign(targetX - transform.position.x);

        // Face the way we are running
        FlipToFaceLeft(direction < 0);

        // While the distance between our current X and target X is greater than 0.2 units...
        while (Mathf.Abs(transform.position.x - targetX) > 0.2f && isDead == false)
        {
            // Apply horizontal velocity, but KEEP the current vertical velocity (so gravity works)
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            animator.SetFloat("xMove", Mathf.Abs(rb.linearVelocityX));
            animator.SetFloat("yMove", rb.linearVelocityY);

            // Wait for the next physics update (Important: Use FixedUpdate for Rigidbody stuff!)
            yield return new WaitForFixedUpdate();
        }

        // We reached the spot! Stop moving horizontally.
        if (isDead == false)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private void Jump()
    {
        if (isDead) return;

        // Stop any weird leftover momentum before jumping
        rb.linearVelocity = new Vector2(0f, 0f);
        animator.SetFloat("yMove", 1f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
    }

    private void Shoot()
    {
        if (isDead) return;
        animator.SetTrigger("Attack");
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // If you set up the enum earlier, you can assign it here!
        // bullet.GetComponent<Projectile>().myType = Projectile.ProjectileType.Enemy;

        bullet.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(transform.localScale.x * bulletSpeed, 0f);
    }

    private void FlipToFaceLeft(bool faceLeft)
    {
        animator.SetFloat("xMove", 0f);
        float absoluteX = Mathf.Abs(transform.localScale.x);
        if (faceLeft)
        {
            transform.localScale = new Vector3(-absoluteX, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(absoluteX, transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile") && isInvincible == false && isDead == false)
        {
            // TODO: Add your ProjectileType enum check here if you want!

            TakeDamage();
            Destroy(collision.gameObject);
        }
    }

    private void TakeDamage()
    {
        health--;

        if (health <= 0)
        {
            isDead = true;
            StopAllCoroutines(); // Stops the movement loop immediately
            rb.linearVelocity = Vector2.zero; // Stops sliding

            Destroy(gameObject, 0.5f);
        }
        else
        {
            StartCoroutine(DamageFlash());
        }
    }

    private IEnumerator DamageFlash()
    {
        isInvincible = true;
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        isInvincible = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, new Vector3(rightSideX, 0f));
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, new Vector3(leftSideX, 0f));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Ouch!");
            //colision object take damage function or line of code
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.gameObject.GetComponent<Collider2D>(), true);
            Invoke(nameof(TempCollision), 2f);
        }
    }

    private void TempCollision()
    {
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<Collider2D>(), false);
    }
}