using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Text & Elements")]
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI promptText; // Your flashing text (Optional)

    [Header("Visual Settings")]
    [SerializeField] private float flashInterval = 0.5f; // Speed of flashing text
    [SerializeField] private string gameplaySceneName = "Stage1"; // Name of your game level scene

    private bool isStarting = false;

    void Start()
    {
        // 1. Load high score from disk (defaults to 10000 if none exists)
        int savedHighScore = PlayerPrefs.GetInt("HighScore", 10000);
        highScoreText.text = savedHighScore.ToString("D6");

        // 2. Start the flashing text prompt if you are using one
        if (promptText != null)
        {
            StartCoroutine(FlashPromptRoutine());
        }
    }

    // --- FIX: This is the public method your Button component will point to ---
    public void OnStartButtonClick()
    {
        if (isStarting) return; // Prevent double clicking while scene is loading

        isStarting = true;
        Debug.Log("Start Button Clicked! Loading Game...");

        // Optional: Trigger your arcade selection audio clip here!

        
            SceneManager.LoadScene(gameplaySceneName);
    
    }

    private IEnumerator FlashPromptRoutine()
    {
        while (!isStarting)
        {
            promptText.enabled = !promptText.enabled;
            yield return new WaitForSeconds(flashInterval);
        }
        promptText.enabled = false;
    }
}