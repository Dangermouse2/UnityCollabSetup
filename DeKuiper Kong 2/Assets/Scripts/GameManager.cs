using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
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

    // --- NEW: VICTORY AUDIO SETTINGS ---
    [Header("Victory Settings")]
    [SerializeField] private AudioClip victorySound; // Assign your retro win tune here
    [SerializeField] private float victoryDelay = 3.5f; // How long to wait before loading next level
    [SerializeField] private AudioSource soundEffectsSource;

    [Header("Music Settings")]
    [SerializeField] private AudioSource musicSource; // Separate source dedicated to looping music
    [SerializeField] private AudioClip mainTheme;
    [SerializeField] private AudioClip hammerTheme;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

    public void PlayMainTheme()
    {
        if (musicSource == null || mainTheme == null) return;

        // Only swap if it isn't already playing (prevents music restarting tracking errors)
        if (musicSource.clip == mainTheme && musicSource.isPlaying) return;

        musicSource.clip = mainTheme;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayHammerTheme()
    {
        if (musicSource == null || hammerTheme == null) return;

        musicSource.clip = hammerTheme;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
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

        PlayMainTheme();
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

        StopMusic();

        playerLives--;
        Debug.Log($"Player Died! Lives remaining: {playerLives}");

        // Let the coroutine handle the visual delay AND the ultimate decision to reload or Game Over!
        StartCoroutine(PlayerDeathSequenceRoutine());
    }

    // --- REWORKED: LEVEL COMPLETE ROUTINE TIE-IN ---
    public void LevelComplete()
    {
        if (!isGameActive) return;
        isGameActive = false; // Freeze the game timer and interactions

        StopMusic();

        // Start the celebratory visual/audio delay sequence
        StartCoroutine(LevelCompleteSequenceRoutine());
    }

    private IEnumerator LevelCompleteSequenceRoutine()
    {
        // 1. Play the magnificent victory music
        if (soundEffectsSource != null && victorySound != null)
        {
            soundEffectsSource.PlayOneShot(victorySound);
        }

        // 2. Lock down the player so they strike an idle/victory pose
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false; // Turn off inputs
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero; // Stop physics drift

            Animator playerAnim = player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                playerAnim.SetBool("isWalking", false);
                playerAnim.SetBool("isJumping", false);
                playerAnim.SetBool("isClimbing", false);
                // If you ever make a custom "win" animation state, you could trigger it here!
            }
        }

        // 3. Process the score rewards
        int finalBonusAward = Mathf.FloorToInt(currentBonusTime);
        AddScore(finalBonusAward);
        Debug.Log($"Level Complete! Awarded {finalBonusAward} Bonus Points!");

        // 4. Let the victory sound play out completely
        yield return new WaitForSeconds(victoryDelay);

        // 5. Safely advance to the reloaded/next scene
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
        // 1. Find the player controller and trigger their clean death routine
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            // --- FIX: Run the player's internal death system (PLAYS THE DEATH SOUND!) ---
            player.Die();

            // --- FIX: Force the body type to Kinematic to freeze all physics evaluations ---
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
                playerAnim.speed = 1f;
                playerAnim.SetBool("isDead", true);
                playerAnim.SetBool("isClimbing", false);
            }
        }

        // 2. Wait for the animation and audio to finish playing out on screen
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