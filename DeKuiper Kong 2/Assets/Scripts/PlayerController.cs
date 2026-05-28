using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 4f;
    public float climbSpeed = 3f;
    public float jumpForce = 6.5f;

    [Header("Arcade Authenticity")]
    [Tooltip("If true, you cannot change direction or stop moving horizontally once in mid-air.")]
    public bool classicCommitmentJump = true;

    [Header("Environment Detection")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public LayerMask ladderLayer;
    public float checkRadius = 0.15f;

    [Header("Hammer Power-Up")]
    public float hammerDuration = 7f;
    public LayerMask smashableLayer; // Set this to your Barrels/Enemies layer
    public Transform hammerHitBox;   // Empty GameObject in front of player
    public float hammerHitRadius = 0.4f;

    // Internal State Tracking
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isNearLadder;
    private bool isClimbing;
    private bool isJumping;
    private float baseGravity;
    private float lockedJumpXVelocity;
    private Transform activeLadder;
    private bool isFacingRight = true;

    // Hammer State
    private bool isHammering = false;
    private float hammerTimer = 0f;

    // Climb Cooldown to prevent bottom-bouncing
    private float climbCooldownTimer = 0f;
    private float climbCooldownAmount = 0.2f;

    // Animations
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        baseGravity = rb.gravityScale;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // --- ABSOLUTE SAFETY LOCK: If this script is disabled mid-frame, halt execution instantly! ---
        if (!enabled) return;

        // 1. Gather Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // 2. Environment Overlap Checks
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // Center check for regular climbing/wandering off
        Collider2D centerLadderCollider = Physics2D.OverlapCircle(transform.position, checkRadius, ladderLayer);

        // Foot check specifically for standing on top of a ladder trying to climb down
        Collider2D footLadderCollider = Physics2D.OverlapCircle(groundCheck.position, checkRadius, ladderLayer);

        // We are near a ladder if either the center or the feet are touching it
        isNearLadder = (centerLadderCollider != null || footLadderCollider != null);

        if (centerLadderCollider != null) activeLadder = centerLadderCollider.transform;
        else if (footLadderCollider != null) activeLadder = footLadderCollider.transform;

        // Subtract cooldown timer
        if (climbCooldownTimer > 0)
        {
            climbCooldownTimer -= Time.deltaTime;
        }

        // 3. Ladder Climbing Logic
        if (isNearLadder && !isHammering && climbCooldownTimer <= 0)
        {
            // CASE A: Standard Climbing Up/Down while already on the ladder
            if (Mathf.Abs(verticalInput) > 0.1f && !isClimbing)
            {
                // Prevent climbing DOWN if we are standing on the solid ground at the very BOTTOM of a ladder
                if (verticalInput < -0.1f && isGrounded && activeLadder != null && groundCheck.position.y <= activeLadder.position.y)
                {
                    // Do nothing
                }
                else if (verticalInput > 0.1f) // Climbing UP
                {
                    isClimbing = true;
                    rb.gravityScale = 0;
                    playerCollider.isTrigger = true;
                    transform.position = new Vector3(activeLadder.position.x, transform.position.y, transform.position.z);
                }
            }

            // CASE B: Standing at the TOP of the ladder trying to initiate a climb DOWN
            if (verticalInput < -0.1f && !isClimbing && isGrounded && footLadderCollider != null)
            {
                isClimbing = true;
                rb.gravityScale = 0;
                playerCollider.isTrigger = true;

                // Snap to ladder X, and drop the player down slightly past the platform lip instantly
                transform.position = new Vector3(activeLadder.position.x, transform.position.y - 0.2f, transform.position.z);
            }
        }

        // If we wander off a ladder entirely
        if (isClimbing && !isNearLadder)
        {
            ExitClimbing();
        }

        // 4. Hammer Timer Logic
        if (isHammering)
        {
            hammerTimer -= Time.deltaTime;
            if (hammerTimer <= 0) StopHammering();
            else SmashCheck();
        }

        // 5. Sprite Flipping
        if (!isClimbing && horizontalInput != 0)
        {
            FlipSprite(horizontalInput);
        }

        // 6. Animation State Updates
        UpdateAnimations();

        // 7. Jump Input
        if (Input.GetButtonDown("Jump") && isGrounded && !isClimbing && !isHammering)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (classicCommitmentJump)
            {
                lockedJumpXVelocity = horizontalInput * walkSpeed;
            }
        }
    }

    void FixedUpdate()
    {
        // --- ABSOLUTE SAFETY LOCK: If this script is disabled mid-frame, halt physics updates instantly! ---
        if (!enabled) return;

        if (isClimbing)
        {
            rb.linearVelocity = new Vector2(0, verticalInput * climbSpeed);

            // Only exit on ground contact if we are near the bottom half of the active ladder
            if (isGrounded && verticalInput < -0.1f && activeLadder != null && groundCheck.position.y <= activeLadder.position.y)
            {
                ExitClimbing();
            }
        }
        else
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(horizontalInput * walkSpeed, rb.linearVelocity.y);
            }
            else
            {
                if (classicCommitmentJump)
                {
                    rb.linearVelocity = new Vector2(lockedJumpXVelocity, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(horizontalInput * walkSpeed, rb.linearVelocity.y);
                }
            }
        }
    }

    public void CollectHammer()
    {
        if (isClimbing) ExitClimbing();
        isHammering = true;
        hammerTimer = hammerDuration;
    }

    private void StopHammering()
    {
        isHammering = false;
    }

    private void SmashCheck()
    {
        if (hammerHitBox == null) return;

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(hammerHitBox.position, hammerHitRadius, smashableLayer);
        foreach (Collider2D obj in hitObjects)
        {
            Destroy(obj.gameObject);
            Debug.Log("Smashed an obstacle!");
        }
    }

    private void UpdateAnimations()
    {
        animator.SetBool("isHammering", isHammering);
        if (isHammering)
        {
            animator.speed = 1f;
            animator.SetBool("isWalking", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isClimbing", false);
            return;
        }

        animator.SetBool("isClimbing", isClimbing);
        if (isClimbing)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isJumping", false);

            if (Mathf.Abs(verticalInput) > 0.1f) animator.speed = 1f;
            else animator.speed = 0f;
            return;
        }

        animator.speed = 1f;

        if (!isGrounded)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isWalking", false);
        }
        else if (horizontalInput != 0)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isWalking", false);
        }
    }

    private void FlipSprite(float input)
    {
        if ((input > 0 && !isFacingRight) || (input < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    private void ExitClimbing()
    {
        isClimbing = false;
        rb.gravityScale = baseGravity;
        animator.speed = 1f;

        playerCollider.isTrigger = false;
        climbCooldownTimer = climbCooldownAmount;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hammer"))
        {
            CollectHammer();
            Destroy(collision.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (hammerHitBox != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(hammerHitBox.position, hammerHitRadius);
        }
    }
}