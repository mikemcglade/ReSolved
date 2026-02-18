using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CollectableFlash : MonoBehaviour
{
    public static CollectableFlash Instance;

    [Header("Flash Settings")]
    [Tooltip("How long the flash takes to fade in and out")]
    public float fadeDuration = 0.3f;
    [Tooltip("How long the flash holds at peak before fading out")]
    public float holdDuration = 0.1f;
    [Tooltip("Peak alpha of the vignette - keep low so view isn't obscured")]
    [Range(0f, 1f)]
    public float peakAlpha = 0.6f;

    [Header("Colours")]
    public Color invincibilityColor = new Color(0f, 1f, 0.3f, 1f);  // Green
    public Color timeColor = new Color(0.2f, 0.5f, 1f, 1f);          // Blue
    public Color lifeLostColor = new Color(1f, 0.1f, 0.1f, 1f);      // Red

    private Image vignetteImage;
    private Coroutine flashCoroutine;
    private bool lowHealthVignetteActive = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        BuildVignetteImage();
    }

    private void BuildVignetteImage()
    {
        // Create a full-screen Image child that will be tinted and faded
        GameObject vignetteObj = new GameObject("VignetteFlash");
        vignetteObj.transform.SetParent(transform, false);

        RectTransform rt = vignetteObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        vignetteImage = vignetteObj.AddComponent<Image>();

        // Use a radial gradient sprite if available, otherwise solid colour
        // The image starts fully transparent
        vignetteImage.color = new Color(0f, 1f, 0.3f, 0f);

        // Ensure it doesn't block raycasts (so UI buttons etc still work)
        vignetteImage.raycastTarget = false;
    }

    /// <summary>
    /// Call this from any collectable to trigger a coloured flash.
    /// </summary>
    public void Flash(Color color)
    {
        // Don't flash if low health vignette is active - would clear it
        if (lowHealthVignetteActive) return;
        
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine(color));
    }

    // Convenience methods so collectables don't need to pass colours directly
    public void FlashInvincibility() => Flash(invincibilityColor);
    public void FlashTime() => Flash(timeColor);
    public void FlashLifeLost() => Flash(lifeLostColor);
    
    /// <summary>
    /// Activate persistent red vignette when player is on last life.
    /// </summary>
    public void ActivateLowHealthVignette()
    {
        if (lowHealthVignetteActive) return;
        
        lowHealthVignetteActive = true;
        StartCoroutine(LowHealthVignettePulse());
    }
    
    private IEnumerator LowHealthVignettePulse()
    {
        // Persistent pulsing red vignette
        while (lowHealthVignetteActive)
        {
            float pulse = 0.3f + Mathf.Sin(Time.time * 2f) * 0.1f; // Pulse between 0.2 and 0.4 alpha
            vignetteImage.color = new Color(lifeLostColor.r, lifeLostColor.g, lifeLostColor.b, pulse);
            yield return null;
        }
        
        vignetteImage.color = new Color(lifeLostColor.r, lifeLostColor.g, lifeLostColor.b, 0f);
    }

    private IEnumerator FlashRoutine(Color color)
    {
        // Fade in
        yield return StartCoroutine(FadeVignette(0f, peakAlpha, color, fadeDuration));

        // Hold briefly at peak
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        yield return StartCoroutine(FadeVignette(peakAlpha, 0f, color, fadeDuration));

        flashCoroutine = null;
    }

    private IEnumerator FadeVignette(float fromAlpha, float toAlpha, Color color, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / duration);
            vignetteImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        vignetteImage.color = new Color(color.r, color.g, color.b, toAlpha);
    }
}
