using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShrinkTutorialHint : MonoBehaviour
{
    [Header("Hint Settings")]
    [Tooltip("How close the player needs to be before the hint fades in")]
    public float detectionRange = 8f;
    [Tooltip("How quickly the hint fades in and out")]
    public float fadeSpeed = 3f;

    [Header("Arrow Bounce")]
    [Tooltip("How far the arrow bobs up and down")]
    public float bounceHeight = 0.15f;
    [Tooltip("How fast the arrow bobs")]
    public float bounceSpeed = 2f;

    [Header("References")]
    [Tooltip("Assign the world-space Canvas child in the Inspector")]
    public Canvas hintCanvas;

    private Transform player;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;
    private RectTransform arrowRect;
    private bool isDismissed = false;
    private Vector3 arrowStartLocalPos;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = Camera.main;

        // Set up CanvasGroup for fading on the canvas root
        if (hintCanvas != null)
        {
            canvasGroup = hintCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = hintCanvas.gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;

            // Find the arrow child by name
            Transform arrowTransform = hintCanvas.transform.Find("Arrow");
            if (arrowTransform != null)
            {
                arrowRect = arrowTransform.GetComponent<RectTransform>();
                arrowStartLocalPos = arrowRect.localPosition;
            }
        }
    }

    void Update()
    {
        if (isDismissed) return;

        // Billboard - always face the camera
        if (mainCamera != null)
        {
            hintCanvas.transform.LookAt(mainCamera.transform);
            hintCanvas.transform.Rotate(0, 180, 0);
        }

        // Proximity fade
        if (player != null && canvasGroup != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            float targetAlpha = distance <= detectionRange ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }

        // Bounce arrow up and down
        if (arrowRect != null)
        {
            float offsetY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight * 100f;
            arrowRect.localPosition = arrowStartLocalPos + new Vector3(0f, offsetY, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDismissed)
        {
            StartCoroutine(DismissHint());
        }
    }

    private IEnumerator DismissHint()
    {
        isDismissed = true;

        // Fade out smoothly then disable
        while (canvasGroup.alpha > 0.01f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed * 2f);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
