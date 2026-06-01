using UnityEngine;

public class FireballEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float ladderClimbSpeed = 1.5f;

    // --- NEW: EDGE DETECTION VARIABLES ---
    [SerializeField] private LayerMask groundLayer;       // Set this to your "Ground" layer in the inspector
    [SerializeField] private float edgeCheckOffset = 0.1f; // How far ahead of its body to look
    [SerializeField] private float edgeCheckLength = 0.5f; // How far down to look for the floor

    // --- NEW: AI TYPE SWITCH ---
    [Header("AI Intelligence Type")]
    [Tooltip("Unchecked = Random/Dumb Girder AI. Checked = Smart Player-Tracking Rivet AI.")]
    [SerializeField] private bool isSmartRivetAI = false;

    [Header("Dumb AI Logic (Girder Stage Only)")]
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

    // --- NEW: Player Tracking Reference ---
    private Transform playerTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Automatically find Jumpman in the hierarchy via his tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

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

            // --- SMART AI INTERVAL PURSUIT ---
            if (isSmartRivetAI)
            {
                if (!isClimbing && playerTransform != null)
                {
                    // If the player is sharing the same platform deck level, track their X coordinate perfectly
                    float yDiff = Mathf.Abs(playerTransform.position.y - transform.position.y);
                    if (yDiff < 1.5f)
                    {
                        int desiredDir = (playerTransform.position.x > transform.position.x) ? 1 : -1;
                        SetMovingDirection(desiredDir);
                    }
                }
                // Smart AI doesn't use random ladder reverse fake-outs; it moves strictly floor to floor!
            }
            // --- DUMB AI TRADITIONAL RANDOM RANDOMIZATION ---
            else
            {
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
    }

    private void HandleWandering()
    {
        // --- FIX: EDGE DETECTION ---
        // Calculate a point slightly in front of the fireball based on its current moving direction
        Vector2 checkOrigin = new Vector2(transform.position.x + (moveDirection * edgeCheckOffset), transform.position.y);

        // Shoot a tiny raycast straight down to see if solid ground is beneath that forward point
        RaycastHit2D hitGround = Physics2D.Raycast(checkOrigin, Vector2.down, edgeCheckLength, groundLayer);

        // DEBUG VISUAL: Draw a green line in the Unity Editor Scene view so you can see the detector working!
        Debug.DrawRay(checkOrigin, Vector2.down * edgeCheckLength, hitGround.collider != null ? Color.green : Color.red);

        // If the raycast returns null, it means we are looking at an empty abyss or a popped rivet gap!
        if (hitGround.collider == null)
        {
            FlipDirection();
            return; // Exit out of this frame early so it doesn't walk into the gap
        }

        // If ground is safe ahead, continue walking normally
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
            if (collision.CompareTag("LadderTop"))
            {
                if (isSmartRivetAI)
                {
                    // Smart Check: Is Jumpman below us? If yes, take the ladder down!
                    if (playerTransform != null && playerTransform.position.y < transform.position.y - 0.5f)
                    {
                        StartClimbing(collision.transform.position.x, -1);
                    }
                }
                else if (Random.value < climbChance)
                {
                    StartClimbing(collision.transform.position.x, -1);
                }
            }
            else if (collision.CompareTag("LadderBottom"))
            {
                if (isSmartRivetAI)
                {
                    // Smart Check: Is Jumpman above us? If yes, run up the ladder!
                    if (playerTransform != null && playerTransform.position.y > transform.position.y + 0.5f)
                    {
                        StartClimbing(collision.transform.position.x, 1);
                    }
                }
                else if (Random.value < climbChance)
                {
                    StartClimbing(collision.transform.position.x, 1);
                }
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

        // --- FIX: Turn into a ghost trigger so we can slip through the floor layout ---
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void FinishClimbing()
    {
        isClimbing = false;
        climbDirection = 0;
        rb.gravityScale = 1f;

        // --- FIX: Turn solid again so we land safely on the platform decks ---
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = false;

        if (isSmartRivetAI && playerTransform != null)
        {
            int desiredDir = (playerTransform.position.x > transform.position.x) ? 1 : -1;
            SetMovingDirection(desiredDir);
        }
        else
        {
            FlipDirection();
        }
    }

    // --- NEW: Absolute movement assignment to clean up sprite handling ---
    private void SetMovingDirection(int direction)
    {
        moveDirection = direction;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * moveDirection;
        transform.localScale = localScale;
    }

    private void FlipDirection()
    {
        SetMovingDirection(moveDirection * -1);
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