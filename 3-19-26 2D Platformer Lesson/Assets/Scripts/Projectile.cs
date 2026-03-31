using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 2f; // Destroys bullet after 2 seconds to prevent lag

    private float direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Destroy the bullet after 'lifetime' seconds so they don't fly forever and crash your game
        Destroy(gameObject, lifetime);
    }

    // The PlayerShooter script calls this immediately after spawning the bullet
    public void Setup(float facingDirection)
    {
        direction = facingDirection;

        // If shooting left, flip the bullet's sprite so it doesn't fly backward
        if (direction < 0)
        {
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    private void FixedUpdate()
    {
        // Move the bullet horizontally at a constant speed
        rb.linearVelocity = new Vector2(speed * direction, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore the player so we don't shoot ourselves!
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        // TODO: Here is where you will eventually check if you hit an enemy and deal damage!

        // Destroy the bullet when it hits anything else (like a wall or enemy)
        Destroy(gameObject);
    }
}