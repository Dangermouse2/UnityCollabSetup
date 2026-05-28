using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PatrolingEnemy : MonoBehaviour
{
    
    // 1. Swapped individual point variables for an Array!
    public enum MovementType {standing, walking, flying };
    public enum AnimationType { none, animations, particles };
    [Header("Movement")]
    [SerializeField] private MovementType movementType;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float walkSpeed = 15f;
    private int currentPointIndex = 0; // Tracks which list item we are currently moving towards
    private Rigidbody rb;

    [Header("Targeting & Shooting")]
    [SerializeField] private GameObject player;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f; // Seconds between shots
    [SerializeField] private float bulletSpeed = 20f;
    [Range(0, 1)][SerializeField] private float bulletAccuracy = 0.5f;

    [SerializeField] private int health = 3;

    private bool isDead; //needed to delay death so disappear isn't instant
    
    [SerializeField] private AnimationType animationType = AnimationType.none;
    private Animator animator;
    
    [SerializeField] private ParticleSystem explosion;
    [SerializeField] private ParticleSystem damage;

    private float nextFireTime; // Timer variable

    private void Start()
    {
        if (animationType == AnimationType.animations)
        {
            animator = GetComponentInChildren<Animator>();
        }
        rb = GetComponent<Rigidbody>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if(movementType == MovementType.flying)
        {
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
    }

    private void Update()
    {
        if (isDead) //if turret is dead, don't do anything
        {
            return;
        }
        if (health <= 0)
        {
            isDead = true;
            if (animationType == AnimationType.animations)
            {
                animator.SetBool("Dead", true);
            }
            if (animationType == AnimationType.particles)
            {
                explosion.Play();
            }
            Destroy(gameObject, 3f);
            return;
        }

        if (player == null) return;

        // Check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (animationType == AnimationType.animations)
        {
            animator.SetBool("Walking", true);
            animator.SetBool("Attacking", false);
        }

            // 2. DEFENSIVE CHECK: Safety fallback if a student forgets to assign points in the Inspector
            if (patrolPoints.Length == 0) return;

        // 3. ARRAY ACCESS: Use our index tracker to extract the target transform from our list
        Transform targetPoint = patrolPoints[currentPointIndex];

        // PHYSICS MOVE: Calculate the next step, then tell the Rigidbody to move there
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, targetPoint.position, walkSpeed * Time.deltaTime);
        rb.MovePosition(nextPosition);

        // Face the waypoint (locking Y to prevent leaning)
        if (movementType == MovementType.flying)
        {
            transform.LookAt(targetPoint.position);
        }
        else
        {
            transform.LookAt(new Vector3(targetPoint.position.x, transform.position.y, targetPoint.position.z));
        }

        // Switch waypoints when we get close enough
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            currentPointIndex++; // Move to the next index index slot in our shopping list

            // 4. RESET LOOP: If we run off the end of our array list, loop back around to 0
            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = 0;
            }
        }
    }

    private void AttackPlayer()
    {
        // Look at the player
        if (movementType == MovementType.flying)
        {
            transform.LookAt(player.transform.position);
        }
        else
        {
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
        }

        // STOP VELOCITY: Stops the enemy from sliding due to physics inertia when they stop to shoot
        rb.linearVelocity = Vector3.zero;

        if (animationType == AnimationType.animations)
        {
            animator.SetBool("Walking", false);
            animator.SetBool("Attacking", true);
        }

            // Cooldown timer: Only shoot if enough time has passed
            if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rbBullet = bullet.GetComponent<Rigidbody>();

        if (rbBullet != null)
        {
            // Calculate perfect direction
            Vector3 direction = (player.transform.position - firePoint.position).normalized;

            // 1. TEACHING MOMENT: Turn accuracy into a "Spread" value. 
            // If accuracy is 1, spread is 0 (perfect shot). If accuracy is 0, spread is 1 (wild shot).
            float spread = 1f - bulletAccuracy;

            // 2. Tweak the coordinates slightly using a random offset multiplied by a maximum weight factor (e.g., 0.25f)
            direction.x += Random.Range(-spread, spread) * 0.25f;
            direction.y += Random.Range(-spread, spread) * 0.25f;
            direction.z += Random.Range(-spread, spread) * 0.25f;

            // 3. IMPORTANT: Re-normalize the vector so the bullet doesn't travel faster or slower due to the added numbers!
            direction = direction.normalized;

            rbBullet.linearVelocity = direction * bulletSpeed;
        }

        Destroy(bullet, 3f);
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        if (animationType == AnimationType.particles)
        {
            damage.Play();
        }
    }
}