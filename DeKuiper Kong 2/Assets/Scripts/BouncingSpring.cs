using UnityEngine;

public class BouncingSpring : MonoBehaviour
{
    [Header("Spring Physics")]
    [SerializeField] private float horizontalSpeed = -3f; // Moves left to right (or right to left)
    [SerializeField] private float bounceForce = 6f;
    [SerializeField] private float gravityScale = 1.5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        // Maintain a constant horizontal speed regardless of bouncing friction
        rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Whenever we touch a girder or a platform, trigger a perfect uniform bounce
        if (collision.gameObject.CompareTag("Ground"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
        }

        // Kill player if hit
        if (collision.gameObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameActive())
            {
                GameManager.Instance.PlayerDeath();
            }
        }
    }
}