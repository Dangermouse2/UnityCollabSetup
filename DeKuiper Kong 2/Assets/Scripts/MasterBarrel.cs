using System.Collections;
using UnityEngine;

public class MasterBarrel : MonoBehaviour
{
    public enum BarrelType { Standard, Wild, Blue, Cinematic }

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

        if (type == BarrelType.Wild)
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
            rb.gravityScale = 0f;

            float randomForwardSpeed = Random.Range(-2f, maxHorizontalSpeed);
            rb.linearVelocity = new Vector2(rollDirection * randomForwardSpeed, -wildDownwardSpeed);
        }
        else if (type == BarrelType.Cinematic)
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
            rb.gravityScale = 0f;

            GameObject oil = GameObject.FindGameObjectWithTag("OilDrum");
            if (oil != null)
            {
                // --- FIX 1: Correct Direction Math (Destination - Origin) ---
                Vector2 directionToDrum = (oil.transform.position - transform.position).normalized;

                // Multiply by wild downward speed so it throws down fast!
                rb.linearVelocity = directionToDrum * wildDownwardSpeed;
            }
            Debug.Log("Cinematic Intro Barrel Thrown!");
        }
        else
        {
            rb.linearVelocity = new Vector2(rollDirection * maxHorizontalSpeed * 0.6f, rb.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {
        if (type == BarrelType.Wild)
        {
            transform.Rotate(0, 0, -wildRotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            transform.Rotate(0, 0, -rb.linearVelocity.x * 3f);
        }

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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wildDownwardSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            rollDirection *= -1;
            rb.linearVelocity = new Vector2(rollDirection * maxHorizontalSpeed * 0.6f, rb.linearVelocity.y);
        }

        if (collision.gameObject.CompareTag("Ground") && isDroppingDownLadder)
        {
            LandedOnFloor();
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            HitPlayer(collision.gameObject);
        }

        // Standard Blue Barrel hits the Oil Drum physically
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

        if (collision.CompareTag("Wall") && type == BarrelType.Wild)
        {
            rollDirection *= -1;
            rb.linearVelocity = new Vector2(rollDirection * maxHorizontalSpeed * 0.8f, rb.linearVelocity.y);
        }

        if ((type == BarrelType.Standard || type == BarrelType.Blue) && collision.CompareTag("LadderTop") && !isDroppingDownLadder)
        {
            CalculateLadderDrop(collision.transform.position.x);
        }

        if (collision.CompareTag("Ground") && isDroppingDownLadder)
        {
            LandedOnFloor();
        }

        // --- FIX 2: Cinematic Barrel needs a Trigger check for the Oil Drum ---
        if (type == BarrelType.Cinematic && collision.CompareTag("OilDrum"))
        {
            InstantiateFireballEnemy(collision.transform.position);
            Destroy(gameObject);
        }

        if (!gameObject.activeInHierarchy) return;

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
            if (player.IsHammering) return;

            Debug.Log("Hazard destroyed the player!");
            GameManager.Instance.PlayerDeath();
        }
    }

    private void InstantiateFireballEnemy(Vector3 spawnPos)
    {
        Debug.Log("Blue barrel ignited the oil drum! Spawning Fireball Enemy!");
        if (fireballPrefab != null)
        {
            Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        }
    }
}