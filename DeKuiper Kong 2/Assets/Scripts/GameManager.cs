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
    [SerializeField] private AudioClip mainTheme;       // Main Menu Theme
    [SerializeField] private AudioClip hammerTheme;     // Power-up music
    [SerializeField] private AudioClip girderStageMusic; // Level 1 Music
    [SerializeField] private AudioClip rivetStageMusic;  // Level 2 Music

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
        // --- FIX: Don't force auto-reset on frame 1. Just play music for whatever scene we started in!
        PlayMainTheme();
    }

    // --- FIXED: Fail-safe track routing using BOTH indices and Scene Strings ---
    public void PlayMainTheme()
    {
        if (musicSource == null) return;

        AudioClip clipToPlay = mainTheme; // Default fallback (Main Menu)

        string sceneName = SceneManager.GetActiveScene().name.ToLower();
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;

        // Route the track based on index OR scene name to bypass configuration errors
        if (currentBuildIndex == 1 || sceneName.Contains("girder") || sceneName.Contains("level1"))
        {
            clipToPlay = girderStageMusic;
        }
        else if (currentBuildIndex == 2 || sceneName.Contains("rivet") || sceneName.Contains("level2"))
        {
            clipToPlay = rivetStageMusic;
        }
        else if (currentBuildIndex == 0 || sceneName.Contains("menu") || sceneName.Contains("title"))
        {
            clipToPlay = mainTheme;
        }

        if (clipToPlay == null) return;

        // Only swap if it isn't already playing (prevents music cutting itself off)
        if (musicSource.clip == clipToPlay && musicSource.isPlaying) return;

        musicSource.clip = clipToPlay;
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

    // --- FIX: Called by your Main Menu UI "Play" button ---
    public void StartNewGame()
    {
        score = 0;
        playerLives = 3;

        // Explicitly load Level 1 (Girder stage) instead of looping the menu scene!
        SceneManager.LoadScene(1);
    }

    private void ReloadCurrentScene()
    {
        isGameActive = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadNextLevel()
    {
        isGameActive = false;

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // ARCADE LOOP: Loop back to level 1 if we run out of stages
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 1;
            Debug.Log("Game Loop Completed! Resetting cycle...");
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // --- FIX: INSTANTLY kill leftover audio from previous screens on arrival ---
        StopMusic();

        string sceneName = scene.name.ToLower();
        if (scene.buildIndex == 0 || sceneName.Contains("menu") || sceneName.Contains("title"))
        {
            isGameActive = false;
            PlayMainTheme(); // Instantly spin up main menu track
        }
        else
        {
            StartCoroutine(LoadLevelRoutine());
        }
    }

    private IEnumerator LoadLevelRoutine()
    {
        isGameActive = false;
        currentBonusTime = startingBonusTime;

        // --- FIX: Start level-specific music IMMEDIATELY during the countdown panel! ---
        PlayMainTheme();

        Debug.Log($"HOW HIGH CAN YOU GET? Level starting in {levelStartDelay} seconds...");

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
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        Debug.Log($"Score: {score} | High Score: {highScore}");
    }

    public void PlayerDeath()
    {
        if (!isGameActive) return;
        isGameActive = false;

        StopMusic();

        playerLives--;
        Debug.Log($"Player Died! Lives remaining: {playerLives}");

        StartCoroutine(PlayerDeathSequenceRoutine());
    }

    public void LevelComplete()
    {
        if (!isGameActive) return;
        isGameActive = false;

        StopMusic();

        StartCoroutine(LevelCompleteSequenceRoutine());
    }

    private IEnumerator LevelCompleteSequenceRoutine()
    {
        if (soundEffectsSource != null && victorySound != null)
        {
            soundEffectsSource.PlayOneShot(victorySound);
        }

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero;

            Animator playerAnim = player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                playerAnim.SetBool("isWalking", false);
                playerAnim.SetBool("isJumping", false);
                playerAnim.SetBool("isClimbing", false);
            }
        }

        int finalBonusAward = Mathf.FloorToInt(currentBonusTime);
        AddScore(finalBonusAward);
        Debug.Log($"Level Complete! Awarded {finalBonusAward} Bonus Points!");

        yield return new WaitForSeconds(victoryDelay);

        LoadNextLevel();
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER. Insert Coin to Play Again.");

        playerLives = 3;
        score = 0;

        SceneManager.LoadScene(0); // Take us back to Main Menu
    }

    private IEnumerator PlayerDeathSequenceRoutine()
    {
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.Die();

            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.bodyType = RigidbodyType2D.Kinematic;
            }

            Animator playerAnim = player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                playerAnim.speed = 1f;
                playerAnim.SetBool("isDead", true);
                playerAnim.SetBool("isClimbing", false);
            }
        }

        yield return new WaitForSeconds(deathDelay);

        if (playerLives > 0)
        {
            ReloadCurrentScene();
        }
        else
        {
            GameOver();
        }
    }

    public int GetScore() => score;
    public int GetHighScore() => highScore;
    public int GetLives() => playerLives;
    public int GetBonusTime() => Mathf.FloorToInt(currentBonusTime);
    public bool IsGameActive() => isGameActive;
}