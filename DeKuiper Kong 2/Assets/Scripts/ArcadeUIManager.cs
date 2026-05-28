using UnityEngine;
using TMPro; // Required for TextMeshPro components

public class ArcadeUIManager : MonoBehaviour
{
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI bonusTimerText;

    [Header("Lives Display Settings")]
    [SerializeField] private GameObject[] lifeIcons; // Array of image elements representing spare lives

    void Update()
    {
        // Safety check to ensure GameManager exists before reading values
        if (GameManager.Instance == null) return;

        UpdateDashboardTexts();
        UpdateLivesDisplay();
    }

    private void UpdateDashboardTexts()
    {
        // 1. Format Score and High Score with clean retro zero-padding (e.g., 000120)
        scoreText.text = GameManager.Instance.GetScore().ToString("D6");
        highScoreText.text = GameManager.Instance.GetHighScore().ToString("D6");

        // 2. Update the Bonus Time counter
        int currentBonus = GameManager.Instance.GetBonusTime();
        bonusTimerText.text = currentBonus.ToString();

        // 3. Optional visual cue: Turn the text red if the bonus time drops below 1000!
        if (currentBonus < 1000)
        {
            bonusTimerText.color = Color.red;
        }
        else
        {
            bonusTimerText.color = Color.yellow; // Classic arcade yellow
        }
    }

    private void UpdateLivesDisplay()
    {
        int currentLives = GameManager.Instance.GetLives();

        // Loop through our array of UI life icons and toggle them active/inactive
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            // If our current life count is greater than the loop index, keep the icon visible
            if (i < currentLives)
            {
                lifeIcons[i].SetActive(true);
            }
            else
            {
                lifeIcons[i].SetActive(false);
            }
        }
    }
}