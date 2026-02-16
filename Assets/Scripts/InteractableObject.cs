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
    
    // Flag to tell other scripts player is near interactable
    public static bool PlayerNearInteractable = false;
    private Camera mainCamera;

    private void Start()
    {
        objectRenderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        interactionPanel1.SetActive(false);
        gameManager = FindObjectOfType<GameManager>();
        
        mainCamera = Camera.main;
        
        // Hide prompt UI at start
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void Update()
    {
        // Make prompt face camera (billboard effect)
        if (promptUI != null && promptUI.activeSelf && mainCamera != null)
        {
            promptUI.transform.LookAt(mainCamera.transform);
            promptUI.transform.Rotate(0, 180, 0); // Flip to face camera correctly
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
            
            // Show prompt UI
            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            PlayerNearInteractable = false;
            HighlightObject(false);
            
            // Hide prompt UI
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
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
