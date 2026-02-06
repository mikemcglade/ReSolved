using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    public float totalTime = 80f; // Total time in seconds
    [SerializeField] TextMeshProUGUI timerText; // Reference to the UI Text component
    private float timeRemaining;
    private GameManager gameManager;

    void Start()
    {
        timeRemaining = totalTime;
        gameManager = FindObjectOfType<GameManager>();

    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            timeRemaining = 0;
            UpdateTimerDisplay();
            Debug.Log("Time's up!");
            // Add any game-over logic here
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
    }
}