using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float jumpForce = 6.5f;

    [Header("Arcade Authenticity")]
    [Tooltip("If true, you cannot change direction or stop moving horizontally once in mid-air.")]
    [SerializeField] private bool classicCommitmentJump = true;

    [Header("Environment Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private float checkRadius = 0.15f;

    [Header("Hammer Power-Up")]
    [SerializeField] private float hammerDuration = 7f;
    [SerializeField] private LayerMask smashableLayer;
    [SerializeField] private Transform hammerHitBox;
    [SerializeField] private float hammerHitRadius = 0.4f;
    [SerializeField] private GameObject scorePrefab;

    // Internal State Tracking
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private float horizontalInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isNearLadder;
    private bool isClimbing;
    private float baseGravity;
    private float lockedJumpXVelocity;
    private Transform activeLadder;
    private bool isFacingRight = true;

    // Hammer State
    private bool isHammering = false;
    private float hammerTimer = 0f;
    public bool IsHammering => isHammering; // Public shortcut for hazard safety gates

    [Header("Sound Effects")]
    [SerializeField] private AudioClip smashSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip barrelJumpSound; // Sound played when successfully jumping over a hazard
    private AudioSource audioSource;

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

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // --- ARCADE CINEMATIC FREEZE: Lock input if the game introduction is playing ---
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive()) return;

        // 1. Gather Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // 2. Environment Overlap Checks
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        Collider2D centerLadderCollider = Physics2D.OverlapCircle(transform.position, checkRadius, ladderLayer);
        Collider2D footLadderCollider = Physics2D.OverlapCircle(groundCheck.position, checkRadius, ladderLayer);

        isNearLadder = (centerLadderCollider != null || footLadderCollider != null);

        if (centerLadderCollider != null) activeLadder = centerLadderCollider.transform;
        else if (footLadderCollider != null) activeLadder = footLadderCollider.transform;

        if (climbCooldownTimer > 0)
        {
            climbCooldownTimer -= Time.deltaTime;
        }

        // 3. Ladder Climbing Logic
        if (isNearLadder && !isHammering && climbCooldownTimer <= 0)
        {
            if (Mathf.Abs(verticalInput) > 0.1f && !isClimbing)
            {
                if (verticalInput < -0.1f && isGrounded && activeLadder != null && groundCheck.position.y <= activeLadder.position.y)
                {
                    // Do nothing
                }
                else if (verticalInput > 0.1f && centerLadderCollider != null)
                {
                    isClimbing = true;
                    rb.gravityScale = 0;
                    playerCollider.isTrigger = true;
                    transform.position = new Vector3(activeLadder.position.x, transform.position.y, transform.position.z);
                }
            }

            if (verticalInput < -0.1f && !isClimbing && isGrounded && footLadderCollider != null)
            {
                isClimbing = true;
                rb.gravityScale = 0;
                playerCollider.isTrigger = true;
                transform.position = new Vector3(activeLadder.position.x, transform.position.y - 0.2f, transform.position.z);
            }
        }

        if (isClimbing && !isNearLadder)
        {
            ExitClimbing();
        }

        // 4. Hammer Timer Logic
        if (isHammering)
        {
            hammerTimer -= Time.deltaTime;
            if (hammerTimer <= 0) StopHammering();
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

            if (audioSource != null && jumpSound != null)
            {
                audioSource.PlayOneShot(jumpSound);
            }

            if (classicCommitmentJump)
            {
                lockedJumpXVelocity = horizontalInput * walkSpeed;
            }
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive())
        {
            // Force physics to a complete stop so the player doesn't slide during the intro
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (isClimbing)
        {
            rb.linearVelocity = new Vector2(0, verticalInput * climbSpeed);

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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayHammerTheme();
        }
    }

    private void StopHammering()
    {
        isHammering = false;

        if (GameManager.Instance != null && GameManager.Instance.IsGameActive())
        {
            GameManager.Instance.PlayMainTheme();
        }
    }

    public void SmashCheck()
    {
        if (hammerHitBox == null) return;

        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(hammerHitBox.position, hammerHitRadius);
        bool hitSomething = false;

        foreach (Collider2D obj in hitObjects)
        {
            GameObject targetToDestroy = null;

            // Checked both Enemy dynamically
            if (obj.CompareTag("Enemy"))
            {
                targetToDestroy = obj.gameObject;
            }
            else if (obj.transform.parent != null &&
                    obj.transform.parent.CompareTag("Enemy"))
            {
                targetToDestroy = obj.transform.parent.gameObject;
            }

            if (targetToDestroy == null) continue;

            hitSomething = true;

            if (scorePrefab != null)
            {
                Instantiate(scorePrefab, targetToDestroy.transform.position, Quaternion.identity);
            }

            targetToDestroy.SetActive(false);
            Destroy(targetToDestroy);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(300);
            }
        }

        if (hitSomething)
        {
            if (audioSource != null && smashSound != null)
            {
                audioSource.PlayOneShot(smashSound);
            }

            StartCoroutine(FreezeFrame(0.12f));
        }
    }

    private System.Collections.IEnumerator FreezeFrame(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    // --- CALL THIS METHOD FROM YOUR GAMEMANAGER / HAZARD SCRIPT TO KILL THE PLAYER ---
    public void Die()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Halt physics completely
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Update animations to stop walking/jumping states
        animator.SetBool("isWalking", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isClimbing", false);

        // Safely shut down this controller loop
        enabled = false;
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

        // --- RETRO REWARD DETECTOR ---
        if (collision.CompareTag("ScoreZone"))
        {
            // Close down the zone immediately to prevent double scoring
            collision.gameObject.SetActive(false);

            if (audioSource != null && barrelJumpSound != null)
            {
                audioSource.PlayOneShot(barrelJumpSound);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(100);
            }

            Debug.Log("Jumped over hazard! +100 Points");
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