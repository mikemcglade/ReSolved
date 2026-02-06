using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject3 : MonoBehaviour
{
 private bool isPlayerInRange = false;
 private Renderer objectRenderer;
 private Material originalMaterial;
 public Material highlightMaterial; // highlight colour is assigned in the Inspector

public GameObject interactionPanel3; // Reference to the panel containing UI elements


    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ShowInteractionUI();
        }
    }

private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        originalMaterial = objectRenderer.material; // Store the original material

        interactionPanel3.SetActive(false);
    }

    private void ShowInteractionUI()
    {
        // Show the panel
        interactionPanel3.SetActive(true);


        // hide after 5 seconds

         StartCoroutine(HideAfterDelay());
        
        // Add additional interaction logic here (e.g., opening doors, picking up items)
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the tag "Player"
        {
            isPlayerInRange = true;
            HighlightObject(true); // Highlight when player enters range
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HighlightObject(false); // Remove highlight when player exits range
        }
    }

    private void HighlightObject(bool highlight)
    {
        if (highlight)
        {
            objectRenderer.material = highlightMaterial; // Change to highlight material
        }
        else
        {
            objectRenderer.material = originalMaterial; // Revert to original material
        }
    }

    private IEnumerator HideAfterDelay()
{
    yield return new WaitForSeconds(5f);
    HideInteractionUI();
}

        private void HideInteractionUI()
    {
        interactionPanel3.SetActive(false);
    }
}
