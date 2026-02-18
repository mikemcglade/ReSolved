using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private int score;
    private int lives = 3;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public bool isGameActive;
    public bool isPlayerInvincible = false;
    private Coroutine invincibilityCoroutine;
    public Image invincibilityTimerImage;
    public Image[] livesImages;
    private int maxLives = 3;

    public GameObject[] enemyPrefabs;
    public LayerMask wallLayer; // Assign wall layer in Inspector
    private float spawnRangeX = 16;
    private float spawnPosZ = 20;
    private float startDelay = 2;
    private float spawnInterval = 2.5f;
    private int maxSpawnAttempts = 10;
    private Coroutine spawnCoroutine;
    public GameObject restartButton;
    public GameObject gameOverScreen;
    private bool isPaused = false;
    public RainController rainController;
    private HashSet<InteractableObject> interactedObjects = new HashSet<InteractableObject>();


    private int totalInteractableObjects = 3;
    public GameObject levelCompleteScreen;
    private InteractableObject lastInteractedObject;
    private bool isWaitingForLastInteraction = false;
    // adds UI visual effect for powerup
    private PlayerVisualEffect playerVisualEffect;
    public AudioSource backgroundMusic;
    public AudioSource rainSoundEffect;
    public AudioClip backgroundMusicClip;
    public AudioClip rainSoundClip;
    public AudioClip criticalHealthSound;
    private int finalScore;
    public TextMeshProUGUI levelCompleteText;


    void Start()
    {
        score = 0;
        lives = maxLives;
        UpdateScore();
        UpdateLives();

        isGameActive = true;
        //restartButton.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        gameOverScreen.SetActive(false);

        interactedObjects = new HashSet<InteractableObject>();

        spawnCoroutine = StartCoroutine(SpawnEnemies());
        levelCompleteScreen.SetActive(false);

       // adds UI visual effect for powerup duration
        playerVisualEffect = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerVisualEffect>();
        rainController = GameObject.Find("RainEffect").GetComponent<RainController>();
        SetRainIntensity(0.5f); // Start with medium rain
        
        PlayBackgroundMusic();
        PlayRainSound();
    }

    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void SetRainIntensity(float intensity)
    {
        rainController.SetRainIntensity(intensity);
    }
    public void AddLives(int value)
    {
        lives = Mathf.Clamp(lives + value, 0, maxLives);
        UpdateLives();
        if (lives <= 0)
        {
            GameOver();
        }
        else if (value < 0)
        {
            CollectableFlash.Instance.FlashLifeLost();
            StartCoroutine(SlowMotionHit());
            
            // Check if this brings us to last life - activate persistent vignette
            if (lives == 1)
            {
                ActivateLowHealthVignette();
            }
        }
        else if (value > 0 && lives > 1)
        {
            // Gained a life and now above critical - deactivate vignette
            if (CollectableFlash.Instance != null)
            {
                CollectableFlash.Instance.DeactivateLowHealthVignette();
            }
        }
    }

    private void UpdateLives()
    {
        Debug.Log("Lives = " + lives);
        for (int i = 0; i < livesImages.Length; i++)
        {
            livesImages[i].gameObject.SetActive(i < lives);
        }
    }

    public void AddScore(int value)
    {
        score += value;
        UpdateScore();
    }

    private void UpdateScore()
    {
        scoreText.text = "Score: " + score;
        Debug.Log("Score = " + score);
    }

    public void GameOver()
    {
        gameOverText.gameObject.SetActive(true);
        restartButton.SetActive(true); // Show the restart button
        gameOverScreen.SetActive(true);

        isGameActive = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        Debug.Log("Game Over");
        // Waits 5 seconds before calling ReturnToStartScreen
        Invoke("ReturnToStartScreen", 5f); 

    }

    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(startDelay);

        while (isGameActive)
        {
            SpawnRandomEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomEnemy()
    {
        Vector3 spawnPos = Vector3.zero;
        bool validPosition = false;
        int attempts = 0;

        // Try to find a valid spawn position (not inside a wall)
        while (!validPosition && attempts < maxSpawnAttempts)
        {
            spawnPos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 1.1f, spawnPosZ);
            
            // Check if this position overlaps with a wall
            if (!Physics.CheckSphere(spawnPos, 0.5f, wallLayer))
            {
                validPosition = true;
            }
            
            attempts++;
        }

        // Only spawn if we found a valid position
        if (validPosition)
        {
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            Instantiate(enemyPrefabs[enemyIndex], spawnPos, enemyPrefabs[enemyIndex].transform.rotation);
        }
    }

    public void SetInvincibility(bool status)
    {
        isPlayerInvincible = status;
    }

    public void ActivateInvincibility(float duration)
    {
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }
        invincibilityCoroutine = StartCoroutine(InvincibilityTimer(duration));
        // adds UI colour change for player
        playerVisualEffect.StartInvincibilityEffect(duration);
    }

    private IEnumerator InvincibilityTimer(float duration)
    {
        isPlayerInvincible = true;
        invincibilityTimerImage.gameObject.SetActive(true);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            invincibilityTimerImage.fillAmount = 1 - (elapsedTime / duration);
            yield return null;
        }

        isPlayerInvincible = false;
        invincibilityTimerImage.gameObject.SetActive(false);
        invincibilityCoroutine = null;
    }

public void RestartGame()
    {
        // Reload the current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

private void ReturnToStartScreen()
    {
        SceneManager.LoadScene("StartScreen"); 
    }

public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ObjectInteracted(InteractableObject interactedObject)
    {
    if (!interactedObjects.Contains(interactedObject))
    {
        interactedObjects.Add(interactedObject);
        lastInteractedObject = interactedObject;
        
        if (interactedObjects.Count >= totalInteractableObjects)
        {
            isWaitingForLastInteraction = true;
        }
    }
    }

    public int GetCollectedItemsCount()
    {
        return interactedObjects.Count;
    }

public void InteractionComplete()
    {
        if (isWaitingForLastInteraction)
        {
            LevelComplete();
        }
    }
    private void LevelComplete()
    {
        isGameActive = false;
        finalScore = score;
        levelCompleteScreen.SetActive(true);
        levelCompleteText.text = "Final Score: " + score;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        Debug.Log("Level Complete!");
        
        // Load the next level after a short delay
        Invoke("LoadNextLevel", 3f); // Adjust delay as needed
    }

    void PlayBackgroundMusic()
    {
       if (backgroundMusic != null)
        {
        backgroundMusic.clip = backgroundMusicClip;
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }
    }

    void PlayRainSound()
    {
      if (rainSoundEffect != null)
        {
     rainSoundEffect.clip = rainSoundClip;
            rainSoundEffect.loop = true;
            rainSoundEffect.Play();
        }
    }

    public void LoadNextLevel()
{
    int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    SceneManager.LoadScene(currentSceneIndex + 1);
}

    private IEnumerator SlowMotionHit()
    {
        // Brief slow-mo on damage for impact feel
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(0.15f);
        Time.timeScale = 1f;
    }
    
    private void ActivateLowHealthVignette()
    {
        // Play one-shot critical health sound
        if (criticalHealthSound != null && backgroundMusic != null)
        {
            backgroundMusic.PlayOneShot(criticalHealthSound);
        }
        
        // Persistent red vignette when on last life
        if (CollectableFlash.Instance != null)
        {
            CollectableFlash.Instance.ActivateLowHealthVignette();
        }
    }
}