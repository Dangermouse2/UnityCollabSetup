using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // <-- CRITICAL: Required to reload scenes!

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private int score = 0;
    [SerializeField] private int highScore = 10000;

    [Header("Arcade Timer Settings")]
    [SerializeField] private int startingBonusTime = 5000;
    private float currentBonusTime;
    private bool isGameActive = false;

    [Header("Start Delay")]
    [SerializeField] private float levelStartDelay = 3f;

    [Header("Death Animation Settings")]
    [SerializeField] private float deathDelay = 2.5f; // How long the death animation takes to play

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Subscribe to scene loading so the game automatically kickstarts 
            // every time a scene finishes loading
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // Clean up our subscription if the manager is ever destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        score = 0;
        playerLives = 3;
        ReloadCurrentScene();
    }

    private void ReloadCurrentScene()
    {
        isGameActive = false;
        // This grabs whatever scene is currently open and reloads it cleanly
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // This automatically fires the moment Unity finishes loading the scene
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(LoadLevelRoutine());
    }

    private IEnumerator LoadLevelRoutine()
    {
        isGameActive = false;
        currentBonusTime = startingBonusTime;

        Debug.Log($"HOW HIGH CAN YOU GET? Level starting in {levelStartDelay} seconds...");

        // This pause gives players a moment to breathe before hazards start spawning
        yield return new WaitForSeconds(levelStartDelay);

        Debug.Log("GO!");
        isGameActive = true;
    }

    void Update()
    {
        if (!isGameActive) return;

        if (currentBonusTime > 0)
        {
            currentBonusTime -= Time.deltaTime * 40f;
            if (currentBonusTime <= 0)
            {
                currentBonusTime = 0;
                PlayerDeath();
            }
        }
    }

    public void AddScore(int points)
    {
        score += points;
        if (score > highScore)
        {
            highScore = score;
            // Save it to disk instantly so the main menu can read it later!
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        Debug.Log($"Score: {score} | High Score: {highScore}");
    }

    // --- FIX: Streamlined PlayerDeath to hand off ALL control to the Coroutine ---
    public void PlayerDeath()
    {
        if (!isGameActive) return;
        isGameActive = false;

        playerLives--;
        Debug.Log($"Player Died! Lives remaining: {playerLives}");

        // Let the coroutine handle the visual delay AND the ultimate decision to reload or Game Over!
        StartCoroutine(PlayerDeathSequenceRoutine());
    }

    public void LevelComplete()
    {
        if (!isGameActive) return;
        isGameActive = false;

        int finalBonusAward = Mathf.FloorToInt(currentBonusTime);
        AddScore(finalBonusAward);

        Debug.Log($"Level Complete! Awarded {finalBonusAward} Bonus Points!");

        // Reloads the level to advance/loop the game state
        ReloadCurrentScene();
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER. Insert Coin to Play Again.");

        playerLives = 3;
        score = 0;

        SceneManager.LoadScene(0);
    }

    private IEnumerator PlayerDeathSequenceRoutine()
    {
        // 1. Find the player controller and trigger their animation hook
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            // Disable player movement script inputs so they can't walk around while dead
            player.enabled = false;

            // --- FIX: Change the body type to Kinematic to freeze all physics evaluations ---
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.bodyType = RigidbodyType2D.Kinematic; // <- ABSOLUTE LOCKDOWN
            }


            // Trigger the animation parameter!
            Animator playerAnim = player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                // --- FIX: Force the animator speed back to 1f so it can actually play! ---
                playerAnim.speed = 1f;

                playerAnim.SetBool("isDead", true);

                // --- FIX: Force-clear active movement parameters so they don't fight the death state ---
                playerAnim.SetBool("isClimbing", false);

                
            }
        }

        // 2. Wait for the animation to finish playing out on screen
        yield return new WaitForSeconds(deathDelay);

        // 3. Evaluate state and reload AFTER the delay has completely finished
        if (playerLives > 0)
        {
            ReloadCurrentScene();
        }
        else
        {
            GameOver();
        }
    }

    // Public getters for UI
    public int GetScore() => score;
    public int GetHighScore() => highScore;
    public int GetLives() => playerLives;
    public int GetBonusTime() => Mathf.FloorToInt(currentBonusTime);
    public bool IsGameActive() => isGameActive;
}