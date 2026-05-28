using UnityEngine;

public class PuzzleManager : Interactables
{
    [Header("Puzzle Elements")]
    [SerializeField] private Statue[] statues;
    [SerializeField] private GameObject door;

    [Header("Winning Angles (Y-Axis)")]
    [Tooltip("Enter the required Y rotation for each statue in the same order as the array above.")]
    [SerializeField] private float[] correctAngles;

    private bool puzzleSolved = false;

    protected override void Interact()
    {
        if (puzzleSolved) return; // Don't do anything if already solved

        if (CheckSolution())
        {
            SolvePuzzle();
        }
        else
        {
            Debug.Log("The statues are not in the correct position...");
        }
    }

    private bool CheckSolution()
    {
        for (int i = 0; i < statues.Length; i++)
        {
            // Get the current Y rotation (0 to 360)
            float currentY = statues[i].transform.eulerAngles.y;

            // Use Mathf.DeltaAngle to handle the wrap-around (e.g., 0 and 360 are the same)
            if (Mathf.Abs(Mathf.DeltaAngle(currentY, correctAngles[i])) > 0.1f)
            {
                return false; // One statue is wrong, so the whole thing is wrong
            }
        }
        return true; // All statues match!
    }

    private void SolvePuzzle()
    {
        puzzleSolved = true;
        Debug.Log("Puzzle Solved!");

        // 1. Freeze all statues
        foreach (Statue s in statues)
        {
            s.isLocked = true;
        }

        // 2. Open the door (You can replace this with an animation trigger)
        door.SetActive(false);
        // OR: door.GetComponent<Animator>().SetTrigger("Open");
    }
}