using UnityEngine;
using TMPro;
using System.Collections;

public class ObjectiveDisplay : MonoBehaviour
{
    [Header("Start Message")]
    public GameObject startMessagePanel;
    public float messageDisplayDuration = 3f;
    public float messageFadeDuration = 0.5f;
    
    [Header("Persistent Counter")]
    public TextMeshProUGUI relicCounterText;
    
    [Header("Bounce Prompt")]
    public RectTransform promptText; // Assign "Press Space to Begin" text
    public float bounceHeight = 8f;
    public float bounceSpeed = 2f;
    
    // Blocks other scripts from using Space during start message
    public static bool IsShowingStartMessage = false;
    
    private GameManager gameManager;
    private CanvasGroup startMessageCanvasGroup;
    private Vector3 promptOriginalPosition;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        // Set up start message
        if (startMessagePanel != null)
        {
            startMessageCanvasGroup = startMessagePanel.GetComponent<CanvasGroup>();
            if (startMessageCanvasGroup == null)
            {
                startMessageCanvasGroup = startMessagePanel.AddComponent<CanvasGroup>();
            }
            
            // Store prompt original position for bounce
            if (promptText != null)
            {
                promptOriginalPosition = promptText.localPosition;
            }
            
            // Show start message
            StartCoroutine(ShowStartMessage());
        }
        
        // Initialize counter
        UpdateRelicCounter();
    }

    void Update()
    {
        // Bounce the prompt text while message is showing
        if (IsShowingStartMessage && promptText != null)
        {
            float bounce = Mathf.Sin(Time.unscaledTime * bounceSpeed) * bounceHeight;
            promptText.localPosition = promptOriginalPosition + new Vector3(0, bounce, 0);
        }
        
        // Update relic counter every frame
        UpdateRelicCounter();
    }

    IEnumerator ShowStartMessage()
    {
        // Block shooting and pause game
        IsShowingStartMessage = true;
        Time.timeScale = 0f;
        
        // Start invisible
        startMessageCanvasGroup.alpha = 0f;
        startMessagePanel.SetActive(true);
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < messageFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            startMessageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / messageFadeDuration);
            yield return null;
        }
        startMessageCanvasGroup.alpha = 1f;
        
        // Wait for Space OR auto-continue after duration
        float waitTime = 0f;
        bool spacePressed = false;
        
        while (waitTime < messageDisplayDuration && !spacePressed)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                spacePressed = true;
            }
            waitTime += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Fade out
        elapsed = 0f;
        while (elapsed < messageFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            startMessageCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / messageFadeDuration);
            yield return null;
        }
        startMessageCanvasGroup.alpha = 0f;
        
        // Reset prompt position
        if (promptText != null)
        {
            promptText.localPosition = promptOriginalPosition;
        }
        
        // Hide, resume game and unblock shooting
        startMessagePanel.SetActive(false);
        Time.timeScale = 1f;
        IsShowingStartMessage = false;
    }

    void UpdateRelicCounter()
    {
        if (relicCounterText != null && gameManager != null)
        {
            int collected = gameManager.GetCollectedItemsCount();
            relicCounterText.text = "Relics: " + collected + "/3";
        }
    }
}

