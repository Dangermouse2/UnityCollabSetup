using System.Collections;
using UnityEngine;

public class RivetManager : MonoBehaviour
{
    public static RivetManager Instance { get; private set; }

    private int totalRivets;

    // THE FIX: This boolean acts as a deadbolt to prevent overlapping coroutines
    private bool hasCollapseStarted = false;

    [Header("Boss Fall Sequence")]
    [SerializeField] private Animator bossAnimator;
    [SerializeField] private Transform bossTransform;
    [Tooltip("Place an empty GameObject where the boss should land, and drag it here")]
    [SerializeField] private Transform bossFallTarget;
    [SerializeField] private string fallTriggerName = "Fall";
    [SerializeField] private float fallDuration = 3.0f;
    [Tooltip("How long the boss hangs in the air before falling")]
    [SerializeField] private float coyoteTimeDelay = 1.0f;

    [Header("Environment Collapse")]
    [Tooltip("Drag all the ladders here so we can delete them")]
    [SerializeField] private GameObject[] ladders;
    [Tooltip("Drag all the platforms here so we can move them")]
    [SerializeField] private Transform[] platforms;
    [Tooltip("How far down the platforms should drop (e.g., Y = -1.5)")]
    [SerializeField] private float platformDropOffset = 1f;

    [Header("Hero Reunion")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform playerSafeSpot;
    [SerializeField] private Transform princessTransform;
    [SerializeField] private Transform princessSafeSpot;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Rivet[] allRivets = Object.FindObjectsByType<Rivet>(FindObjectsSortMode.None);
        totalRivets = allRivets.Length;
        Debug.Log($"Rivet Stage Loaded! Total Rivets to pop: {totalRivets}");
    }

    public void RivetPopped()
    {
        // THE FIX: If the sequence is already running, completely ignore any more rivet pops!
        if (hasCollapseStarted) return;

        totalRivets--;

        if (totalRivets <= 0)
        {
            // Lock the door behind us
            hasCollapseStarted = true;

            Debug.Log("ALL RIVETS POPPED! Structure collapsing!");
            StartCoroutine(BossFallSequenceRoutine());
        }
    }

    private IEnumerator BossFallSequenceRoutine()
    {
        // 1. Instantly freeze the player so they don't walk off the edge during the cinematic
        if (playerTransform.TryGetComponent(out PlayerController pc)) pc.enabled = false;
        if (playerTransform.TryGetComponent(out Rigidbody2D rb)) rb.linearVelocity = Vector2.zero;

        if (bossAnimator != null) bossAnimator.SetBool("Help", true);

        // 2. Teleport the Player and Princess to the top safe spots
        playerTransform.position = playerSafeSpot.position;
        princessTransform.position = princessSafeSpot.position;

        // 3. Delete (deactivate) all ladders
        foreach (GameObject ladder in ladders)
        {
            if (ladder != null) ladder.SetActive(false);
        }

        Vector3[] platformStartPos = new Vector3[platforms.Length];
        for (int i = 0; i < platforms.Length; i++)
        {
            platformStartPos[i] = platforms[i].position;
        }

        // 4. Smoothly move the Platforms over time
        float elapsedTime = 0f;
        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / fallDuration;

            // Move all platforms down slightly using your custom cascading math
            for (int i = 0; i < platforms.Length; i++)
            {
                if (platforms[i] != null)
                {
                    Vector3 targetPos = platformStartPos[i] + new Vector3(0, -platformDropOffset * (i + 1) - 0.61f * (i - 1), 0);
                    platforms[i].position = Vector3.Lerp(platformStartPos[i], targetPos, percentageComplete);
                }
            }

            // Wait until the next frame before continuing the loop
            yield return null;
        }

        // 5. The "Coyote Time" Pause
        // Platforms are gone, boss is hanging in the air!
        yield return new WaitForSeconds(coyoteTimeDelay);

        // 6. Trigger the Boss animation
        if (bossAnimator != null) bossAnimator.SetBool("Help", false);
        if (bossAnimator != null) bossAnimator.SetBool(fallTriggerName, true);

        // 7. Setup the starting positions for our smooth movement loop
        Vector3 bossStartPos = bossTransform.position;
        elapsedTime = 0f;

        // Move boss towards the bottom target
        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / fallDuration;

            if (bossTransform != null && bossFallTarget != null)
            {
                bossTransform.position = Vector3.Lerp(bossStartPos, bossFallTarget.position, percentageComplete);
            }
            yield return null;
        }

        // 8. Ensure everything snaps exactly to the final positions just in case
        if (bossTransform != null && bossFallTarget != null) bossTransform.position = bossFallTarget.position;

        // 9. Trigger the standard GameManager victory sequence NOW that everything is visually done
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelComplete();
        }
    }
}