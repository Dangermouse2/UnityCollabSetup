using System.Collections;
using UnityEngine;

public class MasterBarrel : MonoBehaviour
{
    public enum BarrelType { Standard, Wild, Blue }

    [Header("Barrel Settings")]
    [SerializeField] private BarrelType type = BarrelType.Standard;
    [SerializeField] private float baseRollForce = 4f;
    [SerializeField] private float maxHorizontalSpeed = 5f;
    [SerializeField] private float ladderDropSpeed = 3f; // Controlled downward speed

    [Header("Ladder Drop Logic (Standard/Blue)")]
    [Range(0f, 1f)][SerializeField] private float baseLadderDropChance = 0.2f;
    [Range(0f, 1f)][SerializeField] private float steeredLadderDropChance = 0.65f;

    [Header("Blue Barrel Settings")]
    [SerializeField] private GameObject fireballPrefab; // Drag your Fireball Prefab here in the inspector

    [Header("Wild Barrel Settings")]
    [SerializeField] private float wildDownwardSpeed = 6f;
    [SerializeField] private float wildRotationSpeed = 500f; // Chaotic spin speed

    private Rigidbody2D rb;
    [Header("Spawn Settings")]
    [SerializeField] private int rollDirection = 1; // 1 = Right, -1 = Left
    private bool isDroppingDownLadder = false;
    private Transform targetPlayer;
    private Coroutine ghostCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) targetPlayer = playerObj.transform;

        // --- FIX: Setup Wild Barrels to be absolute ghosts to floors ---
        if (type == BarrelType.Wild)
        {
            // Turn it into a trigger permanently so it passes right through girders!
            GetComponent<CircleCollider2D>().isTrigger = true;

            // Turn off gravity because we will manually drive its terrifying descent
            rb.gravityScale = 0f;

            // Pick a random chaotic starting horizontal push
            float randomForwardSpeed = Random.Range(2f, maxHorizontalSpeed);
            rb.linearVelocity = new Vector2(rollDirection * randomForwardSpeed, -wildDownwardSpeed);
        }
        else
        {
            // Normal and Blue barrels get their steady forward injection
            rb.linearVelocity = new Vector2(rollDirection * maxHorizontalSpeed * 0.6f, rb.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {
        // 1. FIX: Handle Visual Rotation based on Type
        if (type == BarrelType.Wild)
        {
            // Spin constantly and wildly regardless of direction!
            transform.Rotate(0, 0, -wildRotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Normal barrels roll naturally based on physical speed
            transform.Rotate(0, 0, -rb.linearVelocity.x * 3f);
        }

        // 2. Drive Movement Logic based on Type
        switch (type)
        {
            case BarrelType.Standard:
            case BarrelType.Blue:
                HandleRollingMovement();
                break;

            case BarrelType.Wild:
                HandleWildMovement();
                break;
        }
    }

    private void HandleRollingMovement()
    {
        if (isDroppingDownLadder)
        {
            rb.linearVelocity = new Vector2(0f, -ladderDropSpeed);
            return;
        }

        if (Mathf.Abs(rb.linearVelocity.x) < maxHorizontalSpeed)
        {
            rb.AddForce(new Vector2(rollDirection * baseRollForce, 0f), ForceMode2D.Force);
        }
    }

    private void HandleWildMovement()
    {
        // FIX: Maintain a strict, terrifying downward speed, but preserve horizontal bounces
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wildDownwardSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Note: Wild barrels ignore this completely now because they are Triggers!

        // Bouncing off boundary walls flips direction
        if (collision.gameObject.CompareTag("Wall"))
        {
            rollDirection *= -1;
            rb.linearVelocity = new Vector2(rollDirection * maxHorizontalSpeed * 0.6f, rb.linearVelocity.y);
        }

        // Landed on solid ground
        if (collision.gameObject.CompareTag("Ground") && isDroppingDownLadder)
        {
            LandedOnFloor();
        }

        // Solid hit with player
        if (collision.gameObject.CompareTag("Player"))
        {
            HitPlayer(collision.gameObject);
        }

        // Blue Barrel hits the Oil Drum
        if (type == BarrelType.Blue && collision.gameObject.CompareTag("OilDrum"))
        {
            InstantiateFireballEnemy(collision.transform.position);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("KillZone"))
        {
            Destroy(gameObject);
        }

        // FIX: If a Wild barrel hits a outer boundary wall, bounce it back trigger-style!
        if (collision.CompareTag("Wall") && type == BarrelType.Wild)
        {
            rollDirection *= -1;
            rb.linearVelocity = new Vector2(rollDirection * maxHorizontalSpeed * 0.8f, rb.linearVelocity.y);
        }

        // Classic Arcade Ladder Drop Calculation (Only for normal rolling types)
        if ((type == BarrelType.Standard || type == BarrelType.Blue) && collision.CompareTag("LadderTop") && !isDroppingDownLadder)
        {
            CalculateLadderDrop(collision.transform.position.x);
        }

        // Safety Net for normal barrels landing
        if (collision.CompareTag("Ground") && isDroppingDownLadder)
        {
            LandedOnFloor();
        }

        // Trigger overlap hit with player (Works for ALL barrels now)
        if (collision.CompareTag("Player"))
        {
            HitPlayer(collision.gameObject);
        }
    }

    private void CalculateLadderDrop(float ladderXSource)
    {
        float finalDropChance = baseLadderDropChance;

        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            finalDropChance = steeredLadderDropChance;
        }

        if (Random.value < finalDropChance)
        {
            isDroppingDownLadder = true;
            transform.position = new Vector3(ladderXSource, transform.position.y, transform.position.z);
            rb.linearVelocity = new Vector2(0f, -ladderDropSpeed);
            ghostCoroutine = StartCoroutine(GhostThroughFloorRoutine());
        }
    }

    private IEnumerator GhostThroughFloorRoutine()
    {
        CircleCollider2D myCollider = GetComponent<CircleCollider2D>();
        if (myCollider != null)
        {
            myCollider.isTrigger = true;
            yield return new WaitForSeconds(1f / ladderDropSpeed);
            myCollider.isTrigger = false;
        }
    }

    private void LandedOnFloor()
    {
        if (ghostCoroutine != null) StopCoroutine(ghostCoroutine);

        isDroppingDownLadder = false;
        GetComponent<CircleCollider2D>().isTrigger = false;
        rollDirection *= -1;
        rb.linearVelocity = new Vector2(rollDirection * maxHorizontalSpeed * 0.8f, 0f);
    }

    private void HitPlayer(GameObject playerObject)
    {
        PlayerController player = playerObject.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("Hazard destroyed the player!");

            // Notify the game manager instantly
            GameManager.Instance.PlayerDeath();

            // Play player death animation or reset player position here
        }
    }



    private void InstantiateFireballEnemy(Vector3 spawnPos)
    {
        Debug.Log("Blue barrel ignited the oil drum! Spawning Fireball Enemy!");
        if (fireballPrefab != null)
        {
            // Spawn the flame right at the location of the oil drum
            Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        }
    }
}