using UnityEngine;

public class MovingElevator : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float direction = 1f; // 1 for UP, -1 for DOWN

    // --- FIX: Replace raw floats with Transform references ---
    [Header("Wrap Boundaries")]
    [SerializeField] private Transform bottomBoundary; // Empty GameObject at the bottom
    [SerializeField] private Transform topBoundary;    // Empty GameObject at the top

    void FixedUpdate()
    {
        // Safety check to avoid null reference errors if you forget to assign them
        if (bottomBoundary == null || topBoundary == null) return;

        // Move translationally
        transform.Translate(Vector3.up * direction * moveSpeed * Time.fixedDeltaTime);

        // Wrapping Logic using the Y position of your empty transforms
        if (direction > 0 && transform.position.y > topBoundary.position.y)
        {
            transform.position = new Vector3(transform.position.x, bottomBoundary.position.y, transform.position.z);
        }
        else if (direction < 0 && transform.position.y < bottomBoundary.position.y)
        {
            transform.position = new Vector3(transform.position.x, topBoundary.position.y, transform.position.z);
        }
    }
}