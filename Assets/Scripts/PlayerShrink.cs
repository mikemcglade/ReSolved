using UnityEngine;
using System.Collections;

public class PlayerShrink : MonoBehaviour
{
    [SerializeField] private Vector3 shrunkScale = new Vector3(1.2f, 0.08f, 1.2f);  // Wider than original, nearly flat
    [SerializeField] private float shrinkDuration = 0.3f;
    [SerializeField] private float shrunkDuration = 5.0f;
    [SerializeField] private float cooldownDuration = 5.0f;
    [SerializeField] private Material liquidMaterial;
    [SerializeField] private AudioClip shrinkSFX;
    [SerializeField] private AudioClip growSFX;
    [SerializeField] private ParticleSystem shrinkParticles;

    private Vector3 originalScale;
    private bool canShrink = true;
    private bool isShrunk = false;
    private MeshRenderer meshRenderer;
    private Material[] originalMaterials;
    private AudioSource audioSource;

    public bool IsShrunk { get { return isShrunk; } }

    private void Start()
    {
        originalScale = transform.localScale;
        shrinkParticles.Stop();

        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("PlayerShrink: MeshRenderer not found on child object.");
            return;
        }
        originalMaterials = meshRenderer.materials;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canShrink)
            StartCoroutine(ShrinkCoroutine());
    }

    private IEnumerator ShrinkCoroutine()
    {
        canShrink = false;
        isShrunk = true;

        if (shrinkSFX != null)
            audioSource.PlayOneShot(shrinkSFX);

        // Phase 1: squish down and overshoot outward
        // The player spreads wider than the final shrunk size before settling
        Vector3 squishPeak = new Vector3(shrunkScale.x * 1.3f, shrunkScale.y, shrunkScale.z * 1.3f);
        float squishDuration = shrinkDuration * 0.65f;
        float settleDuration = shrinkDuration * 0.35f;

        float elapsed = 0f;
        while (elapsed < squishDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, squishPeak, elapsed / squishDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Phase 2: settle to final shrunk scale
        elapsed = 0f;
        while (elapsed < settleDuration)
        {
            transform.localScale = Vector3.Lerp(squishPeak, shrunkScale, elapsed / settleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = shrunkScale;

        // Swap to liquid material across all material slots
        Material[] shrunkMaterials = new Material[meshRenderer.materials.Length];
        for (int i = 0; i < shrunkMaterials.Length; i++)
            shrunkMaterials[i] = liquidMaterial;
        meshRenderer.materials = shrunkMaterials;

        shrinkParticles.Play();

        yield return new WaitForSeconds(shrunkDuration);

        if (growSFX != null)
            audioSource.PlayOneShot(growSFX);

        shrinkParticles.Stop();

        // Restore original materials before growing back
        meshRenderer.materials = originalMaterials;

        // Grow back with a slight upward pop then settle
        Vector3 growPeak = new Vector3(originalScale.x * 1.05f, originalScale.y * 1.08f, originalScale.z * 1.05f);
        float growDuration = shrinkDuration * 0.6f;
        float growSettleDuration = shrinkDuration * 0.4f;

        elapsed = 0f;
        while (elapsed < growDuration)
        {
            transform.localScale = Vector3.Lerp(shrunkScale, growPeak, elapsed / growDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < growSettleDuration)
        {
            transform.localScale = Vector3.Lerp(growPeak, originalScale, elapsed / growSettleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;

        isShrunk = false;

        yield return new WaitForSeconds(cooldownDuration);
        canShrink = true;
    }
}
