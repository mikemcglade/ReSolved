using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeCollectable : MonoBehaviour
{
    public float timeToAdd = 60f;
    public AudioClip collectSound;
    
    [Header("UI Prompt")]
    public GameObject promptUI;
    public float detectionRange = 5f;
    public float fadeSpeed = 3f;
    
    private Transform player;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = Camera.main;
        
        // Get or add CanvasGroup for fading
        if (promptUI != null)
        {
            canvasGroup = promptUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = promptUI.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f; // Start invisible
        }
    }

    void Update()
    {
        // Make prompt face camera (billboard effect)
        if (promptUI != null && mainCamera != null)
        {
            promptUI.transform.LookAt(mainCamera.transform);
            promptUI.transform.Rotate(0, 180, 0);
        }
        
        // Fade in/out based on distance to player
        if (player != null && promptUI != null && canvasGroup != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= detectionRange)
            {
                // Fade in
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
            }
            else
            {
                // Fade out
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CountdownTimer timer = FindObjectOfType<CountdownTimer>();
            if (timer != null)
            {
                timer.AddTime(timeToAdd);
                PlayCollectSound();
                Destroy(gameObject);
            }
        }
    }

    private void PlayCollectSound()
    {
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
    }
}
