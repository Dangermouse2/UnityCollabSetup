using UnityEngine;
using System.Collections;

public class Statue : Interactables
{
    [SerializeField] private float rotationDuration = 1.5f;
    private bool isRotating = false;

    // The PuzzleManager will set this to true when the door opens
    [HideInInspector] public bool isLocked = false;

    protected override void Interact()
    {
        // Don't rotate if already moving OR if the puzzle is already solved
        if (!isRotating && !isLocked)
        {
            StartCoroutine(RotateSmoothly(90f));
        }
    }

    private IEnumerator RotateSmoothly(float angle)
    {
        isRotating = true;

        Quaternion startRotation = transform.rotation;
        // Using *= Quaternion.Euler ensures it always turns 90 deg relative to where it is
        Quaternion endRotation = transform.rotation * Quaternion.Euler(0, angle, 0);
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            float t = elapsed / rotationDuration;

            // Optional: Makes the start/stop feel more "weighty" and less robotic
            float smoothT = Mathf.SmoothStep(0, 1, t);

            transform.rotation = Quaternion.Slerp(startRotation, endRotation, smoothT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        isRotating = false;
    }
}