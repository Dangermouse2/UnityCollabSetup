using UnityEditor;
using UnityEngine;

public class Rivet : MonoBehaviour
{
    [Header("Rivet Settings")]
    [SerializeField] private int scoreValue = 100;
    [SerializeField] private AudioClip popSound;

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Only the player can pop rivets!
        if (collision.collider.CompareTag("Player"))
        {
            // 1. Award Points
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            // 2. Play a satisfying pop/break sound
            if (popSound != null)
            {
                AudioSource.PlayClipAtPoint(popSound, Camera.main.transform.position);
            }

            // 3. Tell the level that a rivet was removed
            if (RivetManager.Instance != null)
            {
                RivetManager.Instance.RivetPopped();
            }

            // 4. Destroy this rivet to reveal the gap!
            Destroy(gameObject);
        }
    }
}