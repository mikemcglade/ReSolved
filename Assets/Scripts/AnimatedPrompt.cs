using UnityEngine;
using TMPro;
using System.Collections;

public class AnimatedPrompt : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI promptText;
    public CanvasGroup canvasGroup;
    
    [Header("Fade Settings")]
    public float fadeDelay = 1.5f; // Wait before fading in
    public float fadeDuration = 0.5f;
    
    [Header("Bounce Settings")]
    public float bounceHeight = 10f;
    public float bounceSpeed = 2f;
    
    private Vector3 originalPosition;
    private bool isAnimating = false;

    void Start()
    {
        // Get components if not assigned
        if (promptText == null)
        {
            promptText = GetComponent<TextMeshProUGUI>();
        }
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Start invisible
        canvasGroup.alpha = 0f;
        
        // Store original position for bounce
        originalPosition = transform.localPosition;
    }

    void OnEnable()
    {
        // Ensure CanvasGroup exists before using it
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Ensure originalPosition is set
        if (originalPosition == Vector3.zero)
        {
            originalPosition = transform.localPosition;
        }
        
        // Reset and start animation when panel opens
        canvasGroup.alpha = 0f;
        transform.localPosition = originalPosition;
        StopAllCoroutines();
        StartCoroutine(AnimatePrompt());
    }

    void OnDisable()
    {
        // Stop animations when panel closes
        StopAllCoroutines();
        isAnimating = false;
    }

    IEnumerator AnimatePrompt()
    {
        isAnimating = true;
        
        // Wait before fading in
        yield return new WaitForSecondsRealtime(fadeDelay);
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // Start bouncing
        while (isAnimating)
        {
            float bounce = Mathf.Sin(Time.unscaledTime * bounceSpeed) * bounceHeight;
            transform.localPosition = originalPosition + new Vector3(0, bounce, 0);
            yield return null;
        }
    }
}
