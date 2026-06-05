using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroCinematic : MonoBehaviour
{
    [Header("Actors (GameObjects)")]
    [SerializeField] private GameObject penelope;
    [SerializeField] private GameObject dk;
    [SerializeField] private GameObject jumpman;
    [SerializeField] private GameObject barrelPrefab;

    [Header("Animators")]
    [SerializeField] private Animator penelopeAnim;
    [SerializeField] private Animator dkAnim;
    [SerializeField] private Animator jumpmanAnim;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;

    [Header("Waypoints (Empty Transforms)")]
    [SerializeField] private Transform dkStartPos;
    [SerializeField] private Transform penelopePos;
    [SerializeField] private Transform rightOffscreenPos;
    [SerializeField] private Transform jumpmanStartPos;
    [SerializeField] private Transform barrelStartPos;
    [SerializeField] private Transform jumpmanJumpPoint;

    [Header("Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float barrelSpeed = 7f;
    [SerializeField] private float jumpHeight = 2.5f;
    [SerializeField] private float jumpDuration = 0.6f;

    void Start()
    {
        // Hide DK and Jumpman at the very beginning
        dk.transform.position = dkStartPos.position;
        jumpman.transform.position = jumpmanStartPos.position;

        // Start the cinematic timeline
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // 1. Penelope stands in idle. Wait for a brief moment so the player registers the scene.
        yield return new WaitForSeconds(1.5f);

        // 2. Penelope turns around and gets frightened
        penelope.transform.localScale = new Vector3(-penelope.transform.localScale.x, penelope.transform.localScale.y, penelope.transform.localScale.z);
        penelopeAnim.SetTrigger("isFrightened");
        yield return new WaitForSeconds(0.5f);

        // 3. DK walks in from off-screen
        dkAnim.SetBool("isWalking", true);
        while (Vector2.Distance(dk.transform.position, penelopePos.position) > 0.1f)
        {
            dk.transform.position = Vector2.MoveTowards(dk.transform.position, penelopePos.position, walkSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame
        }

        // 4. DK reaches Penelope. Destroy Penelope object and switch DK to holding state
        Destroy(penelope);
        dkAnim.SetBool("isWalking", false);
        dkAnim.SetBool("grabRicky", true);
        yield return new WaitForSeconds(0.8f);

        // 5. DK runs off the screen with Penelope
        dkAnim.SetBool("grabRicky", false);
        dkAnim.SetBool("isRunning", true);
        while (Vector2.Distance(dk.transform.position, rightOffscreenPos.position) > 0.1f)
        {
            dk.transform.position = Vector2.MoveTowards(dk.transform.position, rightOffscreenPos.position, runSpeed * Time.deltaTime);
            yield return null;
        }

        // 6. Jumpman runs in to chase them
        jumpmanAnim.SetBool("isWalking", true);

        // Spawn the rolling barrel
        GameObject barrel = Instantiate(barrelPrefab, barrelStartPos.position, Quaternion.identity);

        // Move Jumpman to the designated jump point while the barrel rolls toward him
        while (Vector2.Distance(jumpman.transform.position, jumpmanJumpPoint.position) > 0.1f)
        {
            jumpman.transform.position = Vector2.MoveTowards(jumpman.transform.position, jumpmanJumpPoint.position, runSpeed * Time.deltaTime);
            if (barrel != null) barrel.transform.Translate(Vector3.left * barrelSpeed * Time.deltaTime);

            yield return null;
        }

        // 7. Jumpman performs the jump over the barrel
        jumpmanAnim.SetBool("isJumping", true);
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);

        float elapsed = 0f;
        Vector3 jumpStartLocal = jumpman.transform.position;

        // A math-based arc to simulate a jump without relying on physics engines
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float timeNormalized = elapsed / jumpDuration;

            // Move forward horizontally
            float currentX = Mathf.Lerp(jumpStartLocal.x, jumpStartLocal.x + 3f, timeNormalized);

            // Calculate vertical arc using a Sine wave
            float currentY = jumpStartLocal.y + (Mathf.Sin(timeNormalized * Mathf.PI) * jumpHeight);

            jumpman.transform.position = new Vector3(currentX, currentY, jumpStartLocal.z);

            // Keep rolling the barrel left
            if (barrel != null) barrel.transform.Translate(Vector3.left * barrelSpeed * Time.deltaTime);

            yield return null;
        }

        // 8. Jumpman lands and continues running offscreen
        jumpmanAnim.SetBool("isWalking", true);
        jumpmanAnim.SetBool("isJumping", false);
        while (Vector2.Distance(jumpman.transform.position, rightOffscreenPos.position) > 0.1f)
        {
            jumpman.transform.position = Vector2.MoveTowards(jumpman.transform.position, rightOffscreenPos.position, runSpeed * Time.deltaTime);

            if (barrel != null) barrel.transform.Translate(Vector3.left * barrelSpeed * Time.deltaTime);

            yield return null;
        }

        // Cleanup the barrel
        if (barrel != null) Destroy(barrel);

        // 9. Start the actual arcade loop
        if (GameManager.Instance != null)
        {
            // --- FIX: Calls our new starting function instead of advancing a broken index ---
            GameManager.Instance.StartArcadeSequence();
        }
        else
        {
            // Fail-safe just in case the GameManager is ever missing during testing
            SceneManager.LoadScene(1);
        }
    }
}