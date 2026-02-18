using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public float totalTime = 80f; // Total time in seconds
    [SerializeField] TextMeshProUGUI timerText; // Reference to the UI Text component
    
    [Header("Timer Warning (< 30s)")]
    public AudioClip warningSound;
    public float warningThreshold = 30f;
    
    private float timeRemaining;
    private GameManager gameManager;
    private AudioSource audioSource;
    private bool isWarningActive = false;
    private RectTransform timerRect;
    private Vector3 originalTimerScale;

    void Start()
    {
        timeRemaining = totalTime;
        gameManager = FindObjectOfType<GameManager>();

        // Set up AudioSource for warning sound
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        
        // Store timer RectTransform for pulse effect
        if (timerText != null)
        {
            timerRect = timerText.GetComponent<RectTransform>();
            if (timerRect != null)
                originalTimerScale = timerRect.localScale;
        }
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
            
            // Activate warning when time drops below threshold
            if (timeRemaining <= warningThreshold && !isWarningActive)
            {
                ActivateWarning();
            }
            
            // Pulse timer text when warning is active
            if (isWarningActive && timerRect != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.1f;
                timerRect.localScale = originalTimerScale * pulse;
            }
        }
        else
        {
            timeRemaining = 0;
            UpdateTimerDisplay();
            DeactivateWarning();
            Debug.Log("Time's up!");
            
            if (gameManager != null && gameManager.isGameActive)
            {
                gameManager.GameOver();
            }
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Add time as a collectable
    public void AddTime(float secondsToAdd)
    {
        timeRemaining += secondsToAdd;
        UpdateTimerDisplay();
        
        // If time added pushes us back above threshold, stop warning
        if (timeRemaining > warningThreshold && isWarningActive)
        {
            DeactivateWarning();
        }
    }
    
    private void ActivateWarning()
    {
        isWarningActive = true;
        
        if (warningSound != null && audioSource != null)
        {
            audioSource.clip = warningSound;
            audioSource.Play();
        }
    }
    
    private void DeactivateWarning()
    {
        if (isWarningActive && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        isWarningActive = false;
        
        // Reset timer scale
        if (timerRect != null)
            timerRect.localScale = originalTimerScale;
    }
}