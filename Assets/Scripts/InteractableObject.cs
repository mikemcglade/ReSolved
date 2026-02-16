using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private Renderer[] objectRenderers;
    public Color highlightColor = Color.yellow;
    private MaterialPropertyBlock propertyBlock;
    public GameObject interactionPanel1;
    private GameManager gameManager;
    private bool isMessageDisplayed = false;
    private bool hasBeenInteracted = false;
    public AudioClip interactSound;
    
    [Header("UI Prompt")]
    public GameObject promptUI; // Assign the prompt Canvas in Inspector
    public float detectionRange = 5f;
    public float fadeSpeed = 3f;
    
    // Flag to tell other scripts player is near interactable
    public static bool PlayerNearInteractable = false;
    private Camera mainCamera;
    private CanvasGroup promptCanvasGroup;

    private void Start()
    {
        objectRenderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        interactionPanel1.SetActive(false);
        gameManager = FindObjectOfType<GameManager>();
        
        mainCamera = Camera.main;
        
        // Set up prompt UI fading (like powerups)
        if (promptUI != null)
        {
            promptUI.SetActive(true); // Keep active, but invisible
            
            promptCanvasGroup = promptUI.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
            {
                promptCanvasGroup = promptUI.AddComponent<CanvasGroup>();
            }
            promptCanvasGroup.alpha = 0f; // Start invisible
        }
    }

    private void Update()
    {
        // Make prompt face camera (billboard effect)
        if (promptUI != null && mainCamera != null)
        {
            promptUI.transform.LookAt(mainCamera.transform);
            promptUI.transform.Rotate(0, 180, 0); // Flip to face camera correctly
        }
        
        // Fade prompt based on player distance (like powerups)
        if (promptUI != null && promptCanvasGroup != null)
        {
            if (isPlayerInRange && !hasBeenInteracted)
            {
                // Fade in
                promptCanvasGroup.alpha = Mathf.Lerp(promptCanvasGroup.alpha, 1f, fadeSpeed * Time.deltaTime);
            }
            else
            {
                // Fade out
                promptCanvasGroup.alpha = Mathf.Lerp(promptCanvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
            }
        }
        
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isMessageDisplayed)
        {
            ShowInteractionUI();
            PlayInteractSound();
        }
        else if (isMessageDisplayed && Input.GetKeyDown(KeyCode.E))
        {
            HideInteractionUI();
        }
    }

    private void ShowInteractionUI()
    {
        interactionPanel1.SetActive(true);
        isMessageDisplayed = true;
        gameManager.PauseGame();
        hasBeenInteracted = true;
        gameManager.ObjectInteracted(this);
    }

    private void HideInteractionUI()
    {
        interactionPanel1.SetActive(false);
        isMessageDisplayed = false;
        gameManager.ResumeGame();
        gameManager.InteractionComplete();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            PlayerNearInteractable = true;
            HighlightObject(true);
            // Prompt fading handled in Update()
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            PlayerNearInteractable = false;
            HighlightObject(false);
            // Prompt fading handled in Update()
        }
    }

    private void HighlightObject(bool highlight)
    {
        foreach (var renderer in objectRenderers)
        {
            renderer.GetPropertyBlock(propertyBlock);
            if (highlight)
            {
                propertyBlock.SetColor("_EmissionColor", highlightColor);
                renderer.material.EnableKeyword("_EMISSION");
            }
            else
            {
                propertyBlock.SetColor("_EmissionColor", Color.black);
                renderer.material.DisableKeyword("_EMISSION");
            }
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

     private void PlayInteractSound()
    {
        if (interactSound != null)
        {
            AudioSource.PlayClipAtPoint(interactSound, transform.position);
        }
    }
}
