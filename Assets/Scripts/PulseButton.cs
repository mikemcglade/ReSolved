using UnityEngine;

public class PulseButton : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float scaleMin = 0.95f;
    public float scaleMax = 1.05f;
    public float speed = 1.5f;

    [Header("Delay Settings")]
    public float startDelay = 0.5f; // Seconds before pulse begins

    private Vector3 originalScale;
    private bool isAnimating = false;
    private float delayTimer = 0f;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Wait for delay before starting pulse
        if (!isAnimating)
        {
            delayTimer += Time.unscaledDeltaTime;
            if (delayTimer >= startDelay)
            {
                isAnimating = true;
            }
            return;
        }

        float scale = Mathf.Lerp(scaleMin, scaleMax, (Mathf.Sin(Time.unscaledTime * speed) + 1f) / 2f);
        transform.localScale = originalScale * scale;
    }

    void OnDisable()
    {
        // Reset scale cleanly if button is hidden
        transform.localScale = originalScale;
        isAnimating = false;
        delayTimer = 0f;
    }
}
