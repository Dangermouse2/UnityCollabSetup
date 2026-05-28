using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss Arsenal (Prefabs)")]
    public GameObject standardBarrelPrefab;
    public GameObject wildBarrelPrefab;
    public GameObject blueBarrelPrefab;

    [Header("Spawning Configuration")]
    public Transform spawnPoint;
    public float spawnInterval = 3.5f;

    [Header("Animation Timings")]
    [Tooltip("How long to wait after starting the throw animation before the barrel actually appears.")]
    public float barrelSpawnDelay = 0.4f;

    private Animator animator;
    private float spawnTimer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        // If the game isn't actively running (like during start delay or death), stop spawning!
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive()) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            StartCoroutine(ThrowBarrelSequence());
            // Reset timer with a tiny bit of arcade randomness so it isn't perfectly predictable
            spawnTimer = spawnInterval + Random.Range(-0.5f, 0.5f);
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