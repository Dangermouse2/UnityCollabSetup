using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RivetSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject fireballPrefab;

    [Header("Spawning Positions")]
    [SerializeField] private Transform[] leftSpawnPoints;
    [SerializeField] private Transform[] rightSpawnPoints;

    [Header("Spawning Rules")]
    [SerializeField] private float initialDelay = 3.0f;
    [SerializeField] private float spawnInterval = 4.5f;
    [SerializeField] private int maxFireballsOnScreen = 5;

    // --- NEW: Time to wait for the chest pound animation frame ---
    [Tooltip("How long into the chest pound animation before the fireball actually spawns.")]
    [SerializeField] private float chestPoundDelay = 0.5f;

    private Transform playerTransform;
    private float spawnTimer;
    private List<GameObject> activeFireballs = new List<GameObject>();
    private Animator dkAnimator;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Grabs the animator from Kong (assumes script is on Kong or he has the animator)
        dkAnimator = GetComponent<Animator>();

        spawnTimer = initialDelay;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive()) return;

        activeFireballs.RemoveAll(item => item == null);
        if (activeFireballs.Count >= maxFireballsOnScreen) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            // --- UPDATED: Fire off the timed sequence coroutine ---
            StartCoroutine(SpawnFireballSequenceRoutine());
            spawnTimer = spawnInterval;
        }
    }

    // --- UPDATED: Turned into a Coroutine for frame-perfect sync ---
    private IEnumerator SpawnFireballSequenceRoutine()
    {
        // 1. Tell Donkey Kong to start pounding his chest
        if (dkAnimator != null)
        {
            dkAnimator.SetTrigger("ChestPound");
        }

        // 2. Wait for the exact visual frame where the fireball should manifest
        yield return new WaitForSeconds(chestPoundDelay);

        // 3. Run the positioning math after the delay finishes
        if (fireballPrefab == null) yield break;

        bool playerIsOnRightSide = true;
        if (playerTransform != null)
        {
            playerIsOnRightSide = playerTransform.position.x > 0f;
        }

        Transform chosenSpawnPoint = null;

        if (playerIsOnRightSide)
        {
            if (leftSpawnPoints.Length > 0)
            {
                chosenSpawnPoint = leftSpawnPoints[Random.Range(0, leftSpawnPoints.Length)];
            }
        }
        else
        {
            if (rightSpawnPoints.Length > 0)
            {
                chosenSpawnPoint = rightSpawnPoints[Random.Range(0, rightSpawnPoints.Length)];
            }
        }

        // 4. Instantly spawn the fireball at the synchronized moment
        if (chosenSpawnPoint != null)
        {
            GameObject newFireball = Instantiate(fireballPrefab, chosenSpawnPoint.position, Quaternion.identity);
            activeFireballs.Add(newFireball);
        }
    }
}