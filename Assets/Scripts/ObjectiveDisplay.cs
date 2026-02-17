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
    
    private GameManager gameManager;
    private CanvasGroup startMessageCanvasGroup;

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
            
            // Show start message
            StartCoroutine(ShowStartMessage());
        }
        
        // Initialize counter
        UpdateRelicCounter();
    }

    void Update()
    {
        // Update counter every frame
        UpdateRelicCounter();
    }

    IEnumerator ShowStartMessage()
    {
        // Pause the game
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
        
        // Wait for player to press Space OR auto-continue after duration
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
        
        // Hide and resume game
        startMessagePanel.SetActive(false);
        Time.timeScale = 1f;
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
