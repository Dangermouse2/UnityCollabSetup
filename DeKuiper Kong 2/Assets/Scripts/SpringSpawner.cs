using UnityEngine;

public class SpringSpawner : MonoBehaviour
{
    [Header("Prefab Setup")]
    [SerializeField] private GameObject springPrefab; // Drop your Bouncing Spring prefab here

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 4.5f; // Seconds between each spring release
    [SerializeField] private float initialDelay = 1.5f;   // Waiting time when the countdown finishes

    private float spawnTimer;
    private bool hasStartedSpawning = false;

    void Start()
    {
        // Start the timer at 0, but use the initial delay gate before rolling out the first spring
        spawnTimer = 0f;
    }

    void Update()
    {
        // --- ARCADE STATE CHECK ---
        // If the level is in a countdown, player is dying, or game is won, freeze the spawner
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive())
        {
            return;
        }

        // Handle the initial safe delay window when the level first starts up
        if (!hasStartedSpawning)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= initialDelay)
            {
                SpawnSpring();
                spawnTimer = 0f;
                hasStartedSpawning = true; // Hand off logic to the regular interval loop
            }
            return;
        }

        // Standard gameplay loop spawning
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnSpring();
            spawnTimer = 0f; // Reset interval
        }
    }

    private void SpawnSpring()
    {
        if (springPrefab == null)
        {
            Debug.LogWarning($"Spring Spawner on {gameObject.name} is missing its Spring Prefab assignation!");
            return;
        }

        // Instantiate the spring exactly at this Spawner's coordinate position
        // Quaternion.identity ensures the sprite spawns right-side up with no rotation angles applied
        GameObject newSpring = Instantiate(springPrefab, transform.position, Quaternion.identity);

        Debug.Log("DK threw a spring!");
    }
}