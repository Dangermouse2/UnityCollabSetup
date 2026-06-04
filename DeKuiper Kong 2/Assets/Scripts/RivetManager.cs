using UnityEngine;

public class RivetManager : MonoBehaviour
{
    public static RivetManager Instance { get; private set; }

    private int totalRivets;

    void Awake()
    {
        // Simple Singleton pattern for this specific scene
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Automatically find and count every rivet currently in the scene!
        Rivet[] allRivets = Object.FindObjectsByType<Rivet>(FindObjectsSortMode.None);
        totalRivets = allRivets.Length;

        Debug.Log($"Rivet Stage Loaded! Total Rivets to pop: {totalRivets}");
    }

    public void RivetPopped()
    {
        totalRivets--;
      //  Debug.Log($"Rivet Popped! Remaining: {totalRivets}");

        // Check for victory condition!
        if (totalRivets <= 0)
        {
            Debug.Log("ALL RIVETS POPPED! Structure collapsing!");

            if (GameManager.Instance != null)
            {
                // Trigger the standard victory sequence
                GameManager.Instance.LevelComplete();
            }
        }
    }
}