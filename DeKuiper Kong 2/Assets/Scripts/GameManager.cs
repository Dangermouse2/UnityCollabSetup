using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private int score = 0;
    [SerializeField] private int highScore = 10000;

    [Header("Arcade Level Loop Sequence")]
    [Tooltip("Type your scene names exactly as they appear in your project folder to match the classic DK loop layout.")]
    [SerializeField] private string[] levelSequence = { "GirderStage", "RivetStage", "GirderStage", "ElevatorStage", "RivetStage" };
    private int currentSequenceIndex = 0;

    [Header("Arcade Timer Settings")]
    [SerializeField] private int startingBonusTime = 5000;
    private float currentBonusTime;
    private bool isGameActive = false;

    [Header("Start Delay")]
    [SerializeField] private float levelStartDelay = 3f;

    [Header("Death Animation Settings")]
    [SerializeField] private float deathDelay = 2.5f; 

    [Header("Victory Settings")]
    [SerializeField] private AudioClip victorySound; 
    [SerializeField] private float victoryDelay = 3.5f; 
    [SerializeField] private AudioSource soundEffectsSource;

    [Header("Music Settings")]
    [SerializeField] private AudioSource musicSource; 
    [SerializeField] private AudioClip mainTheme;       
    [SerializeField] private AudioClip hammerTheme;     
    [SerializeField] private AudioClip girderStageMusic; 
    [SerializeField] private AudioClip elevatorStageMusic; // <-- NEW: Dedicated elevator track slot
    [SerializeField] private AudioClip rivetStageMusic;  

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
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        PlayMainTheme();
    }

    // --- FIXED: Pure string name identification for smart music mapping ---
    public void PlayMainTheme()
    {
        if (musicSource == null) return;

        AudioClip clipToPlay = mainTheme; 

        string sceneName = SceneManager.GetActiveScene().name.ToLower();

        if (sceneName.Contains("girder"))
        {
            clipToPlay = girderStageMusic;
        }
        else if (sceneName.Contains("elevator"))
        {
            clipToPlay = elevatorStageMusic;
        }
        else if (sceneName.Contains("rivet"))
        {
            clipToPlay = rivetStageMusic;
        }
        else if (sceneName.Contains("menu") || sceneName.Contains("title"))
        {
            clipToPlay = mainTheme;
        }

        if (clipToPlay == null) return;

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
        if (musicSource != null) musicSource.Stop();
    }

    // --- FIXED: Explicitly resets sequence indexes when starting a clean playthrough ---
    public void StartNewGame()
    {
        score = 0;
        playerLives = 3;
        currentSequenceIndex = 0;

        if (levelSequence != null && levelSequence.Length > 0)
        {
            SceneManager.LoadScene(levelSequence[0]);
        }
        else
        {
            Debug.LogError("Level Sequence Array is empty on the GameManager component!");
        }
    }

    private void ReloadCurrentScene()
    {
        isGameActive = false;
        // Re-loads by name string to bypass rigid build indices entirely
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- FIXED: Handles the custom Arcade Loop sequence calculations ---
    private void LoadNextLevel()
    {
        isGameActive = false;
        currentSequenceIndex++;

        // ARCADE LOOP DETECTOR
        if (currentSequenceIndex >= levelSequence.Length)
        {
            // Once the sequence array runs out, loop back to the start of Level 2 
            // (Index 2 in our array is the second Girder stage sequence)
            currentSequenceIndex = 2; 
            Debug.Log("Arcade loop complete! Cycling back into advanced difficulty playlist...");
        }

        SceneManager.LoadScene(levelSequence[currentSequenceIndex]);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopMusic();

        string sceneName = scene.name.ToLower();
        if (sceneName.Contains("menu") || sceneName.Contains("title"))
        {
            isGameActive = false;
            PlayMainTheme(); 
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
    }

    public void PlayerDeath()
    {
        if (!isGameActive) return;
        isGameActive = false;

        StopMusic();
        playerLives--;

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

        yield return new WaitForSeconds(victoryDelay);

        LoadNextLevel();
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER. Insert Coin to Play Again.");
        playerLives = 3;
        score = 0;
        currentSequenceIndex = 0;

        SceneManager.LoadScene(0); 
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