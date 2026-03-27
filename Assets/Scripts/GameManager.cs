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
    public LayerMask wallLayer;
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
    private PlayerVisualEffect playerVisualEffect;
    public AudioSource backgroundMusic;
    public AudioSource rainSoundEffect;
    public AudioClip backgroundMusicClip;
    public AudioClip rainSoundClip;
    public AudioClip criticalHealthSound;
    private int finalScore;
    public TextMeshProUGUI levelCompleteText;

    [Header("Game Over HUD")]
    public GameObject minimapBorder;
    public GameObject relicCounter;

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

    void Start()
    {
        score = 0;
        lives = maxLives;
        UpdateScore();
        UpdateLives();

        isGameActive = true;
        gameOverText.gameObject.SetActive(false);
        gameOverScreen.SetActive(false);

        interactedObjects = new HashSet<InteractableObject>();

        spawnCoroutine = StartCoroutine(SpawnEnemies());
        levelCompleteScreen.SetActive(false);

        playerVisualEffect = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerVisualEffect>();
        rainController = GameObject.Find("RainEffect").GetComponent<RainController>();
        SetRainIntensity(0.5f);

        PlayBackgroundMusic();
        PlayRainSound();
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

            if (lives == 1)
            {
                ActivateLowHealthVignette();
            }
        }
        else if (value > 0 && lives > 1)
        {
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
        // Clear vignette before showing game over screen
        if (CollectableFlash.Instance != null)
        {
            CollectableFlash.Instance.DeactivateLowHealthVignette();
        }

        // Hide in-game HUD elements
        if (minimapBorder != null) minimapBorder.SetActive(false);
        if (relicCounter != null) relicCounter.SetActive(false);

        gameOverText.gameObject.SetActive(true);
        restartButton.SetActive(true);
        gameOverScreen.SetActive(true);

        isGameActive = false;
        PauseGame();

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        Debug.Log("Game Over");
        StartCoroutine(FadeOutMusic(7f));
        StartCoroutine(ReturnToStartScreenDelayed());
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        if (backgroundMusic == null) yield break;

        float startVolume = backgroundMusic.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        backgroundMusic.volume = 0f;
        backgroundMusic.Stop();
        backgroundMusic.volume = startVolume; // Reset volume for safety if scene reloads
    }

    private IEnumerator ReturnToStartScreenDelayed()
    {
        yield return new WaitForSecondsRealtime(10f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("StartScreen");
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

        while (!validPosition && attempts < maxSpawnAttempts)
        {
            spawnPos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), 1.1f, spawnPosZ);

            if (!Physics.CheckSphere(spawnPos, 0.5f, wallLayer))
            {
                validPosition = true;
            }

            attempts++;
        }

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
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
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

        // Hide in-game HUD elements
        if (minimapBorder != null) minimapBorder.SetActive(false);
        if (relicCounter != null) relicCounter.SetActive(false);

        levelCompleteScreen.SetActive(true);
        levelCompleteText.text = "Final Score: " + score;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        Debug.Log("Level Complete!");
        Invoke("LoadNextLevel", 3f);
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
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(0.15f);
        Time.timeScale = 1f;
    }

    private void ActivateLowHealthVignette()
    {
        if (criticalHealthSound != null && backgroundMusic != null)
        {
            backgroundMusic.PlayOneShot(criticalHealthSound);
        }

        if (CollectableFlash.Instance != null)
        {
            CollectableFlash.Instance.ActivateLowHealthVignette();
        }
    }
}
