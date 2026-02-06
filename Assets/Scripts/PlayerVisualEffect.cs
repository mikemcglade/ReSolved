using UnityEngine;
using System.Collections;

public class PlayerVisualEffect : MonoBehaviour
{
    public Color invincibilityColor = Color.yellow;
    public float blinkRate = 0.2f;
    private Renderer[] playerRenderers;
    private Material[][] originalMaterials;

    private void Start()
    {
        playerRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[playerRenderers.Length][];
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            originalMaterials[i] = playerRenderers[i].sharedMaterials;
        }
    }

    public void StartInvincibilityEffect(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(InvincibilityEffect(duration));
    }

    private IEnumerator InvincibilityEffect(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            SetMaterialColors(invincibilityColor);
            yield return new WaitForSeconds(blinkRate);
            ResetMaterialColors();
            yield return new WaitForSeconds(blinkRate);

            elapsedTime += blinkRate * 2;
        }

        ResetMaterialColors();
    }

    private void SetMaterialColors(Color color)
    {
        foreach (Renderer renderer in playerRenderers)
        {
            foreach (Material material in renderer.materials)
            {
                material.color = color;
            }
        }
    }

    private void ResetMaterialColors()
    {
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            playerRenderers[i].materials = originalMaterials[i];
        }
    }
}