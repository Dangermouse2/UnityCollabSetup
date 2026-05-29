using UnityEngine;

public class FireballEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float ladderClimbSpeed = 1.5f;

    [Header("AI Logic")]
    [Range(0f, 1f)][SerializeField] private float climbChance = 0.35f; // Chance to take a ladder
    [Range(0f, 1f)][SerializeField] private float midWalkFlipChance = 0.15f; // 15% chance to randomly turn around
    [Range(0f, 1f)][SerializeField] private float ladderReverseChance = 0.10f; // 10% chance to fake-out on a ladder
    [SerializeField] private float aiDecisionInterval = 2f; // How often (in seconds) it makes a random choice

    private Rigidbody2D rb;
    private Animator anim;
    private int moveDirection = 1; // 1 = Right, -1 = Left

    private bool isClimbing = false;
    private float targetLadderX;
    private int climbDirection = 0; // 1 = Up, -1 = Down, 0 = None
    private float aiTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Stagger the initial timer so multiple fireballs don't make decisions on the exact same frame
        aiTimer = Random.Range(0f, aiDecisionInterval);
    }

    void FixedUpdate()
    {
        // --- ARCADE CINEMATIC FREEZE: Lock input if the game introduction is playing ---
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive()) return;

        HandleIntervalAI();

        if (isClimbing)
        {
            HandleClimbing();
        }
        else
        {
            HandleWandering();
        }

        UpdateAnimator();
    }

    private void HandleIntervalAI()
    {
        aiTimer += Time.fixedDeltaTime;
        if (aiTimer >= aiDecisionInterval)
        {
            aiTimer = 0f; // Reset timer

            // Choice A: If wandering on the ground, roll to randomly change direction
            if (!isClimbing && Random.value < midWalkFlipChance)
            {
                FlipDirection();
            }
            // Choice B: If currently on a ladder, roll to randomly reverse direction (The Fake-Out!)
            else if (isClimbing && Random.value < ladderReverseChance)
            {
                climbDirection *= -1; // Invert climb (Up becomes Down, Down becomes Up)
                Debug.Log("Fireball changed its mind on the ladder!");
            }
        }
    }

    private void HandleWandering()
    {
        rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);
    }

    private void HandleClimbing()
    {
        transform.position = new Vector3(targetLadderX, transform.position.y, transform.position.z);
        rb.linearVelocity = new Vector2(0f, climbDirection * ladderClimbSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            FlipDirection();
        }

        // If climbing down and we hit the ground, we finished the descent
        if (collision.gameObject.CompareTag("Ground") && isClimbing && climbDirection == -1)
        {
            FinishClimbing();
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            HitPlayer(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("KillZone"))
        {
            Destroy(gameObject);
        }

        if (collision.CompareTag("Player"))
        {
            HitPlayer(collision.gameObject);
        }

        // --- AI LADDER DECISION MAKING ---
        if (!isClimbing)
        {
            if (collision.CompareTag("LadderTop") && Random.value < climbChance)
            {
                StartClimbing(collision.transform.position.x, -1);
            }
            else if (collision.CompareTag("LadderBottom") && Random.value < climbChance)
            {
                StartClimbing(collision.transform.position.x, 1);
            }
        }
        else
        {
            // If climbing UP, stop when hitting the top trigger
            if (collision.CompareTag("LadderTop") && climbDirection == 1)
            {
                FinishClimbing();
                transform.position += new Vector3(0f, 0.2f, 0f); // Nudge onto platform
            }
            // SAFETY NET: If the fireball reversed down and hits the bottom trigger, stop climbing
            else if (collision.CompareTag("LadderBottom") && climbDirection == -1)
            {
                FinishClimbing();
            }
        }
    }

    private void StartClimbing(float ladderX, int direction)
    {
        isClimbing = true;
        climbDirection = direction;
        targetLadderX = ladderX;
        rb.gravityScale = 0f;
    }

    private void FinishClimbing()
    {
        isClimbing = false;
        climbDirection = 0;
        rb.gravityScale = 1f;
        FlipDirection();
    }

    private void FlipDirection()
    {
        moveDirection *= -1;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * moveDirection;
        transform.localScale = localScale;
    }

    private void UpdateAnimator()
    {
        if (anim == null) return;
        anim.SetBool("IsClimbing", isClimbing);
        anim.SetFloat("HorizontalSpeed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("VerticalSpeed", rb.linearVelocity.y);
    }

    private void HitPlayer(GameObject playerObject)
    {
        PlayerController player = playerObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // --- ARCADE PROTECTION: Don't kill the player if they are hammering! ---
            if (player.IsHammering) return;

            Debug.Log("Hazard destroyed the player!");

            // Notify the game manager instantly
            GameManager.Instance.PlayerDeath();
        }
    }
}