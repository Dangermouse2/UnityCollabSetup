using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss Arsenal (Prefabs)")]
    public GameObject standardBarrelPrefab;
    public GameObject wildBarrelPrefab;
    public GameObject blueBarrelPrefab;

    // --- NEW: Dedicated slot for the opening cinematic barrel ---
    public GameObject introBlueBarrelPrefab;

    [Header("Spawning Configuration")]
    public Transform spawnPoint;
    public float spawnInterval = 3.5f;

    [Header("Animation Timings")]
    [Tooltip("How long to wait after starting the throw animation before the barrel actually appears.")]
    public float barrelSpawnDelay = 0.4f;

    private Animator animator;
    private float spawnTimer;
    private bool hasThrownIntroBarrel = false; // Tracks if the intro happened

    void Start()
    {
        animator = GetComponent<Animator>();
        spawnTimer = spawnInterval;

        // --- NEW: Trigger the cinematic throw immediately on load! ---
        StartCoroutine(IntroThrowSequence());
    }

    void Update()
    {
        // If the game isn't actively running (like during start delay or death), stop normal spawning!
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive()) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            StartCoroutine(ThrowBarrelSequence());
            // Reset timer with a tiny bit of arcade randomness so it isn't perfectly predictable
            spawnTimer = spawnInterval + Random.Range(-0.5f, 0.5f);
        }
    }

    // --- NEW: Dedicated routine just for the opening cutscene drop ---
    private IEnumerator IntroThrowSequence()
    {
        hasThrownIntroBarrel = true;

        // Wait just a split second so the scene settles before Kong moves
        yield return new WaitForSeconds(0.2f);

        if (animator != null)
        {
            // You can use the same throw animation, or make a special "Drop" one later!
            animator.SetTrigger("ThrowBarrel");
        }

        yield return new WaitForSeconds(barrelSpawnDelay);

        if (introBlueBarrelPrefab != null && spawnPoint != null)
        {
            // Spawn the special cinematic barrel!
            Instantiate(introBlueBarrelPrefab, spawnPoint.position, Quaternion.identity);
            
        }
        else
        {
            Debug.LogWarning("Missing Intro Barrel Prefab in BossController!");
        }
    }

    private IEnumerator ThrowBarrelSequence()
    {
        // 1. Trigger the animation hook to start the throw motion
        if (animator != null)
        {
            animator.SetTrigger("ThrowBarrel");
        }

        // 2. Wait for the exact visual frame where Kong "drops" the barrel
        yield return new WaitForSeconds(barrelSpawnDelay);

        // 3. Roll the dice to decide which type of barrel to throw
        float roll = Random.value;
        GameObject barrelToThrow = standardBarrelPrefab; // Default fallback

        if (roll > 0.85f)
        {
            barrelToThrow = blueBarrelPrefab;  // 15% chance
        }
        else if (roll > 0.65f)
        {
            barrelToThrow = wildBarrelPrefab;  // 20% chance
        }

        // 4. Create the selected barrel type at the spawn point
        if (barrelToThrow != null && spawnPoint != null)
        {
            Instantiate(barrelToThrow, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Cannot spawn barrel! Check your Prefab or SpawnPoint assignments in the Inspector.");
        }
    }
}